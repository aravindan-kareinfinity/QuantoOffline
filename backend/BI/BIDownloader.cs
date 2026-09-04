using Newtonsoft.Json;
using Quanto.Offline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quanto.Offline.BISync;

namespace Quanto.BI
{
    internal class BIDownloader
    {
        public class DownloadList
        {
            public bool stock { get; set; }
            public bool sales { get; set; }
            public bool purchase { get; set; }
        }
        System.Timers.Timer timer = new System.Timers.Timer();
        public void Start(object config)
        {
            var minutes = 60 - DateTime.Now.Minute;
            System.Threading.Thread.Sleep(1000);
            timer.Enabled = true;
            timer.Interval = (new TimeSpan(0, minutes, 0)).TotalMilliseconds;
            timer.Elapsed += FirstTimer_Elapsed;
            timer.Start();
            Process();
        }

        private void FirstTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Process();
            timer.Enabled = false;
            timer.Stop();
            timer = new System.Timers.Timer();
            var hours = int.Parse(System.Configuration.ConfigurationManager.AppSettings["BIDownloadTimer"]);
            timer.Interval = (new TimeSpan(hours, 0, 0)).TotalMilliseconds;
            timer.Elapsed += Timer_Elapsed; ;
            timer.Start();

        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Process();
        }

        int reference = 0;
        public void Process()
        {
            if(System.Threading.Interlocked.Increment(ref reference)>1)
            {
                System.Threading.Interlocked.Decrement(ref reference);
                return;
            }
            try
            {
                string BIClientCode = System.Configuration.ConfigurationManager.AppSettings["BIClientCode"];

                Logger.Current.Info("Downloading process started - " + BIClientCode);
                Console.WriteLine("Downloading process started - " + BIClientCode);
                var result = BIProxy.StartDownloadMasters(System.Configuration.ConfigurationManager.AppSettings["ServerURL"] ,
                    BIClientCode, Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(new DownloadList() { purchase = true, sales = true, stock = true })))).Result;
                var key = result.key;
                while (!result.hasfailed && !result.hascompleted)
                {
                    System.Threading.Thread.Sleep(10000);
                    result = BIProxy.GetDownloadedMaster(System.Configuration.ConfigurationManager.AppSettings["ServerURL"] ,
                        BIClientCode, key).Result;
                    Logger.Current.Info("Downloading process status - ");
                    Console.WriteLine("Downloading process status - ");
                    if (result.hasfailed)
                    {
                        Logger.Current.Info("Downloading process status - Failed" + result.exception);
                        Console.WriteLine("Downloading process status - Failed" + result.exception);
                    }
                    else if (result.hascompleted)
                    {
                        Logger.Current.Info("Downloading process status - Completed (" + result.result.Count + ")");
                        Console.WriteLine("Downloading process status - Completed (" + result.result.Count + ")");
                    }
                    else
                    {
                        Logger.Current.Info("Downloading process status - Processing" + result.percentage);
                        Console.WriteLine("Downloading process status - Processing" + result.percentage);
                    }
                }


                if (result.hascompleted)
                {
                    foreach (var datatable in result.result)
                    {
                        datatable.tablename = "bi_" + datatable.tablename.ToLower();
                        var dttable = datatable.GetTable();
                        Logger.Current.Info("Synching " + datatable.tablename+" rows "+dttable.Rows.Count);
                        Console.WriteLine("Synching " + datatable.tablename + " rows " + dttable.Rows.Count);
                        datatable.status = BIProxy.Sync_Masters(BIClientCode,datatable.GetTable(), datatable.tablename, datatable,null,9,null);
                        Logger.Current.Info("Synching " + datatable.tablename + " rows " + dttable.Rows.Count+" Completed");
                        Console.WriteLine("Synching " + datatable.tablename + " rows " + dttable.Rows.Count + " Completed");
                    }
                    Logger.Current.Info("Updating server status");
                    Console.WriteLine("Updating server status");
                    var status = BIProxy.ChangeMasterStatus(System.Configuration.ConfigurationManager.AppSettings["ServerURL"] ,BIClientCode, result.result);
                    Logger.Current.Info("Updating server status completed");
                    Console.WriteLine("Updating server status completed");
                    Logger.Current.Info("Shutting down properly with new changes");
                    Console.WriteLine("Shutting down properly with new changes");
                }
                else
                {
                    Logger.Current.Info("Shutting down without any new changes");
                    Console.WriteLine("Shutting down without any new changes");
                }
                System.Threading.Interlocked.Decrement(ref reference);
            }
            catch(Exception exp )
            {
                System.Threading.Interlocked.Decrement(ref reference);
                Logger.Current.Error(exp);
                Console.WriteLine(exp.Message);
                Logger.Current.Info("Shutting down with exception");
                Console.WriteLine("Shutting down with exception");
                throw exp;
            }
        }
    }
}
