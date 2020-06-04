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
            clientRest = new RestClient("https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/token");
            clientRest.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", "stsservicecookie=estsfd; fpc=AlsSOR2lIsxOqFLubHOBxlufnQOrAQAAAHB6VdYOAAAA; x-ms-gateway-slice=estsfd");
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

        private static async Task<string> GetFailoverGroupAsync(string token, string name, ConfigWrapper config)
        {
            var client = new HttpClient();

            if (name == "eastus")
            {
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + config.SubscriptionId + "/resourceGroups/rgDrDemoEUS/providers/Microsoft.Sql/servers/productservereus/failoverGroups/productdbgroup?api-version=2015-05-01-preview");
            }
            else
            {
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + config.SubscriptionId + "/resourceGroups/rgDrDemo/providers/Microsoft.Sql/servers/productserversea/failoverGroups/productdbgroup?api-version=2015-05-01-preview");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);

            dynamic responseContent = await response.Content.ReadAsAsync<object>();

            return responseContent.properties.replicationRole;
        }



        private static async Task<string> TriggerFailover()
        {
            var client2 = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://failoverfunc.azurewebsites.net/api/InvokeFailover?code=9IUbzLP/NP6lituqDOILOWa9/N1IH2b9n3M1QUn9OWaIFmwII/8LIg==");
            var response = await client2.SendAsync(request);

            return response.Content.ReadAsStringAsync().Result.ToString();
        }

        private static async Task<string> TriggerSendEmail()
        {
            var client2 = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://failoverfunc.azurewebsites.net/api/SendEmail?code=XRuAhdRIBDaLHi8mo15EFRx4AqTIJ89N0GpMnEt7cgP9af3wfcVRmA==");
            var response = await client2.SendAsync(request);

            return response.Content.ReadAsStringAsync().Result.ToString();
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
            string name = "eastus";
            string rgResult = await GetFailoverGroupAsync(token, name, config);
            string actionResult;

            if (rgResult != "Primary")
            {
                string failoverActionType = System.Environment.GetEnvironmentVariable("FullAutomatedFailover", EnvironmentVariableTarget.Process);
                if (failoverActionType == "1")
                {
                    actionResult = await TriggerFailover();
                }
                else if (failoverActionType == "0")
                {
                    actionResult = await TriggerSendEmail();
                }
                else
                {
                    actionResult = "No action needed";
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
