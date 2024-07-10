using System;

namespace GnuPG
{
    public class ImportSecretKeyException : Exception
    {
        private static string _message = "Secrect key import failed.";

        public ImportSecretKeyException() : base(_message)
        {

        }
    }
}
