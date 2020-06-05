using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Azure.WebJobs.Host;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;

namespace FunctionAppFailover
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static async Task<IActionResult> RunAsync(
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

            var apiKey = config.SendGridKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(config.OriginEmailAddress, "Alert Admin");
            List<EmailAddress> tos = new List<EmailAddress>
            {
                new EmailAddress(config.TargetEmailAddress, "System Administrator")
            };
            var subject = "System down: Failover need to be approved";
            var htmlContent = "<p>The Traffic Manager detected failure on the Web frontend " + config.PrimaryWebName + " on "+ DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + ", (UTC Time)" +
                "as such we are failing over to " + config.SecondaryWebName + ". Copy following URL into your browser to invoke the failover: " +
                "</p>" +
                "<p><b>" + config.FailoverFunctionURL + "</b></p>";
            var displayRecipients = false; // set this to true if you want recipients to see each others mail id 
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
            var response = await client.SendEmailAsync(msg);

            return new OkObjectResult("Enail is successfully sent");
        }
    }
}
