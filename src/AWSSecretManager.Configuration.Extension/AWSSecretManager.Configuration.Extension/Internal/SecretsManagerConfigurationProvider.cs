using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecretManager.ConfigurationExtension.Internal
{
    public class SecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly SecretsManagerCache _cache;
        private readonly string _enviroment;
        private readonly string _project;

        public SecretsManagerConfigurationProvider(IAmazonSecretsManager client, string environment, string project, ushort cacheSize = 1024, uint cacheItemTTL = 3600000u)
        {
            var config = new SecretCacheConfiguration
            {
                CacheItemTTL = cacheItemTTL,
                MaxCacheSize = cacheSize
            };
            _cache = new SecretsManagerCache(client, config);
            _enviroment = environment;
            _project = project;
        }
        public override void Load()
        {
            LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        private static bool IsJson(string str) => str.StartsWith("[") || str.StartsWith("{");

        IEnumerable<(string key, string value)> ExtractValues(JsonElement jsonElement, string prefix)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Array:
                    {
                        for (var i = 0; i < jsonElement.GetArrayLength(); i++)
                        {
                            var secretKey = $"{prefix}";
                            foreach (var (key, value) in ExtractValues(jsonElement[i], secretKey))
                            {
                                yield return (key, value);
                            }
                        }

                        break;
                    }
                case JsonValueKind.Object:
                    {
                        foreach (var property in jsonElement.EnumerateObject())
                        {
                            var secretKey = $"{prefix}" + "/" + property.Name;
                            if (property.Value.ValueKind != JsonValueKind.Null || property.Value.ValueKind != JsonValueKind.Undefined)
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
                case JsonValueKind.String:
                    {
                        var value = jsonElement.GetString();
                        yield return (prefix, value);
                        break;
                    }
                default:
                    {
                        throw new FormatException("unsupported json token");
                    }
            }
        }
        async Task<HashSet<(string, string)>> FetchConfigurationAsync()
        {
            var prefix = _enviroment + "/" + _project;

            var configuration = new HashSet<(string, string)>();

            try
            {
                var secretString = await _cache.GetSecretString(prefix).ConfigureAwait(false);
                if (IsJson(secretString))
                {
                    var obj = JsonDocument.Parse(secretString);
                    var values = ExtractValues(obj.RootElement, prefix);


                    foreach (var (key, value) in values)
                    {

                        configuration.Add((key, value));
                    }
                }
                else
                {
                    configuration.Add((prefix, secretString));
                }

            }
            catch (ResourceNotFoundException e)
            {
                throw new MissingSecretValueException($"Error retrieving secret value (Secret:{prefix})", prefix, e);
            }

            return configuration;
        }

        void SetData(IEnumerable<(string, string)> values)
        {
            Data = values.ToDictionary(x => x.Item1, x => x.Item2, StringComparer.InvariantCultureIgnoreCase);
        }

        async Task LoadAsync()
        {
            var _loadedValues = await FetchConfigurationAsync().ConfigureAwait(false);
            SetData(_loadedValues);
        }

    }
}
