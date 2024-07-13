using System;

namespace GnuPG
{
    public class WrongPasswordException : Exception
    {
        private static string _message= "Incorrect key password.";

        public WrongPasswordException() : base(_message)
        {

        }
    }
}
