using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using SecretManager.ConfigurationExtension.Internal;
using Microsoft.Extensions.Configuration;
using System;

namespace SecretManager.ConfigurationExtension
{
    public static class Extension
    {
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder, string region,
           string accessKeyId, string accessKeySecret, string environment = null, string project = null)
        {
            var config = configurationBuilder.Build();
            if (string.IsNullOrEmpty(environment))
                environment = config["ASPNETCORE_ENVIRONMENT"].ToLower();
            if (string.IsNullOrEmpty(project))
                project = config["project"];

            var source = new SecretsManagerConfigurationSource(accessKeyId, accessKeySecret, region, environment, project);
            configurationBuilder.Add(source);

            return configurationBuilder;
        }
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder, SharedCredentialsFile credentials, RegionEndpoint region = null, string environment = null, string project = null)
        {
            if (region is null)
            {
                region = RegionEndpoint.USEast2;
            }
            var config = configurationBuilder.Build();
            if (string.IsNullOrEmpty(environment))
                environment = config["ASPNETCORE_ENVIRONMENT"].ToLower();
            if (string.IsNullOrEmpty(project))
                project = config["project"];
            if (credentials.TryGetProfile(SharedCredentialsFile.DefaultProfileName, out var y))
            {
                var creds = y.GetAWSCredentials(y.CredentialProfileStore);
                var source = new SecretsManagerConfigurationSource(region, creds, environment, project);
                configurationBuilder.Add(source);

                return configurationBuilder;
            }
            throw new Exception("AWS default Credentials not found, Please check https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/credentials.html");
        }

    }

}
