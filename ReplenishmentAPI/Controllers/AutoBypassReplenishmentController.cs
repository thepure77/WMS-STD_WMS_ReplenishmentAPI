using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Business.Services;
using Business.Models;
using ReplenishmentBusiness;
using ReplenishmentBusiness.AutoBypassReplenishment;
using ReplenishmentBusiness.AutoBypassReplenishment.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/autoBypassReplenishment")]
    [ApiController]
    public class AutoBypassReplenishmentController : ControllerBase
    {
        [HttpPost("autoBypass")]
        public IActionResult AutoBypass([FromBody]JObject body)
        {
            var LoggingService = new Loging();
            LoggingService.DataLogLines("Auto Bypass", "Auto Bypass", "-- CREATE START : " + DateTime.Now.ToString("yyyy-MM-dd-HHmm ") + " --");
            try
            {
                //var service = new AutoBypassReplenishmentService();
                //var Models = JsonConvert.DeserializeObject<AutoBypassReplenishmentViewModel>(body.ToString());
                //LoggingService.DataLogLines("TBL_IF_WMS_PALLET_INSPECTION", "TBL_IF_WMS_PALLET_INSPECTION", "Request : " + JsonConvert.SerializeObject(body));
                //var result = service.REWORK_CREATE_WMS_TBL_IF_WMS_PALLET_INSPECTION(Models);
                //var response = JsonConvert.SerializeObject(result);
                //LoggingService.DataLogLines("TBL_IF_WMS_PALLET_INSPECTION", "TBL_IF_WMS_PALLET_INSPECTION", "Response : REWORK CREATE SUCCESS = " + response);
                //return Ok(result);
                return Ok("");
            }
            catch (Exception ex)
            {
                LoggingService.DataLogLines("Auto Bypass", "ERROR", "-- " + DateTime.Now.ToString("yyyy-MM-dd-HHmm ") + " --" + Environment.NewLine + ex.ToString());
                return BadRequest(ex);
            }
            finally
            {
                LoggingService.DataLogLines("Auto Bypass", "Auto Bypass", "-- CREATE END : " + DateTime.Now.ToString("yyyy-MM-dd-HHmm ") + " --" + Environment.NewLine);
            }
        }

    }
}
