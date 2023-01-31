using MasterDataBusiness;
using MasterDataBusiness.BusinessUnit;
using MasterDataBusiness.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigPiecepickItemAPI.Controllers
{
    [Route("api/configPiecepickItem")]
    public class ConfigPiecepickItemController : Controller
    {
        [HttpPost("filter")]
        public IActionResult filter([FromBody] JObject body)
        {
            try
            {
                var service = new ConfigPiecepickItemService();
                var Models = new SearchConfigPiecepickItemViewModel();
                Models = JsonConvert.DeserializeObject<SearchConfigPiecepickItemViewModel>(body.ToString());
                var result = service.filter(Models);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("SaveChanges")]
        public IActionResult SaveChanges([FromBody] JObject body)
        {
            try
            {
                var service = new ConfigPiecepickItemService();
                var Models = new ConfigPiecepickItemViewModel();
                Models = JsonConvert.DeserializeObject<ConfigPiecepickItemViewModel>(body.ToString());
                var result = service.SaveChanges(Models);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        
        [HttpGet("find/{id}")]
        public IActionResult find(Guid id)
        {
            try
            {
                var service = new ConfigPiecepickItemService();
                var result = service.find(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("checkLocation")]
        public IActionResult checkLocation([FromBody] JObject body)
        {
            try
            {
                var service = new ConfigPiecepickItemService();
                var Models = new ConfigPiecepickItemViewModel();
                Models = JsonConvert.DeserializeObject<ConfigPiecepickItemViewModel>(body.ToString());
                var result = service.checkLocation(Models);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("SaveImportList")]
        public IActionResult SaveImportList([FromBody] List<ConfigPiecepickItemViewModel> Models)
        {
            try
            {
                var service = new ConfigPiecepickItemService();
                //var Models = new ConfigPiecepickItemViewModel();
                //Models = JsonConvert.DeserializeObject<ConfigPiecepickItemViewModel>(body.ToString());
                var result = service.SaveImportList(Models);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

    }
}
