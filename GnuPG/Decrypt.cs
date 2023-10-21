using System;
using System.IO;
using System.Text;

namespace GnuPG
{
    public static class Decrypt
    {
        public static void DecryptData(string filePath, string outputFilePath, string passphrase = null, string privateKeyFilePath = null)
        {
            var encryptedFile = File.ReadAllBytes(filePath);
            var privateKey = File.ReadAllBytes(privateKeyFilePath);

            var decryptedFile = DecryptData(encryptedFile, passphrase, privateKey);
            File.WriteAllBytes(outputFilePath, decryptedFile);
        }

        public static byte[] DecryptData(byte[] encyptedFileBytes, string passphrase = null, byte[] privateKey = null)
        {
            string secretKeyId = null;
            if (Utility.IsGnuPgInstalledOnPc() == false)
            {
                throw new GnuPGIsNotInstalledException();
            }

            if (privateKey != null)
            {
                secretKeyId = ImportSecretKey(privateKey, passphrase);
            }

            var filePath = Utility.CreateTempFile(encyptedFileBytes);
            var outputFilePath = filePath + "_decrypted";

            var querry = BuildQuerry(filePath, outputFilePath, passphrase);
            var cmd = Utility.CreateProcess();
            cmd.Start();

            cmd.StandardInput.WriteLine(querry);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Close();
            if (!string.IsNullOrWhiteSpace(secretKeyId))
                Utility.RemoveKeys(secretKeyId);

            if (standardError.ToLower().Contains("no secret key"))
            {
                var index = standardError.IndexOf("ID ");
                if (index > 1)
                {
                    try
                    {
                        var error = standardError.Substring(index + 3);
                        secretKeyId = error.Substring(0, error.IndexOf("\r"));
                    }
                    catch (Exception ex)
                    {
                        throw new SecretKeyNotFoundException(ex);
                    }
                    throw new SecretKeyNotFoundException(secretKeyId);
                }
                throw new SecretKeyNotFoundException();
            }

            var decryptedFileBytes = Utility.GetFile(outputFilePath);
            Utility.DeleteTempFile(filePath);

            return decryptedFileBytes;
        }

        private static string BuildQuerry(string filePath, string outputFilePath, string passphrase)
        {
            var querry = new StringBuilder();
            querry.Append("gpg");
            querry.Append(" --decrypt");
            querry.Append(" --output ");
            querry.Append("\"").Append(outputFilePath).Append("\"");
            if (!string.IsNullOrWhiteSpace(passphrase))
            {
                querry.Append(" --pinentry-mode loopback --passphrase ");
                querry.Append("\"").Append(passphrase).Append("\"");
            }
            querry.Append(" \"").Append(filePath).Append("\"");

            return querry.ToString();
        }

        private static string ImportSecretKey(byte[] secretKeyBytes, string passphrase = null)
        {
            var keyPath = Utility.CreateTempFile(secretKeyBytes, "GnuPGS_Key_Temp.asc");
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
            if (string.IsNullOrWhiteSpace(standardError) || standardError.Contains("secret key imported"))
            {
                var indexOfKeyStart = result.IndexOf("key ");
                if (indexOfKeyStart == -1)
                    indexOfKeyStart = standardError.IndexOf("key ");
                var keyStartToEnd = standardError.Substring(indexOfKeyStart + "key ".Length);
                var keyFragment = keyStartToEnd.Substring(0, keyStartToEnd.IndexOf(":"));
                return Utility.GetSecretKeyId(keyFragment);
            }
            else
            {
                throw new ImportSecretKeyException();
            }

            string BuildQuerry()
            {
                var sb = new StringBuilder();

                sb.Append("gpg --pinentry-mode loopback");
                if (!string.IsNullOrWhiteSpace(passphrase))
                {
                    sb.Append(" --passphrase ");
                    sb.Append("\"").Append(passphrase).Append("\"");
                }
                sb.Append(" --import ");
                sb.Append("\"").Append(keyPath).Append("\"");

                return sb.ToString();
            }
        }
    }
}
