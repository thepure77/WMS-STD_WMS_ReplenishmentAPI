using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.Table
{
    public partial class Ms_ProductLocation
    {
        [Key]
        public Guid ProductLocation_Index { get; set; }

        public string ProductLocation_Id { get; set; }

        public Guid Product_Index { get; set; }

        public Guid Location_Index { get; set; }

        public decimal Qty { get; set; }

        public decimal Replenish_Qty { get; set; }

        public int? IsActive { get; set; }

        public int? IsDelete { get; set; }

        public int? IsSystem { get; set; }

        public int? Status_Id { get; set; }

        public string Create_By { get; set; }

        public DateTime? Create_Date { get; set; }

        public string Update_By { get; set; }

        public DateTime? Update_Date { get; set; }

        public string Cancel_By { get; set; }

        public DateTime? Cancel_Date { get; set; }
    }
}
