using InfyPOS.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Quanto
{
    [RoutePrefix("TextilePOS")]
    public class CompaniesController : ApiController
    {
        [Route("ListOfPrinters")]
        [HttpGet]
        public  async Task<IHttpActionResult> ListOfPrinters()
        {
            List<Quanto.Printer.PrinterService.Printer> result = await Task<List<Quanto.Printer.PrinterService.Printer>>.Run(() => Quanto.Printer.PrinterService.Instance.ListOfPrinters());
            return this.Ok(result);
        }

        [Route("ListOfRemotePrinters/{ipaddress}/{port}")]
        [HttpGet]
        public async Task<IHttpActionResult> ListOfRemotePrinters(string ipaddress,string port)
        {
            List<Quanto.Printer.PrinterService.Printer> result = await Task<List<Quanto.Printer.PrinterService.Printer>>.Run(() => Quanto.Printer.PrinterService.Instance.ListOfRemotePrinters(ipaddress,port));
            return this.Ok(result);
        }

        [Route("ListOfLocalPrinters")]
        [HttpGet]
        public async Task<IHttpActionResult> ListOfLocalPrinters()
        {
            List<Quanto.Printer.PrinterService.Printer> result = await Task<List<Quanto.Printer.PrinterService.Printer>>.Run(() => Quanto.Printer.PrinterService.Instance.ListOfLocalPrinters());
            return this.Ok(result);
        }

        [Route("License")]
        [HttpGet]
        public async Task<IHttpActionResult> License()
        {
            Quanto.Printer.PrinterService.LocationInfo result = await Task<Quanto.Printer.PrinterService.LocationInfo>.Run(() => Quanto.Printer.PrinterService.Instance.GetLicense());
            return this.Ok(result);
        }
        

        [Route("Transfer2Tally")]
        [HttpPost]
        public async Task<IHttpActionResult> Transfer2Tally(Quanto.TallyVocher document)
        {
            var result = await Task<bool>.Run(() => Quanto.TallyServer.Instance.Transfer(document));
            return this.Ok(result);
        }

        [Route("WebData")]
        [HttpPost]
        public async Task<IHttpActionResult> WebData()
        {
            var formData = System.Text.ASCIIEncoding.ASCII.GetString(Request.Content.ReadAsByteArrayAsync().Result);
            var document = Newtonsoft.Json.JsonConvert.DeserializeObject<Quanto.Data.DataManager.TransferDocument>(formData);
            string result = await Task<string>.Run(() => Quanto.Data.DataManager.Instance.SentWebData(document));
            //return this.Ok(result);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(result, Encoding.UTF8, "application/json");
            return ResponseMessage(response);
        }

        [Route("TransferData")]
        [HttpPost]
        public async Task<IHttpActionResult> TransferData()
        {
            var formData = System.Text.ASCIIEncoding.ASCII.GetString(Request.Content.ReadAsByteArrayAsync().Result);
            var document = Newtonsoft.Json.JsonConvert.DeserializeObject<Quanto.Data.DataManager.TransferDocument>(formData);
            bool result = await Task<bool>.Run(() => Quanto.Data.DataManager.Instance.Sent(document));
            return this.Ok(result);
        }

        [Route("PrintRAW")]
        [HttpPost]
        public async Task<IHttpActionResult> Print()
        {
            var formData = System.Text.ASCIIEncoding.ASCII.GetString(Request.Content.ReadAsByteArrayAsync().Result);
            var document = Newtonsoft.Json.JsonConvert.DeserializeObject<Quanto.Printer.PrinterService.PrintDocument>(formData);
            bool result = await Task<bool>.Run(() => Quanto.Printer.PrinterService.Instance.Print(document));
            return this.Ok(result);
        }

        [Route("PrintPDF")]
        [HttpPost]
        public async Task<IHttpActionResult> PrintPDF()
        {
            var formData = System.Text.ASCIIEncoding.ASCII.GetString(Request.Content.ReadAsByteArrayAsync().Result);
            var document = Newtonsoft.Json.JsonConvert.DeserializeObject<Quanto.Printer.PrinterService.PrintDocument>(formData);
            bool result = await Task<bool>.Run(() => Quanto.Printer.PrinterService.Instance.PrintPDF(document));
            return this.Ok(result);
        }


        [Route("Print")]
        [HttpPost]
        public async Task<IHttpActionResult> Print(Quanto.Printer.PrinterService.PrintDocument document)
        {
            bool result = await Task<bool>.Run(() => Quanto.Printer.PrinterService.Instance.Print(document));
            return this.Ok(result);
        }

        [Route("SendSMS")]
        [HttpPost]
        public async Task<IHttpActionResult> SendSMS(Quanto.SMS.SMS document)
        {
            bool result = await Task<bool>.Run(() => Quanto.SMS.SMSManager.Instance.Sent(document));
            return this.Ok(result);
        }

        [Route("KillSMS/{id}/{mobileno}")]
        [HttpGet]
        public async Task<IHttpActionResult> KillSMS(string id,string mobileno)
        {
            bool result = await Task<bool>.Run(() => Quanto.SMS.SMSManager.Instance.KillSMS(id,mobileno));
            return this.Ok(result);
        }

        [Route("ClientMaster")]
        [HttpGet]
        public async Task<IHttpActionResult> ClientMaster()
        {
            var result = await Task.Run(() =>
            {
                var package = new OfflineClient.ClientMasterPackage();
                var masterPath = System.IO.Path.Combine(BillManager.datadirectory, "master.data");
                if (!System.IO.File.Exists(masterPath))
                {
                    package.error = true;
                    package.errormessage = "Master data is not available on this client.";
                    return package;
                }

                package.master = System.IO.File.ReadAllBytes(masterPath);
                var stockPath = System.IO.Path.Combine(BillManager.datadirectory, "stock.data");
                if (System.IO.File.Exists(stockPath))
                    package.stock = System.IO.File.ReadAllBytes(stockPath);
                var customerPath = System.IO.Path.Combine(BillManager.datadirectory, "customer.data");
                if (System.IO.File.Exists(customerPath))
                    package.customer = System.IO.File.ReadAllBytes(customerPath);
                if (BillManager.Instance.Data != null)
                    package.lastSyncOn = BillManager.Instance.Data.lastSyncOn;
                package.completed = true;
                return package;
            });
            return this.Ok(result);
        }

        [Route("ClientCustomer")]
        [HttpGet]
        public async Task<IHttpActionResult> ClientCustomer()
        {
            var result = await Task.Run(() =>
            {
                var package = new OfflineClient.ClientMasterPackage();
                var customerPath = System.IO.Path.Combine(BillManager.datadirectory, "customer.data");
                if (!System.IO.File.Exists(customerPath))
                {
                    package.error = true;
                    package.errormessage = "Customer data is not available on this client.";
                    return package;
                }

                package.customer = System.IO.File.ReadAllBytes(customerPath);
                package.completed = true;
                return package;
            });
            return this.Ok(result);
        }

    }
}
