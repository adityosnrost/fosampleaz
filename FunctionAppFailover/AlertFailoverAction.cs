using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using RestSharp;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;


namespace FunctionAppFailover
{
    public static class AlertFailoverAction
    {
        static RestClient clientRest = null;

        private static string GetBearerToken(ConfigWrapper config)
        {
            clientRest = new RestClient("https://login.microsoftonline.com/" + config.TenantId + "/oauth2/token");
            clientRest.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("grant_type", config.GrantType);
            request.AddParameter("client_id", config.ClientId);
            request.AddParameter("client_secret", config.ClientSecret);
            request.AddParameter("resource", config.Resource);
            IRestResponse response = clientRest.Execute(request);
            dynamic jo = JsonConvert.DeserializeObject(response.Content);
            Console.WriteLine(response.Content);
            string token = jo.access_token.Value;
            if (!string.IsNullOrEmpty(token))
                return token;
            else
                return null;
        }

        private static async Task<string> GetFailoverKey(string token)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Web/sites/" + Environment.GetEnvironmentVariable("FunctionDeployName") + "/functions/invokefailover/listkeys?api-version=2019-08-01");
            var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);

            dynamic responseContent = await response.Content.ReadAsAsync<object>();

            return responseContent["default"];
        }

        private static async Task<string> GetFailoverGroupAsync(string token, string name, ConfigWrapper config)
        {
            var client = new HttpClient();
            if (name == Environment.GetEnvironmentVariable("TargetServerRegion"))
            {
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Sql/servers/" + Environment.GetEnvironmentVariable("SecondaryDBServerName") + "/failoverGroups/" + Environment.GetEnvironmentVariable("FailovergroupName") + "?api-version=2015-05-01-preview");
            }
            else
            {
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Sql/servers/" + Environment.GetEnvironmentVariable("PrimaryDBServerName") + "/failoverGroups/" + Environment.GetEnvironmentVariable("FailovergroupName") + "?api-version=2015-05-01-preview");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);

            dynamic responseContent = await response.Content.ReadAsAsync<object>();

            return responseContent.properties.replicationRole;
        }

        private static async Task<string> TriggerFailover(ConfigWrapper config, string token)
        {
            string functionKey = await GetFailoverKey(token);

            var client2 = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("InvokeFailoverFunctionURL") + functionKey.ToString());
            var response = await client2.SendAsync(request);

            return response.Content.ReadAsStringAsync().Result.ToString();
        }

        private static async Task<string> TriggerSendEmail(ConfigWrapper config, string token)
        {
            string functionKey = await GetFailoverKey(token);

            var postBody = (dynamic)new JsonObject();
            postBody.FailoverFunctionURL = Environment.GetEnvironmentVariable("InvokeFailoverFunctionURL") + functionKey.ToString();
            postBody.PrimaryWebName = Environment.GetEnvironmentVariable("PrimaryWebName");
            postBody.SecondaryWebName = Environment.GetEnvironmentVariable("SecondaryWebName");

            var client2 = new HttpClient();
            var content = new StringContent(postBody.ToString(), Encoding.UTF8, "application/json");
            var result = await client2.PostAsync(Environment.GetEnvironmentVariable("SendEmailLogicURL"), content);

            return "Email has sent successfully";
        }

        private static async Task<bool> GetEmailInvocationInterval(string token, ConfigWrapper config)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Logic/workflows/" + Environment.GetEnvironmentVariable("WorkflowName") + "/runs?api-version=2016-06-01&$top=1&$filter=startTime ge " + DateTime.UtcNow.AddMinutes(-int.Parse(config.InvokeInterval)).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"));

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);

            dynamic responseContent = await response.Content.ReadAsAsync<object>();
            JArray rowsResult = responseContent.value;

            if(rowsResult.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        [FunctionName("AlertFailoverAction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var currentDirectory = "/home/site/wwwroot";
            bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                currentDirectory = Directory.GetCurrentDirectory();
            }

            ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build());

            string token = GetBearerToken(config);
            string name = Environment.GetEnvironmentVariable("TargetServerRegion");
            string rgResult = await GetFailoverGroupAsync(token, name, config);
            string actionResult;

            if (rgResult != "Primary")
            {
                bool isEmailInterval = await GetEmailInvocationInterval(token, config);

                string failoverActionType = System.Environment.GetEnvironmentVariable(config.EnvironmentTriggerFailoverVariable, EnvironmentVariableTarget.Process);
                if (failoverActionType == "1")
                {
                    actionResult = await TriggerFailover(config, token);
                }
                else if (failoverActionType == "0" && isEmailInterval == false)
                {
                    actionResult = await TriggerSendEmail(config, token);
                }
                else
                {
                    actionResult = "No action needed, either email already sent with in interval or failover is done";
                }
            }
            else
            {
                actionResult = "No action needed, failover successfully done";
            }

            return new OkObjectResult(actionResult);
        }
    }
}
