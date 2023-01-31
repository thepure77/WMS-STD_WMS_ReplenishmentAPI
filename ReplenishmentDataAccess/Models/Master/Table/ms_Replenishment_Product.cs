using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.Table
{
    public partial class Ms_Replenishment_Product
    {
        [Key]
        public Guid Replenishment_Product_Index { get; set; }

        public Guid Replenishment_Index { get; set; }

        public Guid ProductType_Index { get; set; }

        public Guid? Product_Index { get; set; }
    }
}
