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
using System.Threading.Tasks;
using TDQlikScriptExecuter.Entities;

namespace TDQlikScriptExecuter
{
    class Program
    {
        static async Task Main()
        {
            License license = FileManipulator.ReadLicenseFromLocal();
            license.MacAddress = (
                                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                                    where nic.OperationalStatus == OperationalStatus.Up
                                    select nic.GetPhysicalAddress().ToString()
                                 ).FirstOrDefault();

            if (await VerifyLicense(license) == false && license != null)
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

            CustomInformation custom = FileManipulator.GetCustomInfromation();
            byte[] key = Convert.FromBase64String("3W42Ped3yWB+qMRZjib4Df/5kT+rt6YBvkR/TKSxAzA=");
            string decryptedScript = DecryptSource(key);
            ILocation location = await ConnectWithProxyAsnyc(custom);
            try
            {
                IApp app = location.App(location.AppWithId($"{custom.AppId}"));
                string previousScript = FileManipulator.ModifyPreviousScript(await app.GetScriptAsync());

                await app.SetScriptAsync(decryptedScript + previousScript);
                DoReloadExResult reloadEx = await app.DoReloadExAsync();
                if (reloadEx.Success)
                {
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Kann sich nicht mit Qlik Sense verbinden! Überprüfe ob Qlik Sense laeuft." + Environment.NewLine + cex.Message);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
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
                    client.BaseAddress = new Uri("http://10.10.0.5:31098/api/License/");  //Ändern!!

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
