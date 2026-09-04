using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;

namespace InfyPOS.Data

{
    public class SmsmessageDataService : BaseData
    {
        const string SQL_Search = @"select id, message, institutionid, isactive, modifiedon, createdon, createdby, modifiedby, version, publishto, smscount from smsmessage where id=@id";


        const string SQL_Insert = @"INSERT INTO public.smsmessage(message, institutionid, isactive, modifiedon, createdon, createdby, modifiedby, version, publishto, smscount) VALUES(@message, @institutionid, @isactive, @modifiedon, @createdon, @createdby, @modifiedby, @version, @publishto, @smscount) RETURNING id";


        const string SQL_Update = @"UPDATE public.smsmessage SET message=@message, institutionid=@institutionid, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, publishto=@publishto, smscount=@smscount where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.smsmessage set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Smsmessage> Search(SmsmessageCreteria _smsmessage)
        {
            List<Smsmessage> lst = new List<Smsmessage>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_smsmessage.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _smsmessage.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("institutionid", _smsmessage.institutionid, "=");

                    qb.AddParameter("createdon", _smsmessage.fromdate, ">=");
                    qb.AddParameter("modifiedon", _smsmessage.todate, "<");
                    qb.SortField = "id DESC";

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Smsmessage osmsmessage = new Smsmessage();
                        osmsmessage.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osmsmessage.message = reader["message"] == DBNull.Value ? "" : reader["message"].ToString();
                        osmsmessage.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
                        osmsmessage.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osmsmessage.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osmsmessage.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osmsmessage.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osmsmessage.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osmsmessage.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //osmsmessage.publishto_json = reader["publishto"] == DBNull.Value ? null : reader["publishto"].ToString();
                        osmsmessage.smscount = reader["smscount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["smscount"]);
                        lst.Add(osmsmessage);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, _smsmessage);
            }
            return lst;
        }

        internal bool UpdateStatus(SMS request)
        {
            //UPDATE status
        }

        public Smsmessage Insert(Smsmessage oSmsmessage)
        {
            InstitutionDataService instdata = new InstitutionDataService();
            instdata.Context = this.Context;
            List<Institution> lst = instdata.Search(new InstitutionCreteria { id = oSmsmessage.institutionid });
            if (lst != null && lst.Count > 0) {
                if (lst[0].smscredit < oSmsmessage.publishto.Count)
                    return oSmsmessage;
            }
            if (!string.IsNullOrEmpty(oSmsmessage.message))
                oSmsmessage.message = oSmsmessage.message.Length > 500 ? oSmsmessage.message.Substring(0, 499) : oSmsmessage.message;

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    oSmsmessage.smscount = oSmsmessage.publishto.Count;
                    Insert(transaction, conn, oSmsmessage);
                    if (SMSManager.Instance.SendByWeb(oSmsmessage))
                    {
                        lst[0].smscredit = oSmsmessage.publishto.Count;
                        instdata.UpdateSMSCredit(transaction, conn, lst[0]);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oSmsmessage);
               
            }            
            return oSmsmessage;
        }

        public Smsmessage Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Smsmessage oSmsmessage)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("message", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSmsmessage.message == null ? "" : oSmsmessage.message;
            command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.GetInstitutionID(oSmsmessage.institutionid);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSmsmessage.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.version;
            command.Parameters.Add("publishto", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.publishto_json == null ? "null" : oSmsmessage.publishto_json;
            command.Parameters.Add("smscount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.smscount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSmsmessage.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSmsmessage;
        }

        public Smsmessage Update(Smsmessage oSmsmessage)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    oSmsmessage.smscount = oSmsmessage.publishto.Count;
                    Update(transaction, conn, oSmsmessage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oSmsmessage);
                
            }

            return oSmsmessage;
        }

        public Smsmessage Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Smsmessage oSmsmessage)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.id;
            command.Parameters.Add("message", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSmsmessage.message == null ? "" : oSmsmessage.message;
            command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.GetInstitutionID(oSmsmessage.institutionid);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSmsmessage.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSmsmessage.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.version;
            command.Parameters.Add("publishto", NpgsqlTypes.NpgsqlDbType.Json).Value = oSmsmessage.publishto_json == null ? "null" : oSmsmessage.publishto_json;
            command.Parameters.Add("smscount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.smscount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSmsmessage.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSmsmessage;
        }


        public bool Delete(Smsmessage oSmsmessage)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSmsmessage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oSmsmessage);

                 
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Smsmessage oSmsmessage)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSmsmessage.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTime.Now;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSmsmessage.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSmsmessage.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

    
}
