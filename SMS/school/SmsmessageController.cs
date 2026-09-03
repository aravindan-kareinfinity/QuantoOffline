
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("SmsmessageService")]
    public class SmsmessageServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Smsmessage>();
            result.Item = new Smsmessage();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.sms, Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<SmsmessageCreteria> request)
        {
            SmsmessageDataService SmsmessageData = new SmsmessageDataService();
            List<Smsmessage> result = await Task<List<Smsmessage>>.Run(() => SmsmessageData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.sms, Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Smsmessage> request)
        {
            SmsmessageDataService SmsmessageData = new SmsmessageDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Smsmessage result = await Task<Smsmessage>.Run(() => SmsmessageData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Smsmessage result = await Task<Smsmessage>.Run(() => SmsmessageData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.sms, Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Smsmessage> request)
        {
            SmsmessageDataService SmsmessageData = new SmsmessageDataService();
            Smsmessage result = await Task<Smsmessage>.Run(() => SmsmessageData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.sms, Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Smsmessage> request)
        {
            SmsmessageDataService SmsmessageData = new SmsmessageDataService();
            Smsmessage result = await Task<Smsmessage>.Run(() => SmsmessageData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> UpdateStatus(SMS request)
        {
            SmsmessageDataService SmsmessageData = new SmsmessageDataService();
            bool result = await Task<bool>.Run(() => SmsmessageData.UpdateStatus(request));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.sms, Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Smsmessage> request)
        {
            SmsmessageDataService SmsmessageData = new SmsmessageDataService();
            bool result = await Task<bool>.Run(() => SmsmessageData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}