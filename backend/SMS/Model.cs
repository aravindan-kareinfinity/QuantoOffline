using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto.SMS
{

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
        public bool isdirect { get; set; }
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

        public voice Voice { get; set; }
        public DateTime scheduleto { get;  set; }

        public class voice
        {
            public byte[] content { get; set; }
            public string clipname { get; set; }
            public string lablename { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }
    }

}
