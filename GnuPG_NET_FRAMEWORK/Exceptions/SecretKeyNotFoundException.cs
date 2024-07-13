using System;

namespace GnuPG
{
    public class SecretKeyNotFoundException : Exception
    {
        private static string _message = "No secret key.";

        public SecretKeyNotFoundException() : base(_message)
        {

        }

        public SecretKeyNotFoundException(string keyId) : base(_message + $"Ends with ID: {keyId}")
        {

        }

        public SecretKeyNotFoundException(Exception innerException) : base(_message, innerException)
        {

        }
    }
}
