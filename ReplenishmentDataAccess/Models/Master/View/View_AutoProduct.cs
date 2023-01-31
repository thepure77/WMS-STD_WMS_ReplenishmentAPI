using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.View
{
    public class View_AutoProduct
    {
        [Key]
        public Guid Product_Index { get; set; }
        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        public string ProductConversion_Name { get; set; }
        public int? SALE_UNIT { get; set; }
        public string Ref_No1 { get; set; }
    }
}
