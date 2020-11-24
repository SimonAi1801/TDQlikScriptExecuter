using System.IO;
using TDQlikScriptExecuter.Entities;

namespace TDQlikScriptExecuter
{
    public static class FileManipulator
    {
        private static string _customInformationFilePath = @"C:\ProgramData\TrustedDecisions\Config\CustomInformation.ini";
        private const string _licenseFilePath = @"C:\ProgramData\TrustedDecisions\License\License.txt";

        public static CustomInformation GetCustomInfromation()
        {
            string[] lines = GetInformationFromFile(_customInformationFilePath).Split("\r\n");
            CustomInformation customInformation = new CustomInformation();

            foreach (var item in lines)
            {
                SetToCustomInformationProperty(item.Split("="), customInformation);
            }
            return customInformation;
        }

        public static License ReadLicenseFromLocal()
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

                if (collumns[0].Equals("Customer"))
                {
                    myLicense.CustomerName = collumns[1];
                }
                else if (collumns[0].Equals("Key"))
                {
                    myLicense.Key = collumns[1];
                }
            }
            return myLicense;
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
            switch (parts[0].ToLower())
            {
                case "appid":
                    {
                        customInformation.AppId = parts[1];
                        break;
                    }
                case "url":
                    {
                        customInformation.Url = parts[1];
                        break;
                    }
                case "headname":
                    {
                        customInformation.HeadName = parts[1];
                        break;
                    }
                case "userdirectory":
                    {
                        customInformation.UserDirectory = parts[1];
                        break;
                    }
                case "userid":
                    {
                        customInformation.UserId = parts[1];
                        break;
                    }
                case "proxypath":
                    {
                        customInformation.ProxyPath = parts[1];
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
