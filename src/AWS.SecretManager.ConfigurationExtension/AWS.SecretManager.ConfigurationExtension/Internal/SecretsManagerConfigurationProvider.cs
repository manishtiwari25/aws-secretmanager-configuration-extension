﻿using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.SecretManager.ConfigurationExtension.Internal
{
    public class SecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly IAmazonSecretsManager _client;
        private HashSet<(string, string)> _loadedValues = new HashSet<(string, string)>();

        public SecretsManagerConfigurationProvider(IAmazonSecretsManager client)
        {
            _client = client;
        }
        public override void Load()
        {
            LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        private static bool IsJson(string str) => str.StartsWith("[") || str.StartsWith("{");

        IEnumerable<(string key, string value)> ExtractValues(JToken token, string prefix)
        {
            switch (token)
            {
                case JArray array:
                    {
                        for (var i = 0; i < array.Count; i++)
                        {
                            var secretKey = $"{prefix}";
                            foreach (var (key, value) in ExtractValues(array[i], secretKey))
                            {
                                yield return (key, value);
                            }
                        }

                        break;
                    }
                case JObject jObject:
                    {
                        foreach (var property in jObject.Properties())
                        {
                            var secretKey = $"{prefix}";

                            if (property.Value.HasValues)
                            {
                                foreach (var (key, value) in ExtractValues(property.Value, secretKey))
                                {
                                    yield return (key, value);
                                }
                            }
                            else
                            {
                                var value = property.Value.ToString();
                                yield return (secretKey, value);
                            }
                        }

                        break;
                    }
                case JValue jValue:
                    {
                        var value = jValue.Value.ToString();
                        yield return (prefix, value);
                        break;
                    }
                default:
                    {
                        throw new FormatException("unsupported json token");
                    }
            }
        }
        async Task<HashSet<(string, string)>> FetchConfigurationAsync(CancellationToken cancellationToken)
        {
            var secrets = await FetchAllSecretsAsync(cancellationToken).ConfigureAwait(false);
            var configuration = new HashSet<(string, string)>();
            foreach (var secret in secrets)
            {
                try
                {
                    var secretValue = await _client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secret.ARN }, cancellationToken).ConfigureAwait(false);

                    var secretString = secretValue.SecretString;

                    if (secretString is null)
                        continue;

                    if (IsJson(secretString))
                    {
                        var obj = JToken.Parse(secretString);

                        var values = ExtractValues(obj, secret.Name);

                        foreach (var (key, value) in values)
                        {
                            configuration.Add((key, value));
                        }
                    }
                    else
                    {
                        configuration.Add((secret.Name, secretString));
                    }
                }
                catch (ResourceNotFoundException e)
                {
                    throw new MissingSecretValueException($"Error retrieving secret value (Secret: {secret.Name} Arn: {secret.ARN})", secret.Name, secret.ARN, e);
                }
            }
            return configuration;
        }
        async Task<IReadOnlyList<SecretListEntry>> FetchAllSecretsAsync(CancellationToken cancellationToken)
        {
            var response = default(ListSecretsResponse);

            var result = new List<SecretListEntry>();

            do
            {
                var nextToken = response?.NextToken;

                var request = new ListSecretsRequest() { NextToken = nextToken };

                response = await _client.ListSecretsAsync(request, cancellationToken).ConfigureAwait(false);

                result.AddRange(response.SecretList);
            } while (response.NextToken != null);

            return result;
        }

        void SetData(IEnumerable<(string, string)> values)
        {
            Data = values.ToDictionary(x => x.Item1, x => x.Item2, StringComparer.InvariantCultureIgnoreCase);
        }

        async Task LoadAsync()
        {
            _loadedValues = await FetchConfigurationAsync(default).ConfigureAwait(false);
            SetData(_loadedValues);
        }
    }
}
