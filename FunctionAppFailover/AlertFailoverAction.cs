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
            string actionResult;
            string failoverActionType = System.Environment.GetEnvironmentVariable("FullAutomatedFailover", EnvironmentVariableTarget.Process);
            if (failoverActionType == "1")
            {
                actionResult = await TriggerFailover();
            }
            else if(failoverActionType == "0")
            {
                actionResult = await TriggerSendEmail();
            }
            else
            {
                actionResult = "No action needed";
            }

            return new OkObjectResult(actionResult);
        }
    }
}
