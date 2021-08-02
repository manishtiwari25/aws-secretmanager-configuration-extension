using Amazon;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using System;
namespace SecretManager.ConfigurationExtension.Internal
{
    public class SecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly AmazonSecretsManagerClient _client;
        private readonly ushort _cacheSize;
        private readonly uint _cacheItemTTL;

        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the AWS Secret Manager.
        /// </summary>
        /// <param name="accessKeyId">AWS Access Key ID</param>
        /// <param name="accessKeySecret">AWS Secret Access Key</param>
        /// <param name="region"> The system name of the service like "us-west-1". The default value is us-east-2</param>
        /// <param name="cacheSize">The maximum number of items the Cache can contain before evicting using LRU. The default value is 1024.</param>
        /// <param name="cacheItemTTL">The TTL of a Cache item in milliseconds.The default value is 3600000 ms, or 1 hour</param>
        public SecretsManagerConfigurationSource(string accessKeyId, string accessKeySecret, string region = "us-east-2", ushort cacheSize = 1024, uint cacheItemTTL = 3600000u)
        {
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(region)
            };
            _client = new AmazonSecretsManagerClient(accessKeyId, accessKeySecret, config);
            _cacheSize = cacheSize;
            _cacheItemTTL = cacheItemTTL;
        }


        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var project = Environment.GetEnvironmentVariable("ASPNETCORE_PROJECT");
            return new SecretsManagerConfigurationProvider(_client, environment, project, _cacheSize, _cacheItemTTL);
        }
    }
}
