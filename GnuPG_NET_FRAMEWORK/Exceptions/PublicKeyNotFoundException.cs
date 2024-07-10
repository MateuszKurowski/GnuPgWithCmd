using System;

namespace GnuPG
{
    public class PublicKeyNotFoundException : Exception
    {
        private static string _message = "No public key.";

        public PublicKeyNotFoundException() : base(_message)
        {

        }

        public PublicKeyNotFoundException(string keyId) : base(_message + $"Ends with ID: {keyId}")
        {

        }

        public PublicKeyNotFoundException(Exception innerException) : base(_message, innerException)
        {

        }
    }
}
