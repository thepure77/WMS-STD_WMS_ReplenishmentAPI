using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Business.Services;
using Business.Models;
using ReplenishmentBusiness.ReplenishOnDemand;
using ReplenishmentBusiness.Models;
using Business.Models.Binbalance;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/replenishOnDemand")]
    [ApiController]
    public class ReplenishOnDemandController : ControllerBase
    {
        #region dropdownRoundWave
        [HttpPost("dropdownRoundWave")]
        public IActionResult dropdownRoundWave([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var Models = new RoundWaveViewModel();
                Models = JsonConvert.DeserializeObject<RoundWaveViewModel>(body.ToString());
                var result = service.dropdownRoundWave(Models);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
        #endregion

        #region filterOndemand
        [HttpPost("filterReplenishOnDemand")]
        public IActionResult filterReplenishOnDemand([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var Models = new FilterReplenishOnDemandViewModel();
                Models = JsonConvert.DeserializeObject<FilterReplenishOnDemandViewModel>(body.ToString());
                var result = service.filterReplenishOnDemand(Models);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
        #endregion

        #region activateBypassReplenishmentFromASRS
        [HttpPost("activateBypassReplenishmentFromASRS")]
        public IActionResult activateBypassReplenishmentFromASRS([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var Models = new FilterReplenishOnDemandViewModel();
                Models = JsonConvert.DeserializeObject<FilterReplenishOnDemandViewModel>(body.ToString());
                var result = service.ActivateBypassReplenishmentFromASRS(Models);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
        #endregion

        #region activePIECEPICKOnDemand
        [HttpPost("activePIECEPICKOnDemand")]
        public IActionResult activePIECEPICKOnDemand([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var Models = new FilterReplenishOnDemandViewModel();
                Models = JsonConvert.DeserializeObject<FilterReplenishOnDemandViewModel>(body.ToString());
                var result = service.ActivateReplenishmentPiecePickOndemand(Models);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex.Message);
            }
        }
        #endregion

        #region getTranferOnDemand
        [HttpGet("getTranferOnDemand")]
        public IActionResult getTranferOnDemand()
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var result = service.getTranferOnDemand();
                return this.Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex);
            }
        }
        #endregion

        #region confirmTransferAndSendWCSBypass
        [HttpPost("confirmTransferAndSendWCSBypass")]
        public IActionResult confirmTransferAndSendWCSBypass([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var Models = new GoodsTransferViewModel();
                Models = JsonConvert.DeserializeObject<GoodsTransferViewModel>(body.ToString());
                var result = service.confirmTransferAndSendWCSBypass(Models);
                return this.Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex);
            }
        }
        #endregion

        #region calculatePalletBypass
        [HttpPost("calculatePalletBypass")]
        public IActionResult CalculatePalletBypass([FromBody]JObject body)
        {
            try
            {
                var service = new ReplenishOnDemandService();
                var Models = new SearchReplenishmentBalanceModel();
                Models = JsonConvert.DeserializeObject<SearchReplenishmentBalanceModel>(body.ToString());
                var result = service.CalculatePalletBypass(Models);
                return this.Ok(result);

            }
            catch (Exception ex)
            {
                return this.BadRequest(ex);
            }
        }
        #endregion
    }
}
