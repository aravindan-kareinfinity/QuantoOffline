using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quanto.Offline
{
    public static class BIProxy
    {
        public static async Task<ProcessStatus> GetDownloadedMaster(string serverurl, string clientid, string statuskey)
        {
            using (var client = new HttpClient())
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncBI_Controller/GetDownloadedMaster/" + clientid + "/" + statuskey;
                Logger.Current.Info("Get Status from " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ProcessStatus>(json);
                }
            }
            return null;
        }
        public static async Task<ProcessStatus> StartDownloadMasters(string serverurl,string clientid, string args)
        {
            using (var client = new HttpClient())
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncBI_Controller/StartDownloadMasters/" + clientid + "/" + args;
                Logger.Current.Info("Get Status from " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ProcessStatus>(json);
                }
            }
            return null;
        }

        public static async Task<List<DBSchemaTable>> DownloadSchema(string serverurl, string clientid)
        {
            using (var client = new HttpClient())
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncBI_Controller/DownloadSchema/" + clientid;
                Logger.Current.Info("Get Status from " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<DBSchemaTable>>(json);
                }
            }
            return null;
        }
        /*update BI_PURCHASE set entrydate='2022-04-01',invoicedate='2022-04-01'
        WHERE ENTRYDATE='-infinity':: timestamp without time zone*/
        public static async Task<ProcessStatus> ChangeMasterStatus(string serverurl, string clientid, List<DBData> datas)
        {
            string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncBI_Controller";
            using (var client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 10, 0)
            })
            {
                var creteria = new SyncStatus();
                creteria.clientid = clientid;
                creteria.data = datas;
                datas.ForEach(e => e.datas = null);
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, System.Text.Encoding.UTF8, "application/json");
                var result = Task.Run(() => client.PostAsync(url + "/ChangeMasterStatus", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var masterData = JsonConvert.DeserializeObject<bool>(json);
                }
            }
            return null;
        }

        public class SyncStatus
        {

            public string location { get; set; }
            public string clientid { get; set; }
            public string privatekey { get; set; }
            public bool all { get; set; }
            public long organizationid { get; set; }
            public string organizationcode { get; set; }
            public List<DBData> data { get; set; }
        }

        public class ProcessStatus
        {
            public string key { get; set; }
            public int percentage { get; set; }
            public bool hascompleted { get; set; }
            public bool hasfailed { get; set; }
            public string exception { get; set; }
            
            public List<DBData> result { get; set; }
        }
        public class DBData
        {
            public int type { get; set; }
            public string tablename { get; set; }
            [JsonIgnore]
            public long availablerows { get; set; }
            [JsonIgnore]
            public long completedrows { get; set; }
            public string data { get; set; }
            public byte[] datas { get; set; }
            public List<RowStatus> status { get; set; }

            public List<List<RowStatus>> GetPartionedStatus(int length)
            {
                List<List<RowStatus>> rowStatuses = new List<List<RowStatus>>();
                int startindex = 0;
                while (true)
                {
                    if (startindex + length >= status.Count)
                    {
                        rowStatuses.Add(status.GetRange(startindex, status.Count - startindex));
                        break;
                    }
                    else
                    {
                        rowStatuses.Add(status.GetRange(startindex, length));
                        startindex += length;
                    }
                }
                return rowStatuses;
            }
            public System.Data.DataTable GetTable()
            {
                if (datas != null)
                {
                    return DataTableCustomFormatter.Deserialize(datas, true);
                    //JsonSerializer serializer = new JsonSerializer();
                    //serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    //serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()
                    //{
                    //});
                    //serializer.NullValueHandling = NullValueHandling.Ignore;
                    //using (var ms = new MemoryStream(datas))
                    //{
                    //    GZipStream gZipStream = new GZipStream(ms, CompressionMode.Decompress);
                    //    using (MemoryStream msout = new MemoryStream())
                    //    {
                    //        gZipStream.CopyTo(msout);
                    //        msout.Seek(0, SeekOrigin.Begin);
                    //        return JsonHelpers.CreateFromJsonStream<System.Data.DataTable>(msout);
                    //    }

                    //}
                }
                return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(data, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                });
            }
            public string GetStatusJSON(List<RowStatus> status)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(status, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                });
            }
            public string GetStatusJSON()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(status, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                });
            }
            public class RowStatus
            {
                public long id { get; set; }
                public int version { get; set; }
                public string code { get; set; }
            }



        }

        public class DBSchemaTable
        {
            public override string ToString()
            {
                return name;
            }
            public string CreateTableScript()
            {
                if (columns == null || columns.Count == 0) return "";
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                //stringBuilder.Append(@"CREATE TABLE public.agent(");
                foreach (var item in columns)
                {
                    stringBuilder.AppendLine((stringBuilder.Length > 0 ? "," : "") + item.toscript());
                }
                if (string.IsNullOrEmpty(name))
                    return "";
                if (name == "triggers")
                    return "";
                stringBuilder.Insert(0, "CREATE TABLE public." + name + "(");
                stringBuilder.Append(");");
                return stringBuilder.ToString();
            }

            public string CreateDeleteScript()
            {
                if (columns == null || columns.Count == 0 || string.IsNullOrEmpty(name)) return "";


                return string.Format("delete from {0};", name);
            }


            public string name { get; set; }

            public List<Column> columns { get; set; }

            public class Column
            {
                public bool isautoincrement { get; set; }
                public string column_name { get; set; }
                public string data_type { get; set; }
                public int character_maximum_length { get; set; }
                public string defaultvalue { get; set; }
                public int numeric_precision { get; set; }
                public int numeric_scale { get; set; }
                public string toscript()
                {
                    if (",methodName,killedBy,loginOn,logoutOn,".Contains("," + column_name + ","))
                        return character_maximum_length > 0 ?
                            string.Format("\"{0}\" {1} ({2}) {3}", column_name, data_type, character_maximum_length,
                            string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue)) :
                            string.Format("\"{0}\" {1}", column_name, data_type, character_maximum_length,
                            string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));

                    if (numeric_precision > 0 &&
                            numeric_scale > 0)
                        return string.Format("{0} {1} ({2},{3}) {4}", column_name, data_type, numeric_precision, numeric_scale,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));

                    return character_maximum_length > 0 ?
                        string.Format("{0} {1} ({2}) {3}", column_name, data_type, character_maximum_length,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue)) :
                        string.Format("{0} {1} {3}", column_name, data_type, character_maximum_length,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));
                }
            }

        }

        public static NpgsqlConnection CreateConnection()
        {
            var connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyBI"].ConnectionString;
            return new NpgsqlConnection(connstring);
        }

        public static List<DBData.RowStatus> Sync_Masters(string clientcode,DataTable table, string tablename, DBData datatable,
            BackgroundWorker bg, int progress, List<DBData> result)
        {
            if (table == null || table.Rows.Count == 0) return new List<DBData.RowStatus>();
            string columnlistquery = "clientcode";
            string parameterlistquery = "@clientcode";

            columnlistquery = "";
            parameterlistquery = "";


            string updatequery = "";
            int completed = 0;
            List<DBData.RowStatus> returnList = new List<DBData.RowStatus>();
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName.ToLower() != "id")
                    updatequery += (updatequery.Length > 0 ? "," : "update public." + tablename + " SET ") + column.ColumnName + "=@" + column.ColumnName;
                columnlistquery += (columnlistquery.Length > 0 ? "," : "") + column.ColumnName;
                parameterlistquery += (parameterlistquery.Length > 0 ? ",@" : "@") + column.ColumnName;
            }
            string insertQuery = string.Format("INSERT INTO public.{0}({1}) VALUES({2}) RETURNING id", tablename, columnlistquery, parameterlistquery);
            updatequery += " where id=@id";
            List<long> ids = new List<long>();
            List<long> duplicateids = new List<long>();
            foreach (DataRow row in table.Rows)
            {
                ids.Add((long)row["id"]);
            }

            SortedDictionary<string, string> columnTypes = new SortedDictionary<string, string>();
            NpgsqlConnection conn = CreateConnection();
            NpgsqlTransaction transaction = null;
            try
            {
                conn.Open();

                string searchQuery = "Select id from " + tablename + " where id in (" + string.Join<long>(",", ids) + ");"+
                                     "Select data_type,column_name from INFORMATION_SCHEMA.COLUMNS where table_name = '" + tablename.ToLower() + "';";
                NpgsqlCommand command = new NpgsqlCommand(searchQuery, conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    ids.Clear();
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt64(0));
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            columnTypes.Add(reader.GetString(1), reader.GetString(0));
                        }
                    }
                }

                transaction = conn.BeginTransaction();
                foreach (DataRow row in table.Rows)
                {
                    datatable.completedrows = completed++;

                    if (bg != null && datatable.completedrows % 1000 == 0)
                    {
                        bg.ReportProgress(progress, result);
                        System.Threading.Thread.Sleep(500);
                    }

                    long id = (long)row["id"];
                    if (duplicateids.Contains(id))
                        continue;


                    int version = row.IsNull("version") ? 0 : Convert.ToInt32(row["version"]);
                    returnList.Add(new DBData.RowStatus()
                    {
                        id = id,
                        version = version
                    });

                    if (ids.Contains(id)) continue;
                    command = new NpgsqlCommand(ids.Contains(id) ? updatequery : insertQuery, conn, transaction);
                    command.Parameters.Add("clientcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value =clientcode;
                    foreach (DataColumn column in table.Columns)
                    {
                        NpgsqlTypes.NpgsqlDbType type = NpgsqlTypes.NpgsqlDbType.Unknown;
                        if (!columnTypes.ContainsKey(column.ColumnName))
                        {
                            Logger.Current.Error(string.Format("Column type missing {0}", column.ColumnName));
                            Logger.Current.Error(string.Format("Column type keys {0}", string.Join(",", columnTypes.Keys.ToList())));
                            Logger.Current.Error(string.Format("Column type values {0}", string.Join(",", columnTypes.Values.ToList())));
                            type = NpgsqlTypes.NpgsqlDbType.Varchar;
                        }
                        else
                        {
                            switch (columnTypes[column.ColumnName])
                            {
                                case "tsvector":
                                    type = NpgsqlTypes.NpgsqlDbType.TsVector;
                                    command.CommandText = command.CommandText.Replace("@" + column.ColumnName, "to_tsvector(@" + column.ColumnName + ")");
                                    break;
                                case "jsonb":
                                    type = NpgsqlTypes.NpgsqlDbType.Jsonb;
                                    break;
                                case "smallint":
                                    type = NpgsqlTypes.NpgsqlDbType.Smallint;
                                    break;
                                case "timestamp without time zone":
                                    type = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                    break;
                                case "json":
                                    type = NpgsqlTypes.NpgsqlDbType.Json;
                                    break;
                                case "boolean":
                                    type = NpgsqlTypes.NpgsqlDbType.Boolean;
                                    break;
                                case "money":
                                case "numeric":
                                    type = NpgsqlTypes.NpgsqlDbType.Numeric;
                                    break;
                                case "character varying":
                                    type = NpgsqlTypes.NpgsqlDbType.Varchar;
                                    break;
                                case "bytea":
                                    type = NpgsqlTypes.NpgsqlDbType.Bytea;
                                    break;
                                case "integer":
                                    type = NpgsqlTypes.NpgsqlDbType.Integer;
                                    break;
                                case "bit":
                                    type = NpgsqlTypes.NpgsqlDbType.Bit;
                                    break;
                                case "bigint":
                                    type = NpgsqlTypes.NpgsqlDbType.Bigint;
                                    break;
                            }
                        }

                        if (type == NpgsqlTypes.NpgsqlDbType.Bytea)
                            command.Parameters.Add(column.ColumnName, type).Value = DBNull.Value;
                        else if (type == NpgsqlTypes.NpgsqlDbType.TsVector)
                            command.Parameters.Add(column.ColumnName, NpgsqlTypes.NpgsqlDbType.Varchar).Value = CreatTSVector(row[column.ColumnName] as DataTable);
                        else
                            command.Parameters.Add(column.ColumnName, type).Value = row[column.ColumnName];
                    }
                    if (!duplicateids.Contains(id))
                    {
                        command.ExecuteNonQuery();
                        duplicateids.Add(id);
                    }
                }
                transaction.Commit();
                return returnList;
            }
            catch (Exception exp)
            {
                if (transaction != null)
                    transaction.Rollback();
                if (conn != null)
                    conn.Close();
                Logger.Current.Error("Sync_Master failed", exp);
                return null;
            }
        }

        public static string CreatTSVector(DataTable dt)
        {
            if (dt == null)
                return "";
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (DataRow dr in dt.Rows)
                stringBuilder.Append((stringBuilder.Length > 0 ? " " : "") + dr[0].ToString());

            return Regex.Replace(stringBuilder.ToString(), @"[^\w\s]", "", RegexOptions.Compiled);

        }

        static string createdbquery = @"CREATE DATABASE ""{0}""
        WITH
        OWNER = postgres
        ENCODING = 'UTF8'
        LC_COLLATE = 'en_US.UTF-8'
        LC_CTYPE = 'en_US.UTF-8'
        TABLESPACE = pg_default
        CONNECTION LIMIT = -1
        IS_TEMPLATE = False;";
        internal static void Create_Masters()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                
            }
        }

        internal static async Task<bool> ClearServerStatusAsync(string clientid, bool sales, bool purchase, bool stock)
        {
            using (var client = new HttpClient())
            {
                string url = System.Configuration.ConfigurationManager.AppSettings["ServerURL"] + "/SyncBI_Controller/ClearStatus/" + clientid + "/" + sales+"/"+purchase+"/"+stock ;
                Logger.Current.Info("Get Status from " + url);
                var result = Task.Run(() => client.GetAsync(url)).Result;
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
