using System;

namespace GnuPG
{
    public class EmptyPasswordException : Exception
    {
        private static string _message= "The key requires a password.";

        public EmptyPasswordException() : base(_message)
        {

        }
    }
}
