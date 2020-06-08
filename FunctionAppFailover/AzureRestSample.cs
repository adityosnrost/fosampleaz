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
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace FunctionAppFailover
{
    public static class AzureRestSample
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

        private static async Task<string> GetResourceGroupAsync(string token, ConfigWrapper config)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + config.SubscriptionId + "/resourcegroups?api-version=2017-05-10");
            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);

            return response.Content.ReadAsStringAsync().Result.ToString();
        }

        private static async Task<bool> GetFailoverGroupAsync(string token, string name, ConfigWrapper config)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + config.SubscriptionId + "/resourceGroups/" + config.SecondaryResourceGroupName + "/providers/Microsoft.Logic/workflows/" + config.WorkflowName + "/runs?api-version=2016-06-01&$top=1&$filter=startTime ge " + DateTime.UtcNow.AddMinutes(-int.Parse(config.InvokeInterval)).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"));

            var request = new HttpRequestMessage(HttpMethod.Get, "");
            request.Headers.Add("Authorization", "Bearer " + token);
            var response = await client.SendAsync(request);

            dynamic responseContent = await response.Content.ReadAsAsync<object>();
            JArray rowsResult = responseContent.value;

            if (rowsResult.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        [FunctionName("AzureRestSample")]
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

            string name = req.Query["serverName"].ToString().ToLower();
            if (name == null)
            {
                name = config.OriginServerRegion;
            }

            string token = GetBearerToken(config);

            bool rgResult = await GetFailoverGroupAsync(token, name, config);

            return new OkObjectResult(rgResult);
        }
    }
}
