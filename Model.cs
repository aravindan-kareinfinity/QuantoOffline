using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    public class ActionRequest<T>
    {
        public T Item { get; set; }
    }


    public class LicenseInfo
    {
        public bool passcode { get; set; }
        public bool disabled { get; set; }
        public string publickey { get; set; }
        public bool created { get; set; }
        public bool exist { get; set; }
    }

    public class PrinterConfig
    {
        public string companycode { get; set; }
        public string locationcode { get; set; }
        public string server { get; set; }
        public string port { get; set; }
        public string ipaddress { get; set; }
        public string licensekey { get; set; }
        public string computername { get; set; }
        public string systemrole { get; set; }
        public string employeecode { get; set; }
        public string passcode { get; set; }
        public string staticip { get; set; }
    }
    
}
