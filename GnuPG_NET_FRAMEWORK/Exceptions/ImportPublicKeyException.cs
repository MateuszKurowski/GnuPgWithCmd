using System;

namespace GnuPG
{
    public class ImportPublicKeyException : Exception
    {
        private static string _message = "Public key import failed.";

        public ImportPublicKeyException() : base(_message)
        {

        }
    }
}
