using System;
using System.Collections.Generic;
using System.Text;

namespace ReplenishmentBusiness.AutoBypassReplenishment.Models
{
    public class AutoBypassReplenishmentViewModel
    {
        public string rowId { get; set; }
        public short product_Id { get; set; }
        public DateTime create_Date { get; set; }
        public string create_By { get; set; }
    }
}
