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
using System.Linq;

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

            List<EmailAddress> tos = new List<EmailAddress>();
            List<string> toEmails = new List<string>();

            toEmails = config.TargetEmailAddress.Split(';').ToList();
            foreach (var toEmail in toEmails)
            {
                tos.Add(new EmailAddress(toEmail.Trim()));
            }
            
            var subject = "System down: Database Failover need to be approved";
            var htmlContent = "<p>The Traffic Manager detected failure on the Web frontend <strong>" + config.PrimaryWebName + "</strong> on "+ DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss") + ", (UTC Time)." +
                " As such we are failing over to <strong>" + config.SecondaryWebName + "</strong> automatically. " +
                "<br /> Please click the <a href="+config.FailoverFunctionURL+ ">following link</a> to initiave the <strong>database failover</strong>. Alternatively, you may copy following URL into your browser: " +
                "</p>" +
                "<p><b>" + config.FailoverFunctionURL + "</b></p>";
            
            //var displayRecipients = false; // set this to true if you want recipients to see each others mail id 
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
            var response = await client.SendEmailAsync(msg);

            return new OkObjectResult("Email has been successfully sent.");
        }
    }
}
