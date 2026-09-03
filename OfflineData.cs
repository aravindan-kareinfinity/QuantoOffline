using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    public class DataManager
    {
        private static DataManager datamanager;
        public static DataManager Instance
        {
            get
            {
                if(datamanager == null)
                {
                    datamanager = new DataManager();
                    datamanager.Initialize();
                }
                return datamanager;
            }
        }

        public OfflineData Data
        {
            get
            {
                return offlineData;
            }
        }

        OfflineData offlineData;
        private void Initialize()
        {
            offlineData = new OfflineData();
            offlineData.Connect();
        }
    }
    public class OfflineData
    {

        bool connected;
        System.Data.SQLite.SQLiteConnection connection;
        public OfflineData()
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().FullName);
            string fileName = System.IO.Path.Combine(fi.Directory.FullName, "Textile.pos");
            string connectionstring = string.Format(@"Data Source={0};Version=3;Synchronous=Full;", fileName);
            connection = new System.Data.SQLite.SQLiteConnection(connectionstring);
        }

        public bool Connect()
        {
            try
            {
                connection.Open();
                string tbl_stock = @"CREATE TABLE [IF NOT EXISTS] [Stock](
                            Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
                            Barcode TEXT NOT NULL,
                            Json TEXT NOT NULL)";
                string tbl_bill = @"CREATE TABLE [IF NOT EXISTS] [Bill](
                            Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
                            BillNo TEXT NOT NULL,
                            Json TEXT NOT NULL,
                            ActualBillNo TEXT,
                            Status integer)";
                string tbl_config = @"CREATE TABLE [IF NOT EXISTS] [Configuration](
                            Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL UNIQUE,
                            ConfigKey TEXT NOT NULL,
                            Value TEXT NOT NULL)";
                System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(tbl_stock, connection);
                command.ExecuteNonQuery();
                command = new System.Data.SQLite.SQLiteCommand(tbl_bill, connection);
                command.ExecuteNonQuery();
                command = new System.Data.SQLite.SQLiteCommand(tbl_config, connection);
                command.ExecuteNonQuery();
                return true;
            }
            catch(Exception exp)
            {
                return false;
            }
        }

        public bool Inward(Bundles stocklist)
        {
            System.Data.SQLite.SQLiteTransaction transaction = null;
            try
            {
                string tbl_insertStock = @"Delete Stock where Barcode=@Barcode
                                           Insert into Stock(Barcode,JSON) Values(@Barcode,@JSON)";
                transaction = connection.BeginTransaction();
                System.Data.SQLite.SQLiteCommand command = new System.Data.SQLite.SQLiteCommand(tbl_insertStock, connection, transaction);
                command.Parameters.Add("Barcode", System.Data.DbType.String);
                command.Parameters.Add("JSON", System.Data.DbType.String);

                foreach (var item in stocklist.Salablegoods)
                {
                    command.Parameters["Barcode"].Value = item.barcode;
                    command.Parameters["JSON"].Value = item.ToJSON();
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                return true;
            }
            catch(Exception exp)
            {
                if (transaction != null)
                    transaction.Rollback();
                return false;
            }
        }
    }
}
