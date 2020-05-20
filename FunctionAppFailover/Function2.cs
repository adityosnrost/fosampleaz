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
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace FunctionAppFailover
{
    public static class Function2
    {
        private static HttpClient Client = new HttpClient();
        public static bool POSTData(string json, string url)
        {
            //using (var content = new StringContent(JsonConvert.SerializeObject(json), System.Text.Encoding.UTF8, "application/json"))
            //{
            using (var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage result = Client.PostAsync(url, content).Result;
                if (result.StatusCode == System.Net.HttpStatusCode.Created)
                    return true;
                string returnValue = result.Content.ReadAsStringAsync().Result;
                throw new Exception($"Failed to POST data: ({result.StatusCode}): {returnValue}");
            }
        }
        [FunctionName("Function2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string json = "{ \"grant_type\":\"client_credentials\", \"client_id\":\"1a144c3d-9627-4ffd-8547-f684b3b5f928\", \"client_secret\":\"296f496a-c727-4410-b3e3-8950397bc197\" }";;


            const string tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";
            string URL = "https://login.microsoftonline.com/";
            string baseUrl = URL + tenantId + "/oauth2/token/";


            //var myObject = (dynamic)new JObject();
            //myObject.grant_type = "client_credentials";
            //myObject.client_id = "1a144c3d-9627-4ffd-8547-f684b3b5f928";
            //myObject.client_secret = "296f496a-c727-4410-b3e3-8950397bc197";

            //HttpRequestMessage msg = new HttpRequestMessage();
            //msg.RequestUri = new Uri(baseUrl);
            //msg.Method = HttpMethod.Post;

            //var content = new StringContent(myObject.ToString(), Encoding.UTF8, "multipart/form-data");
            //msg.Content = content;


            using (var multiPartStream = new MultipartFormDataContent())
            {

                multiPartStream.Add(new StringContent("client_credentials"), "grant_type");
                multiPartStream.Add(new StringContent("1a144c3d-9627-4ffd-8547-f684b3b5f928"), "grant_typeclient_id");
                multiPartStream.Add(new StringContent("296f496a-c727-4410-b3e3-8950397bc197"), "client_secret");
                //multiPartStream.Add(new ByteArrayContent(filecontent, 0, filecontent.Length), "file", docName);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
                request.Content = multiPartStream;
                //"application/json" - content type
                //request.Headers.Accept.Add(JSON_GENERIC_MEDIA_TYPE);
                //request.Headers.Add("X-Client-Id", ClientId);
                //request.Headers.Add("Tenant-Id", TenantId);

                HttpCompletionOption option = HttpCompletionOption.ResponseContentRead;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                using (HttpResponseMessage response = Client.SendAsync(request, option).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var deserializedObject = JsonConvert.DeserializeObject<string>(response.Content.ReadAsStringAsync().Result);
                        string test = deserializedObject;
                    }
                }

            }//End Try





            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //HttpResponseMessage response = Client.SendAsync(msg).Result;
            //(msg, content).Result;


            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Client.BaseAddress);
            //request.Content = new StringContent(body,
            //                                        Encoding.UTF8,
            //                                        "application/x-www-form-urlencoded");
            //List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

            //postData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            //postData.Add(new KeyValuePair<string, string>("client_id", "1a144c3d-9627-4ffd-8547-f684b3b5f928"));
            //postData.Add(new KeyValuePair<string, string>("client_secret", "296f496a-c727-4410-b3e3-8950397bc197")); 

            //Console.WriteLine("Starting connections");
            //for (int i = 0; i < 10; i++)
            //{
            //    var result = await Client.PostAsync("https://httpbin.org/post", data);
            //        //await Client.GetAsync("http://aspnetmonsters.com");
            //    Console.WriteLine(result.StatusCode);
            //}
            Console.WriteLine("Connections done");






            #region hide

            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            #endregion
            return new OkObjectResult(responseMessage);
        }
    }
}
