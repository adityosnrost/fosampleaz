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

namespace FunctionAppFailover
{
    public static class InvokeFailover
    {
        static RestClient client = null; 

        private static string GetBearerToken()
        {
            client = new RestClient("https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", "stsservicecookie=estsfd; fpc=AlsSOR2lIsxOqFLubHOBxlufnQOrAQAAAHB6VdYOAAAA; x-ms-gateway-slice=estsfd");
            request.AlwaysMultipartFormData = true;
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", "1a144c3d-9627-4ffd-8547-f684b3b5f928");
            request.AddParameter("client_secret", "296f496a-c727-4410-b3e3-8950397bc197");
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            dynamic jo = JsonConvert.DeserializeObject(response.Content);
            string token = jo.access_token.Value;
            if (!string.IsNullOrEmpty(token))
                return token;
            else
                return null;
        }

        private  static async Task<JObject> GetAADToken()
        {
            const string tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";
            string URL = "https://login.microsoftonline.com/" ;
            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(URL);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Add("grant_type", "client_credentials");
                //client.DefaultRequestHeaders.Add("client_id", "1a144c3d-9627-4ffd-8547-f684b3b5f928");
                //client.DefaultRequestHeaders.Add("client_secret", "296f496a-c727-4410-b3e3-8950397bc197");
                //client.DefaultRequestHeaders.Add("resource", "https://management.azure.com");
                //client.DefaultRequestHeaders.Add("subscriptionId", "86306f52-a93a-48f2-a3f2-d34b242a37c9");

                Uri uri = new Uri(URL + tenantId + "/oauth2/token");

                var payload = 
                    "{ \"grant_type\":\"client_credentials\", \"client_id\":\"1a144c3d-9627-4ffd-8547-f684b3b5f928\", \"client_secret\":\"296f496a-c727-4410-b3e3-8950397bc197\" }";
                HttpContent cont = new StringContent(payload, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await client.PostAsync(uri, cont)) //.Result;
                                                                                                               //client.PostAsJsonAsync(tenantId + "/oauth2/token", null).Result;
                {
                    using (HttpContent content = response.Content)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string returnValue = response.Content.ToString();
                            Console.Write(returnValue);

                            return null; // await Task.Run( () => JObject.Parse(content));
                        }
                        else
                            return null;
                    }
                }
                
            }

        }

        [FunctionName("InvokeFailover")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string token = GetBearerToken();

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");


            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
        //[FunctionName("InvokeFailover")]
        //public static void Run([EventHubTrigger("samples-workitems", Connection = "EventHubConnectionAppSetting")] string myEventHubMessage, ILogger log)
        //{
        //    log.LogInformation($"C# function triggered to process a message: {myEventHubMessage}");
        //}
    }
}
