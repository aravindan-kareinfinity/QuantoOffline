using Quanto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto.SMS
{
    public class SMSService
    {
        private static SMSService instance;
        public static SMSService Instance
        {
            get
            {
                if (instance == null)
                    instance = new SMSService();
                return instance;
            }
        }


        string connectionstring;
        public SMSService()
        {

            System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName);
            string fileName = System.IO.Path.Combine(fi.Directory.FullName, "SMSService.db");
            connectionstring = string.Format(@"Data Source={0};Version=3;Synchronous=Full;", fileName);
            Initialize();
        }
        public bool Send(SMS entity)
        {
           return SendByWeb(entity);
        }

        public bool UploadVoice(SMS smsmessag)
        {
            try
            {
                //System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                //{
                //    FileName = @"C:\tmp\SendVSMS.exe",
                //    Arguments = "raja123 123456a testdirect14.wav 97910404132",
                //    CreateNoWindow = true,
                //    UseShellExecute = true,
                //    WorkingDirectory = @"c:\temp"
                //});
                Logger.Current.Info("Initializing voice sms " + smsmessag.Voice.username + ":" + smsmessag.Voice.password + ":" + smsmessag.Voice.clipname + " user : "+ System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                UnicelVoiceSMS voicesms = new UnicelVoiceSMS(smsmessag.Voice.username, smsmessag.Voice.password);
                string clipname = smsmessag.Voice.clipname;
                var clip = voicesms.Execute(clipname, smsmessag.Voice.content, clipname.Split('.')[0]);
                Logger.Current.Info("voice sms uploaded " + clip.name + ":" + clip.duration + ":" + clip.size + " - " + clip.uniqueid);
                return clip.success;
            }catch(Exception exp)
            {
                return false;
            }
        }
        public bool SendByWeb(SMS smsmessage)
        {
            string MobileNo = "";
            string body = "";
            try
            {
                if(smsmessage.Voice != null && smsmessage.Voice.content != null)
                {
                    if (!UploadVoice(smsmessage))
                        return false;
                }

                foreach (var item in smsmessage.items)
                {
                    if (!string.IsNullOrEmpty(item.mobile) && item.mobile.Length >= 10)
                    {
                        string message = smsmessage.message;

                        if (smsmessage.arguments != null)
                        {
                            foreach (var args in smsmessage.arguments)
                                message = message.Replace(args.key, args.value);
                        }
                        if (item.arguments != null)
                        {
                            foreach (var args in item.arguments)
                                message = message.Replace(args.key, args.value);
                        }
                        MobileNo = item.mobile;
                        string url = smsmessage.smsserverurl.Replace("{Mobile}", "{0}").Replace("{Message}", "{1}");
                        body = string.Format(url, System.Web.HttpUtility.UrlPathEncode(MobileNo),
                            System.Web.HttpUtility.UrlPathEncode(SMSEscape(message)));
                        string result = HttpGet(body);

                        bool success = result != null && result.ToUpper().Contains(smsmessage.smsconfirmation.ToUpper());
                        if(success)
                        {
                            item.hassent = true;
                        }
                        else
                        {
                            item.error = result;
                        }
                    }
                }
                return true;
            }
            catch (Exception exp)
            {
                
                return false;
            }
        }

        private string SMSEscape(string message)
        {
            message = (message.IndexOf("%") >= 0) ? message.Replace("%", "%25") : message;
            message = (message.IndexOf("&") >= 0) ? message.Replace("&", "%26") : message;
            message = (message.IndexOf("?") >= 0) ? message.Replace("?", "%3F") : message;
            message = (message.IndexOf("\"") >= 0) ? message.Replace("\"", "%22") : message;
            message = (message.IndexOf("<") >= 0) ? message.Replace("<", "%3C") : message;
            message = (message.IndexOf(">") >= 0) ? message.Replace(">", "%3E") : message;
            message = (message.IndexOf("+") >= 0) ? message.Replace("+", "%2B") : message;
            message = (message.IndexOf("#") >= 0) ? message.Replace("#", "%23") : message;
            message = (message.IndexOf("*") >= 0) ? message.Replace("*", "%2A") : message;
            message = (message.IndexOf("!") >= 0) ? message.Replace("!", "%21") : message;
            //message = (message.IndexOf("'") >= 0) ? message.Replace("'", "%2C") : message;
            message = (message.IndexOf("=") >= 0) ? message.Replace("=", "%3D") : message;
            return message;
        }

        private string HttpGet(string URI)
        {

            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();

        }

        private bool Initialize()
        {
            System.Data.SQLite.SQLiteConnection connection = new System.Data.SQLite.SQLiteConnection(connectionstring);
            try
            {
                connection.Open();
                string tbl_stock = @"CREATE TABLE IF NOT EXISTS [SMS](
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Json TEXT NOT NULL)";
                System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(tbl_stock, connection);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }
        public bool Notify(SMS entity)
        {
            ServiceProxy.Instance.UpdateStatus(entity);
            MessageSent(entity);
            return true;
        }

        public SMS MessageSent(SMS entity)
        {
            System.Data.SQLite.SQLiteConnection connection = new System.Data.SQLite.SQLiteConnection(connectionstring);
            try
            {
                connection.Open();
                string tbl_insertStock = @"Delete from SMS where id=@id"; ;
                System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(tbl_insertStock, connection);
                command.Parameters.Add("id", System.Data.DbType.Int64);
                command.Parameters["id"].Value = entity.internalid;
                command.ExecuteNonQuery();
                return entity;
            }
            catch (Exception exp)
            {
                return entity;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        public SMS Persist(SMS entity)
        {
            System.Data.SQLite.SQLiteConnection connection = new System.Data.SQLite.SQLiteConnection(connectionstring);
            try
            {
                connection.Open();
                string tbl_insertStock = @"Insert into SMS(JSON) Values(@JSON)";;
                System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(tbl_insertStock, connection);
                command.Parameters.Add("JSON", System.Data.DbType.String);
                command.Parameters["JSON"].Value = entity.ToJSON();
                command.ExecuteNonQuery();
                entity.internalid = connection.LastInsertRowId;
                return entity;
            }
            catch (Exception exp)
            {
                return entity;
            }
            finally
            {
                if(connection.State == System.Data.ConnectionState.Open)
                connection.Close();
            }
        }
    }
}
