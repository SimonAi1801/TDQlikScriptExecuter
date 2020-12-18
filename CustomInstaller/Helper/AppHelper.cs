//using Qlik.Sense.RestClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomInstaller.Helper
{
    public class AppHelper
    {
        private const string _filePath = @"C:\ProgramData\TrustedDecisions\";
        private const string _config = "Config";
        private const string _license = "License";
        private static QAppResponse response = new QAppResponse();
        public static void Save(string serverUrl, 
                                string userId, 
                                string userDirectory, 
                                string proxyPath, 
                                string headName, 
                                string qlikConnectorName, 
                                //string misc, 
                                string dataBaseName, 
                                string financeDashBoardConnectorPath, 
                                string appDashBoardName, 
                                string appDataLoadName, 
                                string key, 
                                string consumer, 
                                string installFolderTagret)
        {
            //var restClient = GetRestClient(serverUrl, userDirectory, userId, headName);

            CreateFolderStructure(financeDashBoardConnectorPath,
                                  serverUrl, headName, proxyPath,
                                  qlikConnectorName, dataBaseName,
                                  consumer, key, installFolderTagret,
                                  userId, userDirectory);
        }
        #region Helper
        private static void CreateFolderStructure(string financeDashBoardConnectorPath, 
                                                   string serverUrl, string proxyPath, 
                                                   string headName, string qlikConnectorName, 
                                                   string dataBaseName, string consumer, 
                                                   string key, string installFolderTagret,
                                                   string userId, string userDirectory)
        {
            StringBuilder sbConfig = new StringBuilder();
            StringBuilder sbConnection = new StringBuilder();

            if (!Directory.Exists(_filePath + _config))
            {
                Directory.CreateDirectory(_filePath + _config);
            }

            if (!Directory.Exists(_filePath + _license))
            {
                Directory.CreateDirectory(_filePath + _license);
            }

            if (!string.IsNullOrEmpty(financeDashBoardConnectorPath) &&
                !financeDashBoardConnectorPath.Equals(@"\"))
            {
                Directory.CreateDirectory(financeDashBoardConnectorPath);
                Directory.CreateDirectory(financeDashBoardConnectorPath + @"\FinanceDashboard\01_Load");
                Directory.CreateDirectory(financeDashBoardConnectorPath + @"\FinanceDashboard\02_Manipulate");
                Directory.CreateDirectory(financeDashBoardConnectorPath + @"\FinanceDashboard\03_Show");
            }
            else
            {
                Directory.CreateDirectory(installFolderTagret);
                Directory.CreateDirectory(installFolderTagret + @"FinanceDashboard\01_Load");
                Directory.CreateDirectory(installFolderTagret + @"FinanceDashboard\02_Manipulate");
                Directory.CreateDirectory(installFolderTagret + @"FinanceDashboard\03_Show");
            }

            Directory.CreateDirectory(installFolderTagret + @"\ExecuterQVDs");

            sbConfig.AppendLine($"appId={response.Id}");
            sbConfig.AppendLine($"url={serverUrl}");
            sbConfig.AppendLine($"headname={headName}");
            sbConfig.AppendLine($"userdirectory={userDirectory}");
            sbConfig.AppendLine($"userid={userId}");
            sbConfig.AppendLine($"proxypath={proxyPath}");

            sbConnection.AppendLine($"set vSQLServerConnector = ;");
            sbConnection.AppendLine($"set vCPConnectRoot = ;");
            sbConnection.AppendLine($"set vDataBaseName = {((char)34)}{dataBaseName}{((char)34)};");
            sbConnection.AppendLine($"set vQlikConnectRoot = {qlikConnectorName};");

            string licenseFile = $"Customer={consumer};Key={key}";

            File.WriteAllText(_filePath + _config + @"\CustomInformation.ini", sbConfig.ToString(), Encoding.UTF8);
            File.WriteAllText(_filePath + _config + @"\Connection.qvs", sbConnection.ToString(), Encoding.UTF8);
            File.WriteAllText(_filePath + _license + @"\License.txt", licenseFile, Encoding.UTF8);
        }

        //private static RestClient GetRestClient(string serverUrl, string userDirectory, string userId, string headName)
        //{
        //    var restClient = new RestClient(serverUrl);
        //    restClient.CustomHeaders.Add(headName, $"{userDirectory}\\{userId}");
        //    restClient.AsNtlmUserViaProxy();
        //    return restClient;
        //}
        #endregion
    }
}
