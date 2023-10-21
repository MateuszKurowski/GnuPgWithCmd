namespace Start
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /**********************************************************************************************/

            GnuPG.Decrypt.DecryptData(
                filePath: @"C:\Users\mkurowski\Downloads\4.pdf.gpg",
                outputFilePath: @"C:\Users\mkurowski\Downloads\nowyTest.pdf",
                passphrase: "z9-Lo5>j8(6",
                privateKeyFilePath: @"C:\Users\mkurowski\Downloads\Mateusz Kurowski_0x09776C16_SECRET.asc");

            /**********************************************************************************************/

            GnuPG.Encrypt.EncryptData(
                filePath: @"C:\Users\mkurowski\Downloads\test.txt",
                outputFilePath: @"C:\Users\mkurowski\Downloads\Zaszyfrowany",
                publicKeyPath: @"C:\Users\mkurowski\Downloads\Archman SP zoo_public.asc");

            /**********************************************************************************************/
        }
    }
}
