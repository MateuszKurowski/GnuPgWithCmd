using System;

namespace GnuPG
{
    public class PublicKeyCurrupted : Exception
    {
        private static string _message= "The public key is either invalid or has expired.";

        public PublicKeyCurrupted() : base(_message)
        {

        }
    }
}
