using Microsoft.Extensions.Configuration;
using SecretManager.ConfigurationExtension.Internal;
using System;

namespace SecretManager.ConfigurationExtension
{
    public static class Extension
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the AWS Secret Manager.
        /// </summary>
        /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="accessKeyId">AWS Access Key ID</param>
        /// <param name="accessKeySecret">AWS Secret Access Key</param>
        /// <param name="region"> The system name of the service like "us-west-1". The default value is us-east-2</param>
        /// <param name="cacheSize">The maximum number of items the Cache can contain before evicting using LRU. The default value is 1024.</param>
        /// <param name="cacheItemTTL">The TTL of a Cache item in milliseconds.The default value is 3600000 ms, or 1 hour</param>
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder,
            string accessKeyId,
            string accessKeySecret,
            string region = "us-east-2",
            ushort cacheSize = 1024,
            uint cacheItemTTL = 3600000u)
        {
            if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                var source = new SecretsManagerConfigurationSource(accessKeyId, accessKeySecret, region, cacheSize, cacheItemTTL);
                configurationBuilder.Add(source);
            }
            return configurationBuilder;
        }
    }

}
