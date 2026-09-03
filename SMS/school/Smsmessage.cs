using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Smsmessage : BaseEntity
    {
        public long id { get; set; }
        public string message { get; set; }
        public long institutionid { get; set; }
        public bool isactive { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int smscount { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string publishto_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(publishto); }
            set {
                if (value != null)
                    publishto = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Publishto>>(value);
            }
        }
        public List<Publishto> publishto { get; set; }
        
    }
    public class SmsmessageCreteria : Smsmessage
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }


    public class SMS
    {
        public string message { get; set; }
        public long id { get; set; }
        public long internalid { get; set; }
        public string senderid { get; set; }
        public string smsserverurl { get; set; }
        public string sendmethod { get; set; }
        public string smsconfirmation { get; set; }
        public List<Item> items { get; set; }

        public List<KeyValue> arguments { get; set; }
        public class Item
        {
            public string mobile { get; set; }
            public List<KeyValue> arguments { get; set; }
            public bool hassent { get; set; }
            public string error { get; set; }
        }

        public class KeyValue
        {
            public string key { get; set; }
            public string value { get; set; }
        }
    }
}
