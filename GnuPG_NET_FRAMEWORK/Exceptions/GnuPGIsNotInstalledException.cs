using System;

namespace GnuPG
{
    public class GnuPGIsNotInstalledException : Exception
    {
        private static string _message = "GnuPG is not installed.";

        public GnuPGIsNotInstalledException() : base(_message)
        {

        }

        public GnuPGIsNotInstalledException(Exception innerException) : base(_message, innerException)
        {

        }
    }
}
