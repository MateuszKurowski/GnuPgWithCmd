namespace Start
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /**********************************************************************************************/

            //GnuPG.Decrypt.DecryptData(
            //    filePath: @"C:\Users\mkurowski\Downloads\test_klucza.pdf.gpg",
            //    outputFilePath: @"C:\Users\mkurowski\Downloads\rozszyfrowany.pdf",
            //    //passphrase: "",
            //    passphrase: "z9-Lo5>j8(6",
            //    privateKeyFilePath: @"C:\Users\mkurowski\Downloads\ZKluczem_0x9A27D7BE_SECRET.asc");

            /**********************************************************************************************/

            GnuPG.Encrypt.EncryptData(
                filePath: @"C:\Users\mkurowski\Downloads\Test.txt",
                outputFilePath: @"C:\Users\mkurowski\Downloads\Test.txt.gpg",
                publicKeyPath: @"C:\Users\mkurowski\Downloads\test_public.asc");

            /**********************************************************************************************/
        }
    }
}
