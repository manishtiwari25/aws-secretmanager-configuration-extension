using Amazon;
using Amazon.Runtime.CredentialManagement;
using AWSSecretManager.Configuration.Extension.Internal;
using Microsoft.Extensions.Configuration;
using SecretManager.ConfigurationExtension.Internal;
using System;

namespace SecretManager.ConfigurationExtension
{
    public static class Extension
    {
        public static IConfigurationBuilder AddSecretsManager(this IConfigurationBuilder configurationBuilder, string region,
           string accessKeyId, string accessKeySecret, string environment = null, string project = null)
        {
            if (string.IsNullOrEmpty(environment))
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower();
            if (string.IsNullOrEmpty(project))
                project = Environment.GetEnvironmentVariable("project");

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

            if (string.IsNullOrEmpty(environment))
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower();
            if (string.IsNullOrEmpty(project))
                project = Environment.GetEnvironmentVariable("project");
            if (credentials.TryGetProfile(SharedCredentialsFile.DefaultProfileName, out var y))
            {
                var creds = y.GetAWSCredentials(y.CredentialProfileStore);
                var source = new SecretsManagerConfigurationSource(region, creds, environment, project);
                configurationBuilder.Add(source);

                return configurationBuilder;
            }
            throw new CustomException("AWS default Credentials not found, Please check https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/credentials.html");
        }

    }

}
