using Quanto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    public class ServiceProxy
    {
        private static ServiceProxy instance;
        public static ServiceProxy Instance
        {
            get
            {
                if (instance == null)
                    instance = new ServiceProxy();

                return instance;
            }
        }

        public async Task<InfyPOS.Processors.OfflineClient.WindowsOfflineResponse> GetWindowsOfflineStatus(string key)
        {
            using (var client = new HttpClient())
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncController/UploadWindowsOfflineStatus/" + key;
                Logger.Current.Info("Get Status from " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<InfyPOS.Processors.OfflineClient.WindowsOfflineResponse>(json);
                }
            }
            return null;
        }
        public bool UseClientMasterSource
        {
            get
            {
                var source = System.Configuration.ConfigurationManager.AppSettings["MasterSource"];
                return !string.IsNullOrEmpty(source) &&
                       source.Equals("Client", StringComparison.OrdinalIgnoreCase);
            }
        }

        public string ClientWebUrl
        {
            get
            {
                var url = System.Configuration.ConfigurationManager.AppSettings["ClientURL"];
                return string.IsNullOrEmpty(url) ? "" : url.TrimEnd('/');
            }
        }

        public InfyPOS.Processors.OfflineClient.WindowsOfflineResponse DownloadMasterFromConfiguredSource(InfyPOS.Processors.OfflineClient.WindowsOfflineRequest creteria)
        {
            if (UseClientMasterSource)
                return DownloadAndApplyClientMaster();

            var result = DownloadMaster(creteria).Result;
            if (result == null)
                return new InfyPOS.Processors.OfflineClient.WindowsOfflineResponse()
                {
                    error = true,
                    completed = true,
                    errormessage = "Unable to start master download from Quanto server."
                };

            while (!result.completed && !result.error)
            {
                System.Threading.Thread.Sleep(5000);
                var statusresponse = GetWindowsOfflineStatus(result.key).Result;
                if (statusresponse != null && (statusresponse.completed || statusresponse.error))
                {
                    if (statusresponse.completed && statusresponse.data != null)
                    {
                        InfyPOS.Processors.BillManager.Instance.Load(statusresponse.data);
                        statusresponse.data = null;
                    }
                    return statusresponse;
                }
            }

            if (result.completed && result.data != null)
            {
                InfyPOS.Processors.BillManager.Instance.Load(result.data);
                result.data = null;
            }
            return result;
        }

        public InfyPOS.Processors.OfflineClient.WindowsOfflineResponse DownloadAndApplyClientMaster()
        {
            var package = DownloadClientMaster().Result;
            if (package == null)
            {
                return new InfyPOS.Processors.OfflineClient.WindowsOfflineResponse()
                {
                    error = true,
                    completed = true,
                    errormessage = "Unable to download master from the client application."
                };
            }
            if (package.error)
            {
                return new InfyPOS.Processors.OfflineClient.WindowsOfflineResponse()
                {
                    error = true,
                    completed = true,
                    errormessage = package.errormessage
                };
            }

            InfyPOS.Processors.BillManager.Instance.LoadFromClientPackage(package);
            return new InfyPOS.Processors.OfflineClient.WindowsOfflineResponse()
            {
                completed = true
            };
        }

        public async Task<InfyPOS.Processors.OfflineClient.ClientMasterPackage> DownloadClientMaster()
        {
            if (string.IsNullOrEmpty(ClientWebUrl))
            {
                return new InfyPOS.Processors.OfflineClient.ClientMasterPackage()
                {
                    error = true,
                    errormessage = "ClientURL is not configured in app.config."
                };
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                string url = ClientWebUrl + "/TextilePOS/ClientMaster";
                Logger.Current.Info("DownloadMaster from client " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<InfyPOS.Processors.OfflineClient.ClientMasterPackage>(json);
                }
                return new InfyPOS.Processors.OfflineClient.ClientMasterPackage()
                {
                    error = true,
                    errormessage = "Client master download failed with status " + (int)result.StatusCode
                };
            }
        }

        public async Task<InfyPOS.Processors.OfflineClient.ClientMasterPackage> DownloadClientCustomer()
        {
            if (string.IsNullOrEmpty(ClientWebUrl))
            {
                return new InfyPOS.Processors.OfflineClient.ClientMasterPackage()
                {
                    error = true,
                    errormessage = "ClientURL is not configured in app.config."
                };
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                string url = ClientWebUrl + "/TextilePOS/ClientCustomer";
                Logger.Current.Info("DownloadCustomer from client " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<InfyPOS.Processors.OfflineClient.ClientMasterPackage>(json);
                }
                return new InfyPOS.Processors.OfflineClient.ClientMasterPackage()
                {
                    error = true,
                    errormessage = "Client customer download failed with status " + (int)result.StatusCode
                };
            }
        }

        public async Task<InfyPOS.Processors.OfflineClient.WindowsOfflineResponse> DownloadMaster(InfyPOS.Processors.OfflineClient.WindowsOfflineRequest creteria)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncController/DownloadWindowsOffline";
                Logger.Current.Info("DownloadMaster from " + url);
                Logger.Current.Info(content.ToJSON());

                var result = Task.Run(() => client.PostAsync(url, content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<InfyPOS.Processors.OfflineClient.WindowsOfflineResponse>(json);
                }
            }
            return null;
        }

        public async Task<InfyPOS.Processors.OfflineClient.WindowsOfflineResponse> SyncOffline(InfyPOS.Processors.OfflineClient.WindowsOfflineRequest creteria)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncController/UploadWindowsOffline";
                Logger.Current.Info("DownloadMaster from " + url);

                var result = Task.Run(() => client.PostAsync(url, content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<InfyPOS.Processors.OfflineClient.WindowsOfflineResponse>(json);
                }
            }
            return null;
        }

        private ServiceProxy()
        {
            URI = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/CompanyService";
            SMSURI = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SmsmessageService";
        }

        string URI = "http://localhost:1706/CompanyService";
        string SMSURI = "http://localhost:1706/SmsmessageService";
        public async Task<bool> AddPrinter(PrinterConfig creteria)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                Logger.Current.Info("Adding Printer to " + URI + "/AddPrinter");
                Logger.Current.Info(content.ToJSON());

                var result = Task.Run(() => client.PostAsync(URI + "/AddPrinter", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
            }
            return false;
        }


        public async Task<LicenseInfo> CreateClientLicense(PrinterConfig creteria)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                Logger.Current.Info("CreateClientLicense " + URI + "/CreateClientLicense");
                Logger.Current.Info(content.ToJSON());

                var result = Task.Run(() => client.PostAsync(URI + "/CreateClientLicense", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LicenseInfo>(json);
                }
            }
            return null;
        }

        public async Task<bool> SendPasscode(PrinterConfig creteria)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                Logger.Current.Info("Adding Printer to " + URI + "/SendPasscode");
                Logger.Current.Info(content.ToJSON());

                var result = Task.Run(() => client.PostAsync(URI + "/SendPasscode", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
            }
            return false;
        }

        public async Task<bool> TriggerSchedule(DateTime dt)
        {
            using (var client = new HttpClient())
            {
                var result = Task.Run(() => client.GetAsync(SMSURI + "/StartScheduleMessage")).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
            }
            return false;
        }
        public async Task<bool> UpdateStatus(SMS.SMS creteria)
        {
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(SMSURI + "/UpdateStatus", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<bool>(json);
                }
            }
            return false;
        }

    }
}
