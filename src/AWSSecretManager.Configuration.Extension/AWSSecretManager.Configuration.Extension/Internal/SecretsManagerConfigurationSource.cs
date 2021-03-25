using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
namespace SecretManager.ConfigurationExtension.Internal
{
    public class SecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly AmazonSecretsManagerClient _client;
        private readonly string _enviroment;
        private readonly string _project;
        public SecretsManagerConfigurationSource(string accessKeyId, string accessKeySecret, string region,string environment, string project)
        {
            if (string.IsNullOrEmpty(region))
            {
                region = RegionEndpoint.USEast2.SystemName;
            }
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };
            _enviroment = environment;
            _project = project;
            _client = new AmazonSecretsManagerClient(accessKeyId, accessKeySecret, config);
        }

        public SecretsManagerConfigurationSource(RegionEndpoint region, AWSCredentials credentials,string environment, string project)
        {
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = region
            };
            _enviroment = environment;
            _project = project;
            _client = new AmazonSecretsManagerClient(credentials, config);
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SecretsManagerConfigurationProvider(_client,_enviroment,_project);
        }
    }
}
