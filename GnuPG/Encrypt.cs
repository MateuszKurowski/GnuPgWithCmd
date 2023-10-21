using System;
using System.IO;
using System.Text;

namespace GnuPG
{
    public static class Encrypt
    {
        public static void EncryptData(string filePath, string outputFilePath, string publicKeyPath)
        {
            var file = File.ReadAllBytes(filePath);
            var publicKey = File.ReadAllBytes(publicKeyPath);

            var encryptedFile = EncryptData(file, publicKey);
            File.WriteAllBytes(outputFilePath, encryptedFile);
        }

        public static byte[] EncryptData(byte[] fileBytes, byte[] publicKey)
        {
            string publicKeyId = null;
            if (Utility.IsGnuPgInstalledOnPc() == false)
            {
                throw new GnuPGIsNotInstalledException();
            }
                
            publicKeyId = ImportPublicKey(publicKey);

            var filePath = Utility.CreateTempFile(fileBytes);
            var outputFilePath = filePath + "_encrypted";

            var querry = BuildQuerry(filePath, outputFilePath, publicKeyId);
            var cmd = Utility.CreateProcess();
            cmd.Start();

            cmd.StandardInput.WriteLine(querry);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Close();
            if (!string.IsNullOrWhiteSpace(publicKeyId))
                Utility.RemoveKeys(publicKeyId);

            if (standardError.ToLower().Contains("no public key"))
            {
                var index = standardError.IndexOf("ID ");
                if (index > 1)
                {
                    try
                    {
                        var error = standardError.Substring(index + 3);
                        publicKeyId = error.Substring(0, error.IndexOf("\r"));
                    }
                    catch (Exception ex)
                    {
                        throw new PublicKeyNotFoundException(ex);
                    }
                    throw new PublicKeyNotFoundException(publicKeyId);
                }
                throw new PublicKeyNotFoundException();
            }

            var encryptedFileBytes = Utility.GetFile(outputFilePath);
            Utility.DeleteTempFile(filePath);

            return encryptedFileBytes;
        }

        private static string BuildQuerry(string filePath, string outputFilePath, string keyId)
        {
            var querry = new StringBuilder();
            querry.Append("gpg");
            querry.Append(" --batch");
            querry.Append(" --yes");
            querry.Append(" --trust-model always");
            querry.Append(" --encrypt");
            querry.Append(" --recipient ").Append("\"").Append(keyId).Append("\"");
            querry.Append(" --output ").Append("\"").Append(outputFilePath).Append("\"");
            querry.Append(" \"").Append(filePath).Append("\"");

            return querry.ToString();
        }

        private static string ImportPublicKey(byte[] secretKeyBytes)
        {
            var keyPath = Utility.CreateTempFile(secretKeyBytes, "GnuPGP_Public_Key_Temp.asc");
            var querry = BuildQuerry();
            var cmd = Utility.CreateProcess();
            cmd.Start();
            cmd.StandardInput.WriteLine(querry);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var result = cmd.StandardOutput.ReadToEnd();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Dispose();
            cmd.Close();

            Utility.DeleteTempFile(keyPath);
            if (string.IsNullOrWhiteSpace(standardError) || standardError.Contains("imported") || standardError.Contains("not changed"))
            {
                var indexOfKeyStart = result.IndexOf("key ");
                if (indexOfKeyStart == -1)
                    indexOfKeyStart = standardError.IndexOf("key ");
                var keyStartToEnd = standardError.Substring(indexOfKeyStart + "key ".Length);
                var keyFragment = keyStartToEnd.Substring(0, keyStartToEnd.IndexOf(":"));
                return Utility.GetPublicKeyId(keyFragment);
            }
            else
            {
                throw new ImportPublicKeyException();
            }

            string BuildQuerry()
            {
                var sb = new StringBuilder();

                sb.Append("gpg ");
                sb.Append(" --import ");
                sb.Append("\"").Append(keyPath).Append("\"");

                return sb.ToString();
            }
        }
    }
}
