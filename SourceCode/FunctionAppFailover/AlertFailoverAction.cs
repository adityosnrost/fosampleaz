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

        private static string GetBearerToken()
        {
            clientRest = new RestClient("https://login.microsoftonline.com/" + Environment.GetEnvironmentVariable("tenant_id") + "/oauth2/token");
            clientRest.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("grant_type", Environment.GetEnvironmentVariable("grant_type"));
            request.AddParameter("client_id", Environment.GetEnvironmentVariable("client_id"));
            request.AddParameter("client_secret", Environment.GetEnvironmentVariable("client_secret"));
            request.AddParameter("resource", Environment.GetEnvironmentVariable("resource"));
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

        private static async Task<string> GetFailoverGroupAsync(string token, string name)
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

        private static async Task<string> TriggerFailover(string token)
        {
            string functionKey = await GetFailoverKey(token);

            var client2 = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("InvokeFailoverFunctionURL") + functionKey.ToString());
            var response = await client2.SendAsync(request);

            return response.Content.ReadAsStringAsync().Result.ToString();
        }

        private static async Task<string> TriggerSendEmail(string token)
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

        private static async Task<bool> GetEmailInvocationInterval(string token)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Logic/workflows/" + Environment.GetEnvironmentVariable("WorkflowName") + "/runs?api-version=2016-06-01&$top=1&$filter=startTime ge " + DateTime.UtcNow.AddMinutes(-int.Parse(Environment.GetEnvironmentVariable("InvokeInterval"))).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"));

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

            string token = GetBearerToken();
            string name = Environment.GetEnvironmentVariable("TargetServerRegion");
            string rgResult = await GetFailoverGroupAsync(token, name);
            string actionResult;

            if (rgResult != "Primary")
            {
                bool isEmailInterval = await GetEmailInvocationInterval(token);

                string failoverActionType = Environment.GetEnvironmentVariable("EnvironmentTriggerFailoverVariable");
                if (failoverActionType == "1")
                {
                    actionResult = await TriggerFailover(token);
                }
                else if (failoverActionType == "0" && isEmailInterval == false)
                {
                    actionResult = await TriggerSendEmail(token);
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
