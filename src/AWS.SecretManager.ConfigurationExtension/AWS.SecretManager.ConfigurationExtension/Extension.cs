using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AWS.SecretManager.ConfigurationExtension.Internal;
using Microsoft.Extensions.Configuration;
using System;

namespace AWS.SecretManager.ConfigurationExtension
{
    public static class Extension
    {
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder, string region,
           string accessKeyId, string accessKeySecret)
        {
            var source = new SecretsManagerConfigurationSource(accessKeyId, accessKeySecret, region);
            configurationBuilder.Add(source);

            return configurationBuilder;
        }
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder, SharedCredentialsFile credentials, RegionEndpoint? region = null)
        {
            if (region is null)
            {
                region = RegionEndpoint.USEast2;
            }
            if (credentials.TryGetProfile(SharedCredentialsFile.DefaultProfileName, out var y))
            {
                var creds = y.GetAWSCredentials(y.CredentialProfileStore);
                var source = new SecretsManagerConfigurationSource(region, creds);
                configurationBuilder.Add(source);

                return configurationBuilder;
            }
            throw new Exception("AWS default Credentials not found, Please check https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/credentials.html");
        }

    }

}
