﻿using System;
using System.Diagnostics;
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

            if (standardError.ToLower().Contains("no secret key")
                 || standardError.ToLower().Contains("brak klucza tajnego"))
            {
                var keyWord = "ID ";
                var index = standardError.IndexOf(keyWord);
                if (index == -1)
                {
                    keyWord = "identyfikatorze ";
                    index = standardError.IndexOf(keyWord);
                }
                if (index > 1)
                {
                    try
                    {
                        var error = standardError.Substring(index + keyWord.Length);
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

            if (standardError.ToLower().Contains("sorry, we are in batchmode - can't get input"))
            {
                throw new EmptyPasswordException();
            }

            if (standardError.ToLower().Contains("bad passphrase")
                || standardError.ToLower().Contains("błędne hasło"))
            {
                throw new WrongPasswordException();
            }

            var decryptedFileBytes = Utility.GetFile(outputFilePath);
            Utility.DeleteTempFile(filePath);

            return decryptedFileBytes;
        }

        private static string BuildQuerry(string filePath, string outputFilePath, string passphrase)
        {
            var querry = new StringBuilder();
            querry.Append("gpg --pinentry-mode loopback --batch");
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
            if (string.IsNullOrWhiteSpace(standardError) || standardError.Contains("secret key imported") || standardError.Contains("wczytany do zbioru"))
            {
                var keyWord = "key ";
                var indexOfKeyStart = result.IndexOf(keyWord);
                if (indexOfKeyStart == -1)
                    indexOfKeyStart = standardError.IndexOf(keyWord);
                if (indexOfKeyStart == -1)
                {
                    keyWord = "klucz ";
                    indexOfKeyStart = result.IndexOf(keyWord);
                }
                if (indexOfKeyStart == -1)
                    indexOfKeyStart = standardError.IndexOf(keyWord);
                var keyStartToEnd = standardError.Substring(indexOfKeyStart + keyWord.Length);
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

                sb.Append("gpg --pinentry-mode loopback --batch");
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

        private static void ReadOutput(Process cmd, StringBuilder resultBuilder, StringBuilder errorBuilder, string passphrase)
        {
            string line;
            while ((line = cmd.StandardOutput.ReadLine()) != null)
            {
                resultBuilder.AppendLine(line);
                if (line.Contains("Hasło:"))
                {
                    if (!string.IsNullOrWhiteSpace(passphrase))
                    {
                        cmd.StandardInput.WriteLine(passphrase);
                    }
                    cmd.StandardInput.WriteLine(); // Simulate pressing Enter
                }
            }

            while ((line = cmd.StandardError.ReadLine()) != null)
            {
                errorBuilder.AppendLine(line);
            }
        }
    }
}
