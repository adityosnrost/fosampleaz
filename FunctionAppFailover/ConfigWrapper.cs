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
    }
}
