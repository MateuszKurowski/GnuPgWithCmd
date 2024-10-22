﻿using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace GnuPG
{
    public class Encrypt
    {
        public bool LogCoomands { get; set; }
        public string LogFilePath { get; set; }

        public Encrypt(bool logCoomands = true, string logFilePath = null)
        {
            LogCoomands = logCoomands;

            if (string.IsNullOrWhiteSpace(logFilePath))
            {
                string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string logDirectory = Path.Combine(userDocumentsPath, "Navigator");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                LogFilePath = Path.Combine(logDirectory, "GnuPgLogs.txt");
            }
            else
                LogFilePath = logFilePath;
        }

        public void EncryptData(string filePath, string outputFilePath, string publicKeyPath)
        {
            var file = File.ReadAllBytes(filePath);
            var publicKey = File.ReadAllBytes(publicKeyPath);

            if (!Utility.IsGnuPgInstalledOnPc(LogFilePath))
                throw new GnuPGIsNotInstalledException();

            var encryptedFile = EncryptData(file, publicKey);
            File.WriteAllBytes(outputFilePath, encryptedFile);
        }

        public byte[] EncryptData(byte[] fileBytes, byte[] publicKey)
        {
            var publicKeyId = ImportPublicKey(publicKey);

            return EncryptFile(ref publicKeyId, fileBytes);
        }

        public Dictionary<int, byte[]> EncryptData(Dictionary<int, byte[]> files, byte[] publicKey)
        {
            var publicKeyId = ImportPublicKey(publicKey);
            var result = new Dictionary<int, byte[]>();

            foreach (var file in files)
            {
                var fileBytes = file.Value;

                var encryptedFileBytes = EncryptFile(ref publicKeyId, fileBytes);
                result.Add(file.Key, encryptedFileBytes);
            }

            return result;
        }

        private byte[] EncryptFile(ref string publicKeyId, byte[] fileBytes)
        {
            var filePath = Utility.CreateTempFile(fileBytes);
            var outputFilePath = filePath + "_encrypted";

            var querry = BuildQuerry(filePath, outputFilePath, publicKeyId);
            var cmd = Utility.CreateProcess();
            cmd.Start();

            cmd.StandardInput.WriteLine(querry);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();

            var standardOutput = cmd.StandardOutput.ReadToEnd();
            var standardError = cmd.StandardError.ReadToEnd();

            cmd.WaitForExit();
            cmd.Close();

            Utility.LogCommand(LogFilePath, "StandardOutput", standardOutput);
            Utility.LogCommand(LogFilePath, "StandardError", standardError);

            Utility.DeleteTempFile(filePath);

            if (standardError.ToLower().Contains("skipped: unusable public key"))
            {
                throw new PublicKeyCurrupted();
            }

            //if (!string.IsNullOrWhiteSpace(standardOutput))
            //    throw new Exception(standardOutput);

            //if (!string.IsNullOrWhiteSpace(standardError))
            //    throw new Exception(standardError);

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

            if (!string.IsNullOrWhiteSpace(publicKeyId))
                Utility.RemoveKeys(LogFilePath, publicKeyId);


            return Utility.GetFile(outputFilePath);
        }

        private string BuildQuerry(string filePath, string outputFilePath, string keyId)
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

        private string ImportPublicKey(byte[] secretKeyBytes)
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

            Utility.LogCommand(LogFilePath, "StandardOutput", result);
            Utility.LogCommand(LogFilePath, "StandardError", standardError);

            Utility.DeleteTempFile(keyPath);
            if (string.IsNullOrWhiteSpace(standardError) || standardError.Contains("imported") || standardError.Contains("wczytano do zbioru") || standardError.Contains("not changed") || standardError.Contains("bez zmian"))
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
                return Utility.GetPublicKeyId(LogFilePath, keyFragment);
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
