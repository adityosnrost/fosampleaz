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
    public static class InvokeFailover
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

            return response.Content.ReadAsStringAsync().Result.ToString();
        }

        private static async Task<string> TriggerFailover(string token, string name)
        {
            var client2 = new HttpClient();
            if (name == Environment.GetEnvironmentVariable("TargetServerRegion"))
            {
                client2.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Sql/servers/" + Environment.GetEnvironmentVariable("SecondaryDBServerName") + "/failoverGroups/" + Environment.GetEnvironmentVariable("FailovergroupName") + "/failover?api-version=2015-05-01-preview");
            }
            else
            {
                client2.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + Environment.GetEnvironmentVariable("SubscriptionId") + "/resourceGroups/" + Environment.GetEnvironmentVariable("ResourceGroupName") + "/providers/Microsoft.Sql/servers/" + Environment.GetEnvironmentVariable("PrimaryDBServerName") + "/failoverGroups/" + Environment.GetEnvironmentVariable("FailovergroupName") + "/failover?api-version=2015-05-01-preview");
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client2.SendAsync(request);

            return response.Content.ReadAsStringAsync().Result.ToString();
        }

        [FunctionName("InvokeFailover")]
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

            string name = req.Query["serverName"].ToString().ToLower();
            if (name == null || name == "")
            {
                name = Environment.GetEnvironmentVariable("TargetServerRegion");
            }

            string token = GetBearerToken();
            string actionResult;
            actionResult = await TriggerFailover(token, name);

            return new OkObjectResult(actionResult);
        }
    }
}
