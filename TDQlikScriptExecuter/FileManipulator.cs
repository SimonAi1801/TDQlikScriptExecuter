using System;
using System.IO;
using System.Text;
using TDQlikScriptExecuter.Entities;

namespace TDQlikScriptExecuter
{
    public static class FileManipulator
    {
        private static string _customInformationFilePath = @"C:\ProgramData\TrustedDecisions\Config\CustomInformation.ini";
        private const string _licenseFilePath = @"C:\ProgramData\TrustedDecisions\License\License.txt";
        private const string _connectorFilePath = @"C:\ProgramData\TrustedDecisions\Config\Connection.qvs";

        public static CustomInformation ReadCustomInfromation()
        {
            string[] lines = GetInformationFromFile(_customInformationFilePath).Split("\r\n");
            CustomInformation customInformation = new CustomInformation();

            foreach (var item in lines)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    SetToCustomInformationProperty(item.Split("="), customInformation);
                }
            }
            return customInformation;
        }

        public static License ReadLicense()
        {
            string lines = GetInformationFromFile(_licenseFilePath);

            if (string.IsNullOrEmpty(lines))
            {
                return null;
            }

            string[] parts = lines.Split(";");
            License myLicense = new License();

            foreach (var item in parts)
            {
                string[] collumns = item.Split("=");
                string[] temp = collumns[1].Split((char)92);
                if (collumns[0].Equals("Customer"))
                {
                    myLicense.CustomerName = temp[0];
                }
                else if (collumns[0].Equals("Key"))
                {
                    myLicense.Key = temp[0];
                }
            }
            return myLicense;
        }

        public static void ModifyQvs()
        {
            string lines = GetInformationFromFile(_connectorFilePath);
            StringBuilder sb = new StringBuilder();
            string[] parts = lines.Split(';');

            foreach (var item in parts)
            {
                string[] tmp = item.Split((char)92);
                if (!string.IsNullOrWhiteSpace(item))
                {
                    sb.AppendLine(tmp[0] + ';');
                }
            }
            WriteInformationToFile(sb.ToString(), _connectorFilePath);
        }

        public static string ModifyPreviousScript(string previousScript)
        {
            string[] parts = previousScript.Split(';');
            string modifiedScript = null;
            foreach (var item in parts)
            {
                if (item.Contains("Store"))
                {
                    modifiedScript += item + ';';
                }
            }
            return modifiedScript;
        }

        public static void WriteLicenseToLocal(License license)
        {
            WriteInformationToFile(license.ToString(), _licenseFilePath);
        }

        public static void ModifyLogFile(string logFilePath)
        {
            string newLogFile = null;
            string[] lines = GetInformationFromFile(logFilePath).Split("\n");

            foreach (var item in lines)
            {
                if (IsLineValid(item.Split(" ")))
                {
                    newLogFile += item + "\n";
                }
            }
            WriteInformationToFile(newLogFile, logFilePath);
        }


        #region Helper
        private static bool IsLineValid(string[] parts)
        {
            bool isValid = true;
            for (int i = 0; i < parts.Length; i++)
            {
                if (IsDigit(parts[i]) && parts[i].Length == 4)
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        private static bool IsDigit(string part)
        {
            bool isValid = true;
            for (int i = 0; i < part.Length; i++)
            {
                if (!char.IsDigit(part[i]))
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        private static void SetToCustomInformationProperty(string[] parts, CustomInformation customInformation)
        {
            string[] tmp = parts[1].Split((char)92);

            switch (parts[0].ToLower())
            {
                case "appid":
                    {
                        customInformation.AppId = tmp[0];
                        break;
                    }
                case "url":
                    {
                        customInformation.Url = tmp[0];
                        break;
                    }
                case "headname":
                    {
                        customInformation.HeadName = tmp[0];
                        break;
                    }
                case "userdirectory":
                    {
                        customInformation.UserDirectory = tmp[0];
                        break;
                    }
                case "userid":
                    {
                        customInformation.UserId = tmp[0];
                        break;
                    }
                case "proxypath":
                    {
                        customInformation.ProxyPath = tmp[0];
                        break;
                    }
            }
        }

        private static string GetInformationFromFile(string filePath)
        {
            using (var sourceStream = File.OpenRead(filePath))
            using (StreamReader sr = new StreamReader(sourceStream))
            {
                return sr.ReadToEnd();
            }
        }

        private static void WriteInformationToFile(string newFile, string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write($"{newFile}");
            }
        }
        #endregion
    }
}
