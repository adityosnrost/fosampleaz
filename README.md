# Azure Web and Database Failover Automation

This repository contains projects that show how to do Azure failover on Web and Database when one of the region gone down.
This solution using couple of Azure services such as Traffic Manager, EventHub, Steam Analytics, Azure Function and Logic App.

This repository can be accessed directly using https://github.com/adityosnrost/fosampleaz.

## Prerequisites for failover project

### 1. Login or Create an account

Create a Azure account in your if don't have it already ([Login/Register Azure Account](https://azure.microsoft.com/en-us/free/)).

### 2. Fork this repository

Fork this repo, and you will start editing your own version of this projects. ([What is Fork?](https://help.github.com/en/github/getting-started-with-github/fork-a-repo)).

## Create and deploy full scenario failover solution services

### 1. Create AAD App Client and Retreived Key 

First, We need to create AAD App Client and retreived several information that we need to add on appsettings later. These are some value that we need to get:
tenant_id, grant_type, client_id, client_secret

Please go through this link tutorial to get required information to be added later: https://blog.jongallant.com/2017/11/azure-rest-apis-postman/

### 2. Add App Client information to Appsettings file 

Add information that we got from ealier step into your appsettings.json files on FunctionAppFailover folder. We need this to be setup, so our application can access resources at Azure

### 3. Deploy Based Instances

At this step, we are deploying these instances:
2 Web Apps, 2 Database with Failover, Traffic Management, Event Hub, Storage, Logic App, and Function App.

Click below button to automatically deploy your instances using Azure ARM Template:
<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

You can check the web project here ([Dotnet Sample Project](https://github.com/adityosnrost/fosampleaz/tree/master/WebApiDrDemoCS)).

### 3. Deploy final Instances and manage your email account on Logic App

After we have apps and database instances, we need to add Streaming Analytics to finalized our end to end auto failover for this project.
Why Streaming Analytics is seperated from template in step 3? There are some limitation on function listkey to get our functions app listkey added into Streaming Analytics. Functions need to be fully deployed with source control to finished.

Click below button to automatically deploy your instances using Azure ARM Template:
<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

After deployment is successful, then we config logic app office 365 connectors. Open your Azure portal, go to Logic App deployment. Click on app designer and manage the office 365 connector to use your email domain.

## NOTE

If not already done : fork the repo (IMPORTANT!).

Make sure that your changes is commited and push into your repository. Then deploy the solution using below deploy to azure button.

Note : if you never provided your GitHub account in the Azure portal before, the continuous integration probably will probably fail and you won't see the functions. In that case, you need to setup it manually. Go to your azure functions deployment / Functions app settings / Configure continuous integration. Select GitHub as a source and configure it to use your fork.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>
