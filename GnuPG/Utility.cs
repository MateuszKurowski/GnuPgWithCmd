using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace GnuPG
{
    internal static class Utility
    {
        public static void LogCommand(string logFilePath, string name, string text)
        {
            text = text.Replace("Microsoft Windows [Version 10.0.20348.1547]", "");
            text = text.Replace("(c) Microsoft Corporation.All rights reserved.", "");

            using (StreamWriter sw = new StreamWriter(logFilePath, true))
            {
                sw.WriteLine($"[{DateTime.Now}]{name}: {text}");
            }
        }

        internal static bool IsGnuPgInstalledOnPc(string logFilePath)
        {
            try
            {
                var cmd = CreateProcess();
                cmd.Start();

                cmd.StandardInput.WriteLine("gpg --version & exit");
                cmd.StandardInput.Flush();
                cmd.StandardInput.Close();
                cmd.StandardInput.Dispose();
                var result = cmd.StandardOutput.ReadToEnd();
                var standardError = cmd.StandardError.ReadToEnd();
                cmd.WaitForExit();
                cmd.Dispose();
                cmd.Close();

                Utility.LogCommand(logFilePath, "StandardOutput", result);
                Utility.LogCommand(logFilePath, "StandardError", standardError);

                if (result.ToLower().Contains("gpg (gnupg)"))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw new GnuPGIsNotInstalledException(ex);
            }
        }

        internal static string CreateTempFile(byte[] file, string name = null)
        {
            if (name is null)
                name = "NavigatorGnuPG_TempFile";

            var path = Path.Combine(Path.GetTempPath(), name);
            try
            {
                if (File.Exists(path))
                    DeleteTempFile(path);
            }
            catch (Exception)
            {
                var i = 1;
                while (File.Exists(path))
                {
                    path += i.ToString();
                    i++;
                }
            }

            File.WriteAllBytes(path, file);
            return path;
        }

        internal static void DeleteTempFile(string path)
            => File.Delete(path);

        internal static Process CreateProcess()
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            cmd.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            cmd.StartInfo.EnvironmentVariables["LANG"] = "en_US.UTF-8";
            return cmd;
        }

        internal static byte[] GetFile(string outputFilePath)
        {
            var fileBytes = File.ReadAllBytes(outputFilePath);
            DeleteTempFile(outputFilePath);
            return fileBytes;
        }

        internal static void RemoveKeys(string logFilePath, string keyId)
        {
            RemoveSecretKey(logFilePath, keyId);
            Thread.Sleep(1000);
            RemovePublicKey(logFilePath, keyId);
        }

        internal static string GetSecretKeyId(string logFilePath, string keyFragment)
        {
            var cmd = CreateProcess();
            cmd.Start();
            cmd.StandardInput.WriteLine("gpg --list-secret-key");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var result = cmd.StandardOutput.ReadToEnd();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Dispose();
            cmd.Close();

            Utility.LogCommand(logFilePath, "StandardOutput", result);
            Utility.LogCommand(logFilePath, "StandardError", standardError);

            var indexOfEndKey = result.IndexOf(keyFragment) + keyFragment.Length;
            if (indexOfEndKey == -1 - keyFragment.Length)
                return null;

            var partFromStartToKeyEnd = result.Substring(0, indexOfEndKey);
            var keyStartIndex = partFromStartToKeyEnd.LastIndexOf(' ') + 1;
            if (keyStartIndex == -1 - 1)
                return null;

            return partFromStartToKeyEnd.Substring(keyStartIndex);
        }

        internal static string GetPublicKeyId(string logFilePath, string keyFragment)
        {
            var cmd = Utility.CreateProcess();
            cmd.Start();
            cmd.StandardInput.WriteLine("gpg --list-key");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var result = cmd.StandardOutput.ReadToEnd();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Dispose();
            cmd.Close();

            Utility.LogCommand(logFilePath, "StandardOutput", result);
            Utility.LogCommand(logFilePath, "StandardError", standardError);

            var indexOfEndKey = result.IndexOf(keyFragment) + keyFragment.Length;
            if (indexOfEndKey == -1 - keyFragment.Length)
                return null;

            var partFromStartToKeyEnd = result.Substring(0, indexOfEndKey);
            var keyStartIndex = partFromStartToKeyEnd.LastIndexOf(' ') + 1;
            if (keyStartIndex == -1 - 1)
                return null;

            return partFromStartToKeyEnd.Substring(keyStartIndex);
        }

        private static void RemoveSecretKey(string logFilePath, string keyId)
        {
            var cmd = CreateProcess();
            cmd.Start();
            cmd.StandardInput.WriteLine($"gpg --yes --batch --delete-secret-key \"{keyId}\"");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var result = cmd.StandardOutput.ReadToEnd();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Dispose();
            cmd.Close();

            Utility.LogCommand(logFilePath, "StandardOutput", result);
            Utility.LogCommand(logFilePath, "StandardError", standardError);
        }

        private static void RemovePublicKey(string logFilePath, string keyId)
        {
            var cmd = CreateProcess();
            cmd.Start();
            cmd.StandardInput.WriteLine($"gpg --yes --batch --delete-key \"{keyId}\"");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.StandardInput.Dispose();
            var result = cmd.StandardOutput.ReadToEnd();
            var standardError = cmd.StandardError.ReadToEnd();
            cmd.WaitForExit();
            cmd.Dispose();
            cmd.Close();

            Utility.LogCommand(logFilePath, "StandardOutput", result);
            Utility.LogCommand(logFilePath, "StandardError", standardError);
        }
    }
}
