# Implementing multi-region deployment and disaster recovery in Azure

This repository complements this [video series](https://www.youtube.com/playlist?list=PLe32w3jNLanrZ9_X58_3d13tQ5REMmnK3) where we demonstrated  how to perform a failover on Web/App and Database when one of the region goes down.
This solution uses a number of Azure services such as App Service, SQL Datbase, Traffic Manager, EventHub, Steam Analytics, Azure Function and Logic App.

We aim to make this deployment and configuration process as simple as possible.

This project contains:

1. Source codes used in the demo which includes the [sample web api project](SourceCode/WebApiDrDemoCS/) built-with .NET and Visual Studio as well as [Azure Function project](SourceCode/FunctionAppFailover/).
2. ARM Deployment template which you can easily deploy entire solution to Azure with just a SINGLE CLICK!

<!-- # How to perform SINGLE CLICK deployment -->

## Prerequisites

### 1. Login or Create an account

Create a Azure account in your if don't have one ([Login/Register Azure Account](https://azure.microsoft.com/en-us/free/)).

### 2. Fork this repository

Fork this repo, and you will start editing your own version of this projects. ([What is Fork?](https://help.github.com/en/github/getting-started-with-github/fork-a-repo)).

## Getting ready your necessary keys and secrets

In this step, you need to create AAD (Azure Active Directory) App Client and Retreived the keys

Firstly, we need to create AAD App Client and retreived several information such as **tenant_id, grant_type, client_id, client_secret**.

Please go through this link tutorial to get required information to be added later: https://blog.jongallant.com/2017/11/azure-rest-apis-postman/

**Ensure you keep those information securely and avoid exposing those to the public.** 

## One Click deployment

Now, we are deploying all necessary resources with just one unified deployment template.

Click below button to automatically deploy your instances using Azure ARM Template:

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fwely%2Ffosampleaz%2Fmaster%2FDeployment%2FazureDeployment.json" target="_blank"><img src="https://aka.ms/deploytoazurebutton"/></a>

Add those info / keys (**tenant_id, grant_type, client_id, client_secret**) that we got from ealier to your deployment paramter. They are primarily used as authorization keys to perform Database Failover programmatically thru our Azure Functions.

Most of the parameters have been prefilled with the default values. You may optionally update them to your preferred values.

Click Review + create button to proceed.

It takes about x minutes to deploy the solution.

## Post deployment configuration

After deployment is successful, then we config logic app office 365 connectors. Open your Azure portal, go to Logic App deployment. Click on app designer and manage the office 365 connector to use your email domain.

## NOTE

If not already done : fork the repo (IMPORTANT!).

Make sure that your changes is commited and push into your repository. Then deploy the solution using below deploy to azure button.

Note : if you never provided your GitHub account in the Azure portal before, the continuous integration probably will probably fail and you won't see the functions. In that case, you need to setup it manually. Go to your azure functions deployment / Functions app settings / Configure continuous integration. Select GitHub as a source and configure it to use your fork.
