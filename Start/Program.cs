namespace Start
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /**********************************************************************************************/

            GnuPG.Decrypt.DecryptData(
                filePath: @"C:\Users\test\Downloads\file.pdf",
                outputFilePath: @"C:\Users\test\Downloads\file.pdf.gpg",
                passphrase: "123456",
                privateKeyFilePath: @"C:\Users\test\Downloads\key_private.asc");

            /**********************************************************************************************/

            //GnuPG.Encrypt.EncryptData(
            //    filePath: @"C:\Users\test\Downloads\file.pdf",
            //    outputFilePath: @"C:\Users\test\Downloads\file.pdf.gpg",
            //    publicKeyPath: @"C:\Users\test\Downloads\key_public.asc");

            /**********************************************************************************************/
        }
    }
}
