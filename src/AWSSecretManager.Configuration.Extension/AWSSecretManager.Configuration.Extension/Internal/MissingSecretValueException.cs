using System;

namespace SecretManager.ConfigurationExtension.Internal
{
    public class MissingSecretValueException : Exception
    {
        public MissingSecretValueException(string errorMessage, string secretName, string secretArn, Exception exception) : base(errorMessage, exception)
        {
            SecretName = secretName;
            SecretArn = secretArn;
        }

        public MissingSecretValueException(string errorMessage, string secretName, Exception exception) : base(errorMessage, exception)
        {
            SecretName = secretName;
        }
        public string SecretArn { get; }

        public string SecretName { get; }
    }
}
