using InfyPOS.Processors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Quanto
{
    [Route("TextilePOS")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        [HttpGet("ListOfPrinters")]
        public async Task<ActionResult> ListOfPrinters()
        {
            List<Quanto.Printer.PrinterService.Printer> result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.ListOfPrinters());
            return Ok(result);
        }

        [HttpGet("ListOfRemotePrinters/{ipaddress}/{port}")]
        public async Task<ActionResult> ListOfRemotePrinters(string ipaddress, string port)
        {
            List<Quanto.Printer.PrinterService.Printer> result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.ListOfRemotePrinters(ipaddress, port));
            return Ok(result);
        }

        [HttpGet("ListOfLocalPrinters")]
        public async Task<ActionResult> ListOfLocalPrinters()
        {
            List<Quanto.Printer.PrinterService.Printer> result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.ListOfLocalPrinters());
            return Ok(result);
        }

        [HttpGet("License")]
        public async Task<ActionResult> License()
        {
            Quanto.Printer.PrinterService.LocationInfo result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.GetLicense());
            return Ok(result);
        }

        [HttpPost("Transfer2Tally")]
        public async Task<ActionResult> Transfer2Tally([FromBody] Quanto.TallyVocher document)
        {
            var result = await Task.Run(() => Quanto.TallyServer.Instance.Transfer(document));
            return Ok(result);
        }

        [HttpPost("WebData")]
        public async Task<ContentResult> WebData()
        {
            using var reader = new StreamReader(Request.Body, Encoding.ASCII);
            var formData = await reader.ReadToEndAsync();
            var document = JsonConvert.DeserializeObject<Quanto.Data.DataManager.TransferDocument>(formData);
            string result = await Task.Run(() => Quanto.Data.DataManager.Instance.SentWebData(document));
            return Content(result, "application/json", Encoding.UTF8);
        }

        [HttpPost("TransferData")]
        public async Task<ActionResult> TransferData()
        {
            using var reader = new StreamReader(Request.Body, Encoding.ASCII);
            var formData = await reader.ReadToEndAsync();
            var document = JsonConvert.DeserializeObject<Quanto.Data.DataManager.TransferDocument>(formData);
            bool result = await Task.Run(() => Quanto.Data.DataManager.Instance.Sent(document));
            return Ok(result);
        }

        [HttpPost("PrintRAW")]
        public async Task<ActionResult> Print()
        {
            using var reader = new StreamReader(Request.Body, Encoding.ASCII);
            var formData = await reader.ReadToEndAsync();
            var document = JsonConvert.DeserializeObject<Quanto.Printer.PrinterService.PrintDocument>(formData);
            bool result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.Print(document));
            return Ok(result);
        }

        [HttpPost("PrintPDF")]
        public async Task<ActionResult> PrintPDF()
        {
            using var reader = new StreamReader(Request.Body, Encoding.ASCII);
            var formData = await reader.ReadToEndAsync();
            var document = JsonConvert.DeserializeObject<Quanto.Printer.PrinterService.PrintDocument>(formData);
            bool result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.PrintPDF(document));
            return Ok(result);
        }

        [HttpPost("Print")]
        public async Task<ActionResult> Print([FromBody] Quanto.Printer.PrinterService.PrintDocument document)
        {
            bool result = await Task.Run(() => Quanto.Printer.PrinterService.Instance.Print(document));
            return Ok(result);
        }

        [HttpPost("SendSMS")]
        public async Task<ActionResult> SendSMS([FromBody] Quanto.SMS.SMS document)
        {
            bool result = await Task.Run(() => Quanto.SMS.SMSManager.Instance.Sent(document));
            return Ok(result);
        }

        [HttpGet("KillSMS/{id}/{mobileno}")]
        public async Task<ActionResult> KillSMS(string id, string mobileno)
        {
            bool result = await Task.Run(() => Quanto.SMS.SMSManager.Instance.KillSMS(id, mobileno));
            return Ok(result);
        }

        [HttpGet("ClientMaster")]
        public async Task<ActionResult> ClientMaster()
        {
            var result = await Task.Run(() =>
            {
                var package = new OfflineClient.ClientMasterPackage();
                if (!OfflineDataStore.Instance.Exists(OfflineDataFile.Master))
                {
                    package.error = true;
                    package.errormessage = "Master data is not available on this client.";
                    return package;
                }

                package.master = OfflineDataStore.Instance.ReadAllBytes(OfflineDataFile.Master);
                if (OfflineDataStore.Instance.Exists(OfflineDataFile.Stock))
                    package.stock = OfflineDataStore.Instance.ReadAllBytes(OfflineDataFile.Stock);
                if (OfflineDataStore.Instance.Exists(OfflineDataFile.Customer))
                    package.customer = OfflineDataStore.Instance.ReadAllBytes(OfflineDataFile.Customer);
                if (BillManager.Instance.Data != null)
                    package.lastSyncOn = BillManager.Instance.Data.lastSyncOn;
                package.completed = true;
                return package;
            });
            return Ok(result);
        }

        /// <summary>
        /// CLIENT reads today's (or a chosen date's) bills from MASTER so Available Bills matches the MASTER screen.
        /// </summary>
        [HttpGet("ClientBills/{yyyymmdd}")]
        public async Task<ActionResult> ClientBills(string yyyymmdd)
        {
            var result = await Task.Run(() =>
            {
                var payload = new OfflineClient.ClientUploadResult { bills = new List<OfflineClient.Bill>() };
                if (!MachineConfig.IsMaster)
                {
                    payload.error = true;
                    payload.completed = true;
                    payload.errormessage = "This computer is not configured as MASTER.";
                    return payload;
                }
                DateTime date;
                if (!DateTime.TryParseExact(yyyymmdd, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    payload.error = true;
                    payload.completed = true;
                    payload.errormessage = "Invalid bill date.";
                    return payload;
                }
                payload.bills = BillManager.Instance.GetBillsForDate(date);
                payload.completed = true;
                return payload;
            });
            return Ok(result);
        }

        [HttpGet("ClientStock")]
        public async Task<ActionResult> ClientStock([FromQuery] string barcode)
        {
            var result = await Task.Run(() =>
            {
                var payload = new OfflineClient.ClientStockResult();
                if (!MachineConfig.IsMaster)
                {
                    payload.error = true;
                    payload.errormessage = "This computer is not configured as MASTER.";
                    return payload;
                }
                if (string.IsNullOrWhiteSpace(barcode))
                    return payload;
                try
                {
                    BillManager.Instance.EnsureMasterBusinessDataLoaded();
                    var stock = BillManager.Instance.FindStockLocal(barcode);
                    payload.found = stock != null;
                    payload.stock = stock;
                    return payload;
                }
                catch (Exception exp)
                {
                    payload.error = true;
                    payload.errormessage = exp.Message;
                    return payload;
                }
            });
            return Ok(result);
        }

        [HttpGet("ClientCustomerLookup")]
        public async Task<ActionResult> ClientCustomerLookup([FromQuery] string mobile)
        {
            var result = await Task.Run(() =>
            {
                var payload = new OfflineClient.ClientCustomerResult();
                if (!MachineConfig.IsMaster)
                {
                    payload.error = true;
                    payload.errormessage = "This computer is not configured as MASTER.";
                    return payload;
                }
                try
                {
                    BillManager.Instance.EnsureTransactionFilesLoaded();
                    BillManager.CustomerManager.Instance.Reload();
                    var customer = BillManager.CustomerManager.Instance.Get(mobile);
                    payload.found = customer != null;
                    payload.customer = customer;
                    return payload;
                }
                catch (Exception exp)
                {
                    payload.error = true;
                    payload.errormessage = exp.Message;
                    return payload;
                }
            });
            return Ok(result);
        }

        [HttpGet("ClientCustomer")]
        public async Task<ActionResult> ClientCustomer()
        {
            var result = await Task.Run(() =>
            {
                var package = new OfflineClient.ClientMasterPackage();
                if (!OfflineDataStore.Instance.Exists(OfflineDataFile.Customer))
                {
                    package.error = true;
                    package.errormessage = "Customer data is not available on this client.";
                    return package;
                }

                package.customer = OfflineDataStore.Instance.ReadAllBytes(OfflineDataFile.Customer);
                package.completed = true;
                return package;
            });
            return Ok(result);
        }

        /// <summary>
        /// CLIENT writes bills into MASTER's bill.data via HTTP (MASTER role only).
        /// </summary>
        [HttpPost("UploadClientBills")]
        public async Task<ActionResult> UploadClientBills([FromBody] OfflineClient.ClientBillUpload upload)
        {
            var result = await Task.Run(() =>
            {
                if (!MachineConfig.IsMaster)
                {
                    return new OfflineClient.ClientUploadResult
                    {
                        error = true,
                        completed = true,
                        errormessage = "This computer is not configured as MASTER."
                    };
                }
                if (upload == null || upload.bills == null)
                {
                    return new OfflineClient.ClientUploadResult
                    {
                        error = true,
                        completed = true,
                        errormessage = "No bills in request."
                    };
                }
                return BillManager.Instance.AcceptRemoteBills(upload.bills, upload.deviceId);
            });
            return Ok(result);
        }

        /// <summary>
        /// CLIENT writes settlements into MASTER's settlement.data via HTTP (MASTER role only).
        /// </summary>
        [HttpPost("UploadClientSettlements")]
        public async Task<ActionResult> UploadClientSettlements([FromBody] OfflineClient.ClientSettlementUpload upload)
        {
            var result = await Task.Run(() =>
            {
                if (!MachineConfig.IsMaster)
                {
                    return new OfflineClient.ClientUploadResult
                    {
                        error = true,
                        completed = true,
                        errormessage = "This computer is not configured as MASTER."
                    };
                }
                if (upload == null || upload.settlements == null)
                {
                    return new OfflineClient.ClientUploadResult
                    {
                        error = true,
                        completed = true,
                        errormessage = "No settlements in request."
                    };
                }
                return BillManager.Instance.AcceptRemoteSettlements(upload.settlements, upload.deviceId);
            });
            return Ok(result);
        }
    }
}
