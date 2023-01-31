using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Master.Table
{

    public partial class View_Replenishment_Config
    {
        [Key]
        public Guid Replenishment_Index { get; set; }

        public string Replenishment_Id { get; set; }

        public string Replenishment_Remark { get; set; }

        public int IsActive { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        public string Location_Name { get; set; }

        public decimal Qty { get; set; }

        public decimal? Replenish_Qty { get; set; }

        public decimal Min_Qty { get; set; }

        public string LocType { get; set; }

        public Guid Replenishment_Product_Index { get; set; }

        public Guid Replenishment_Location_Index { get; set; }

        public Guid ProductLocation_Index { get; set; }

        public Guid Product_Index { get; set; }

        public Guid Location_Index { get; set; }

        public string Sale_Unit { get; set; }

    }
}
