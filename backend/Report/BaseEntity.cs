using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;

namespace InfyPOS.Common
{
    public class BaseEntity
    {
        public static DateTime MinDate = new DateTime(1970, 1, 1);
        private static CultureInfo enUS = new CultureInfo("en-US");
        public static string GetDateString(DateTime date)
        {
            return date.ToString("dd-MM-yyyy");
        }
        public static DateTime GetDataFromString(string source)
        {
            DateTime dt;
            if (DateTime.TryParseExact(source, "dd-MM-yyyy", enUS, DateTimeStyles.None, out dt))
                return dt;
            return DateTime.MinValue;
        }
        public enum DataStatus
        {
            Create,
            Update,
            Delete
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public bool HasLastModifiedSearch { get; set; }
        
        static void setInstanceProperty<PROPERTY_TYPE>(object instance, string propertyName, PROPERTY_TYPE value)
        {
            Type type = instance.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public, null, typeof(PROPERTY_TYPE), new Type[0], null);

            propertyInfo.SetValue(instance, value, null);

            return;
        }
        public static TimeSpan ParseTime(string time)
        {
            DateTime dt;
            if (DateTime.TryParseExact(time, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt.TimeOfDay;

            return new TimeSpan();
        }

        public void ChangeStatus(DataStatus ds)
        {
            UserClaim Context = null;
            if (System.Web.HttpContext.Current != null)
            {
                Context = new UserClaim(System.Web.HttpContext.Current.Request.GetOwinContext().Authentication);
            }
            else
            {
                Context = new UserClaim();
                Context.UserID = 0;
            }
            ChangeStatus(ds, Context);
        }
        public void ChangeStatus(DataStatus ds, UserClaim Context)
        {
            Type type = this.GetType();
            PropertyInfo propertyInfo = null;
            if (ds == DataStatus.Create)
            {
                propertyInfo = type.GetProperty("createdon", BindingFlags.Instance | BindingFlags.Public, null, typeof(DateTime), new Type[0], null);
                propertyInfo.SetValue(this, DateTime.Now);
                propertyInfo = type.GetProperty("createdby", BindingFlags.Instance | BindingFlags.Public, null, typeof(long), new Type[0], null);
                propertyInfo.SetValue(this, Context.UserID);
                propertyInfo = type.GetProperty("version", BindingFlags.Instance | BindingFlags.Public, null, typeof(int), new Type[0], null);
                propertyInfo.SetValue(this, (int)1);
                propertyInfo = type.GetProperty("isactive", BindingFlags.Instance | BindingFlags.Public, null, typeof(bool), new Type[0], null);
                propertyInfo.SetValue(this, true);
            }else if(ds == DataStatus.Update)
            {
                propertyInfo = type.GetProperty("modifiedby", BindingFlags.Instance | BindingFlags.Public, null, typeof(long), new Type[0], null);
                propertyInfo.SetValue(this, Context.UserID);
                propertyInfo = type.GetProperty("modifiedon", BindingFlags.Instance | BindingFlags.Public, null, typeof(DateTime), new Type[0], null);
                propertyInfo.SetValue(this, DateTime.Now);
            }
        }

        public virtual string GetMobileNo()
        {
            return "";
        }

        public virtual string GetEmailID()
        {
            return "";
        }
    }

    public class PrimitiveTypes<T> : BaseEntity
    {
        public List<T> Items { get; set; }
        public T Item { get; set; }
    }
    [System.Runtime.Serialization.DataContract]
    public class ActionMulitpleRequest<T> where T : BaseEntity
    {
        [DataMember]
        public string LastUpdate { get; set; }
        public DateTime lastUpdate
        {
            get
            {
                if (string.IsNullOrEmpty(LastUpdate))
                    return BaseEntity.MinDate;
                return DateTime.Parse(LastUpdate);
            }
            set
            {
                LastUpdate = value.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            }
        }
        [DataMember]
        public string SecurityToken { get; set; }

        [DataMember]
        public List<T> Items { get; set; }

        public ActionMulitpleRequest()
        {

        }
        public ActionMulitpleRequest(List<T> items)
        {
            this.Items = items;
        }
    }

    [System.Runtime.Serialization.DataContract]
    public class ReferenceList : BaseEntity
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        public void Parse(DataRow dr)
        {
            ID = dr.IsNull("ID") ? "" : dr["ID"].ToString();
            Name = dr.IsNull("Name") ? "" : dr["Name"].ToString();
        }
    }

    [System.Runtime.Serialization.DataContract]
    public class CommonList : BaseEntity
    {
        [DataMember]
        public long ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        public void Parse(DataRow dr)
        {
            ID = (long)dr["ID"];
            Name = dr.IsNull("Name") ? "" : dr["Name"].ToString();
        }
    }

    [DataContract]
    public class LookAhead
    {
        [DataMember]
        public string total_count { get; set; }
        [DataMember]
        public bool incomplete_results { get; set; }
        [DataMember]
        public List<LookAheadItem> items { get; set; }
    }

    [DataContract]
    public class LookAheadItem
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
    }

    [System.Runtime.Serialization.DataContract]
    public class ActionSingleValueRequest<T>
    {
        [DataMember]
        public T Value { get; set; }
    }

    [System.Runtime.Serialization.DataContract]
    public class ActionRequest<T> where T : BaseEntity
    {

        [DataMember]
        public string LastUpdate { get; set; }
        public DateTime lastUpdate
        {
            get
            {
                if (string.IsNullOrEmpty(LastUpdate))
                    return BaseEntity.MinDate;
                return DateTime.Parse(LastUpdate);
            }
            set
            {
                LastUpdate = value.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            }
        }

        [DataMember]
        public string SecurityToken { get; set; }

        [DataMember]
        public T Item { get; set; }
    }



    public class ListRequest<T> where T : BaseEntity
    {

        [DataMember]
        public string SecurityToken { get; set; }

        [DataMember]
        public T Item { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Parameters { get; set; }
    }

  [System.Runtime.Serialization.DataContract]
    public class GenericResponse<T>
    {

        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        
        [DataMember]
        public T Item { get; set; }

        public GenericResponse()
        {

        }
        public GenericResponse(T item)
        {
            this.Item = item;
            this.Success = true;
        }
        public GenericResponse(bool success, string exception)
        {
            this.Success = success;
            this.ErrorMessage = exception;
        }
    }

    [System.Runtime.Serialization.DataContract]
    public class ActionSingleResponse<T> where T : BaseEntity
    {

        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        //[DataMember]
        //public UserInformation UserInfo { get; set; }

        [DataMember]
        public T Item { get; set; }

        public ActionSingleResponse()
        {

        }
        public ActionSingleResponse(T item)
        {
            this.Item = item;
            this.Success = true;
        }
        public ActionSingleResponse(bool success,string exception)
        {
            this.Success = success;
            this.ErrorMessage = exception;
        }
    }

    [System.Runtime.Serialization.DataContract]
    public class ActionMulitipleResponse<T> where T : BaseEntity
    {
        public ActionMulitipleResponse()
        {
        }
        public ActionMulitipleResponse(T value)
        {
            Items = new List<T>();
            Items.Add(value);
        }
        public ActionMulitipleResponse(List<T> value)
        {
            Items = value;
        }
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }

        //[DataMember]
        //public UserInformation UserInfo { get; set; }

        [DataMember]
        public List<T> Items { get; set; }
    }

    public class UserInformation
    {
        [DataMember]
        public string FullName { get; set; }
        [DataMember]
        public long UserID { get; set; }
        [DataMember]
        public string UserMessage { get; set; }
        [DataMember]
        public string UserRole { get; set; }
    }
    
}