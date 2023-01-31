using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MasterDataBusiness.BusinessUnit
{
    public class ConfigPiecepickItemViewModel
    {
        [Key]
        public Guid replenishment_Index { get; set; }

        public string replenishment_Id { get; set; }

        public string replenishment_Remark { get; set; }

        public int isActive { get; set; }

        public string product_Id { get; set; }

        public string product_Name { get; set; }

        public string location_Name { get; set; }

        public string show_location_Name { get; set; }

        public decimal qty { get; set; }

        public decimal? replenish_Qty { get; set; }

        public decimal? show_replenish_Qty { get; set; }
        
        public decimal min_Qty { get; set; }
        public decimal show_min_Qty { get; set; }

        public string locType { get; set; }

        public Guid replenishment_Product_Index { get; set; }

        public Guid replenishment_Location_Index { get; set; }

        public Guid productLocation_Index { get; set; }

        public Guid product_Index { get; set; }

        public Guid location_Index { get; set; }

        public string sale_Unit { get; set; }

        public string key { get; set; }

        public string create_By { get; set; }
    }
}
