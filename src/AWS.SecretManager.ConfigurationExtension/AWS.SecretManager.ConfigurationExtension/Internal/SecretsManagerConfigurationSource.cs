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

        public SecretsManagerConfigurationSource(string accessKeyId, string accessKeySecret, string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                region = RegionEndpoint.USEast2.SystemName;
            }
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };
            _client = new AmazonSecretsManagerClient(accessKeyId, accessKeySecret, config);
        }

        public SecretsManagerConfigurationSource(RegionEndpoint region, AWSCredentials credentials)
        {
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = region
            };
            _client = new AmazonSecretsManagerClient(credentials, config);
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SecretsManagerConfigurationProvider(_client);
        }
    }
}
