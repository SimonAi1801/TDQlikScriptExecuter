using Qlik.Engine;
using Qlik.Engine.Communication;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TDQlikScriptExecuter.Entities;

namespace TDQlikScriptExecuter
{
    class Program
    {
        static async Task Main()
        {
            License license = FileManipulator.ReadLicense();
            license.MacAddress = (
                                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                                    where nic.OperationalStatus == OperationalStatus.Up
                                    select nic.GetPhysicalAddress().ToString()
                                 ).FirstOrDefault();
            //Thread.Sleep(5000);
            var isValid = await VerifyLicense(license);

            if (!isValid && license != null)
            {
                throw new Exception("Die eingegebene Lizenz ist falsch!");
            }
            else if (!license.IsKeyValid)
            {
                throw new Exception("Die eingegebene Lizenz ist abgelaufen!");
            }
            else
            {
                FileManipulator.WriteLicenseToLocal(license);
            }
            //Thread.Sleep(5000);
            CustomInformation custom = FileManipulator.ReadCustomInfromation();
            FileManipulator.ModifyQvs();
            byte[] key = Convert.FromBase64String("QRZbi39oJei+Zz6F0Ht04Xwu3jx4cnDjaaEkJJdCIhs=");
            string decryptedScript = DecryptSource(key);
            ILocation location = await ConnectWithProxyAsnyc(custom);
            File.WriteAllText(@"C:\Work\ScriptExecuter\stage0.txt", location.ToString());
            Thread.Sleep(5000);
            try
            {

                File.WriteAllText(@"C:\Work\ScriptExecuter\stageId.txt", custom.ToString());
                IApp app = location.App(location.AppWithId($"{custom.AppId}"));
                File.WriteAllText(@"C:\Work\ScriptExecuter\stage1", "After App Reload");
                string previousScript = FileManipulator.ModifyPreviousScript(await app.GetScriptAsync());
                Thread.Sleep(5000);
                File.WriteAllText(@"C:\Work\ScriptExecuter\stage2.txt", "Get Script");
                await app.SetScriptAsync(decryptedScript + previousScript);
                DoReloadExResult reloadEx = await app.DoReloadExAsync();
                //File.WriteAllText(@"C:\Work\ScriptExecuter\stage3.txt", "Reload");
                if (reloadEx.Success)
                {
                    Thread.Sleep(5000);
                    await app.SetScriptAsync(await app.GetEmptyScriptAsync() + previousScript);
                    await app.DoSaveAsync();
                    FileManipulator.ModifyLogFile(reloadEx.ScriptLogFile);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Es gab ein Problem beim Laden!");
                    Console.ResetColor();
                }
            }
            catch (CommunicationErrorException cex)
            {
                File.WriteAllText(@"C:\Work\ScriptExecuter\errorCommunication.txt", cex.Message + cex.InnerException.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Kann sich nicht mit Qlik Sense verbinden! Überprüfe ob Qlik Sense laeuft." + Environment.NewLine + cex.Message);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                File.WriteAllText(@"C:\Work\ScriptExecuter\error.txt", ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Allgemeiner Fehler: " + Environment.NewLine + ex.Message);
                Console.ResetColor();
            }
        }

        #region Http
        private async static Task<bool> VerifyLicense(License license)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://qlikdemo.trusteddecisions.com:31098/api/License/");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Setting timeout.  
                    client.Timeout = TimeSpan.FromSeconds(Convert.ToDouble(1000000));

                    // Initialization.  
                    HttpResponseMessage response = new HttpResponseMessage();
                    // HTTP get  
                    response = await client.GetAsync(client.BaseAddress + $"{license.Key}/{license.MacAddress}/{license.CustomerName}/getVerified");
                    string stringResult = await response.Content.ReadAsStringAsync();
                    var rarLicense = JsonSerializer.Deserialize<License>(stringResult);

                    license.Key = rarLicense.Key;
                    license.ValidUntil = rarLicense.ValidUntil;

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Qlik Sense

        private async static Task<ILocation> ConnectWithProxyAsnyc(CustomInformation custom)
        {
            ILocation location = Location.FromUri(new Uri($"{custom.Url}"));
            location.CustomUserHeaders.Add($"{custom.HeadName}", $"{custom.UserDirectory}\\{custom.UserId}");
            await location.AsNtlmUserViaProxyAsync();
            location.VirtualProxyPath = $"{custom.ProxyPath}";
            return location;
        }

        #endregion

        #region Crypto

        private static string DecryptSource(byte[] key)
        {
            using (var sourceStream = File.OpenRead(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\myfile.crypto"))
            using (var provider = new AesCryptoServiceProvider())
            {
                var IV = new byte[provider.IV.Length];
                sourceStream.Read(IV, 0, IV.Length);
                using (var cryptoTransform = provider.CreateDecryptor(key, IV))
                using (var cryptoStream = new CryptoStream(sourceStream, cryptoTransform, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cryptoStream))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        #endregion
    }
}
