using System;

namespace AWSSecretManager.Configuration.Extension.Internal
{
    [Serializable]
    public class CustomException : Exception
    {
        public CustomException(string message)
            : base(message)
        {

        }
    }
}
