using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.View
{
    public class View_Replenishment_Product
    {
        [Key]
        public Guid Replenishment_Product_Index { get; set; }

        public Guid Replenishment_Index { get; set; }

        public Guid ProductType_Index { get; set; }

        public string ProductType_Id { get; set; }

        public string ProductType_Name { get; set; }

        public Guid? Product_Index { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }
    }
}
