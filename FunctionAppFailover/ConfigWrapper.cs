using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;

namespace FunctionAppFailover
{
    public class ConfigWrapper
    {
        private readonly IConfiguration _config;

        public ConfigWrapper(IConfiguration config)
        {
            _config = config;
        }

        public string TenantId
        {
            get { return _config["tenant_id"]; }
        }

        public string GrantType
        {
            get { return _config["grant_type"]; }
        }

        public string ClientId
        {
            get { return _config["client_id"]; }
        }

        public string ClientSecret
        {
            get { return _config["client_secret"]; }
        }

        public string Resource
        {
            get { return _config["resource"]; }
        }

        public string SubscriptionId
        {
            get { return _config["SubscriptionId"]; }
        }

        public string OriginEmailAddress
        {
            get { return _config["OriginEmailAddress"]; }
        }

        public string TargetEmailAddress
        {
            get { return _config["TargetEmailAddress"]; }
        }

        public string SendGridKey
        {
            get { return _config["SendGridKey"]; }
        }

        public string OriginServerRegion
        {
            get { return _config["OriginServerRegion"]; }
        }

        public string TargetServerRegion
        {
            get { return _config["TargetServerRegion"]; }
        }

        public string SendEmailFunctionURL
        {
            get { return _config["SendEmailFunctionURL"]; }
        }

        public string SendEmailLogicURL
        {
            get { return _config["SendEmailLogicURL"]; }
        }

        public string InvokeFailoverFunctionURL
        {
            get { return _config["InvokeFailoverFunctionURL"]; }
        }

        public string PrimaryWebName
        {
            get { return _config["PrimaryWebName"]; }
        }

        public string SecondaryWebName
        {
            get { return _config["SecondaryWebName"]; }
        }

        public string FailoverFunctionURL
        {
            get { return _config["FailoverFunctionURL"]; }
        }

        public string PrimaryResourceGroupName
        {
            get { return _config["PrimaryResourceGroupName"]; }
        }

        public string SecondaryResourceGroupName
        {
            get { return _config["SecondaryResourceGroupName"]; }
        }

        public string PrimaryDBServerName
        {
            get { return _config["PrimaryDBServerName"]; }
        }

        public string SecondaryDBServerName
        {
            get { return _config["SecondaryDBServerName"]; }
        }

        public string DBName
        {
            get { return _config["DBName"]; }
        }

        public string EnvironmentTriggerFailoverVariable
        {
            get { return _config["EnvironmentTriggerFailoverVariable"]; }
        }
    }
}
