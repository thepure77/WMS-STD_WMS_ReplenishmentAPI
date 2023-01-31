using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Business.Services;
using Business.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/replenishment")]
    [ApiController]
    public class ReplenishmentController : ControllerBase
    {
        [HttpPost("list")]
        public IActionResult ListReplenishment([FromBody]JObject body)
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                var result = service.ListConfigReplenishment(body?.ToString() ?? string.Empty);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("get")]
        public IActionResult GetReplenishment([FromBody]JObject body)
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                var result = service.GetConfigReplenishment(body?.ToString() ?? string.Empty);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("save")]
        public IActionResult SaveReplenishment([FromBody]JObject body)
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                string result =  service.SaveConfigReplenishment(body?.ToString() ?? string.Empty);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("delete")]
        public IActionResult DeleteReplenishment([FromBody]JObject body)
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                bool result = service.DeleteConfigReplenishment(body?.ToString() ?? string.Empty);
                return Ok(result? "Delete Successfully": "Delete failed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("active")]
        public IActionResult ActiveReplenishment()
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                var result = service.ActivateReplenishment();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }


        [HttpPost("activeASRS")]
        public IActionResult ActiveReplenishmentASRS()
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                var result = service.ActivateReplenishmentASRS();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("activePIECEPICK")]
        public IActionResult ActivateReplenishmentPiecePick()
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                var result = service.ActivateReplenishmentPiecePick();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("generateTaskPiecePick")]
        public IActionResult GenerateTaskPiecePick()
        {
            try
            {
                ReplenishmentService service = new ReplenishmentService();
                var result = service.GenerateTaskPiecePick();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #region printOutTraceTransferReplenish
        [HttpPost("printOutTraceTransferReplenish")]
        public IActionResult printOutTracePicking([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishmentService();
                var Models = new TraceTransferModel();
                Models = JsonConvert.DeserializeObject<TraceTransferModel>(body.ToString());
                var result = service.printOutTraceTransferReplenish(Models);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        #endregion

    }
}
