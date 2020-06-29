# Azure Web and Database Failover Automation

This repository contains projects that show how to do Azure failover on Web and Database when one of the region gone down.
This solution using couple of Azure services such as Traffic Manager, EventHub, Steam Analytics, Azure Function and Logic App.

This repository can be accessed directly using https://github.com/adityosnrost/samplefailover.

## Prerequisites for failover project

### 1. Login or Create an account

Create a Azure account in your if don't have it already ([Login/Register Azure Account](https://azure.microsoft.com/en-us/free/)).

### 2. Fork this repository

Fork this repo, and you will start editing your own version of this projects. ([What is Fork?](https://help.github.com/en/github/getting-started-with-github/fork-a-repo)).

## Create and deploy full scenario failover solution services

### 1. Deploy Web Apps and Databases with failover resources 

First, We deploy web apps and databases instances that setup with failover feature. 
These setup include traffic manager to manage web apps failover and failover group on these deployed DB.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

### 2. Deploy Application and Databases (Optional, but recommended)

Deploy web with database project that you have or using sample project from this repository. 
You can check the web project here ([Dotnet Sample Project](https://github.com/adityosnrost/samplefailover/tree/master/WebApiDrDemoCS)).

### 3. Deploy and config alert solution

We need to config traffic manager to send information of current status into event hub and monitored by stream analytics.
First, we deploy event hub and stream analytics through this template button below:

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

After deployment is successful, then we config event hub to capture traffic manager event.

. . . . . .

### 3. Deploy the failover solution services

If not already done : fork the repo (IMPORTANT!).

This part, we will deploy Functions, and Logic App using ARM deployment template below. 
Please check appsettings.json and match them with your deployment on Azure. This setting is very important to be set up. 
Those setup is for running scripts on web failover event that occured and sending alert / do failover on DB.

Make sure that your changes is commited and push into your repository. Then deploy the solution using below deploy to azure button.

Note : if you never provided your GitHub account in the Azure portal before, the continuous integration probably will probably fail and you won't see the functions. In that case, you need to setup it manually. Go to your azure functions deployment / Functions app settings / Configure continuous integration. Select GitHub as a source and configure it to use your fork.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>
