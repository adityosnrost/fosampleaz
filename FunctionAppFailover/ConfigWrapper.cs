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

        public string OriginEmailServer
        {
            get { return _config["OriginEmailServer"]; }
        }

        public string OriginEmailServerPort
        {
            get { return _config["OriginEmailServerPort"]; }
        }

        public string OriginEmailAddress
        {
            get { return _config["OriginEmailAddress"]; }
        }

        public string OriginEmailPassword
        {
            get { return _config["OriginEmailPassword"]; }
        }

        public string TargetEmailAddress
        {
            get { return _config["TargetEmailAddress"]; }
        }

        public string SendGridKey
        {
            get { return _config["SendGridKey"]; }
        }
    }
}
