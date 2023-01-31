using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.View
{
    public class View_ProductLocation
    {
        [Key]
        public Guid ProductLocation_Index { get; set; }
        public string ProductLocation_Id { get; set; }

        public Guid Product_Index { get; set; }
        public string Product_Id { get; set; }
        public string Product_Name { get; set; }

        public Guid Location_Index { get; set; }
        public string Location_Id { get; set; }
        public string Location_Name { get; set; }

        public int? IsActive { get; set; }

        public int? IsDelete { get; set; }

        public decimal Qty { get; set; }

        public decimal Replenish_Qty { get; set; }
    }
}
