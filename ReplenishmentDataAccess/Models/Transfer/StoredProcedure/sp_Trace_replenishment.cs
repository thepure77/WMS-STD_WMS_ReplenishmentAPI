using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Transfer.StoredProcedure
{
    public partial class sp_Trace_replenishment
    {
        [Key]
        public long? RowIndex { get; set; }
        public string GoodsTransfer_No { get; set; }
        public string ERP_Location { get; set; }
        public string Tag_No { get; set; }
        public string Product_Id { get; set; }
        public string Product_Name { get; set; }
        public string Product_Lot { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Ratio { get; set; }
        public decimal? TotalQty { get; set; }
        public string ProductConversion_Name { get; set; }
        public string Location_Id { get; set; }
        public string Location_Id_To { get; set; }
        public DateTime? GoodsTransfer_Date { get; set; }
        public Guid? GoodsReceiveItemLocation_Index { get; set; }
        public string ERP_Location_To { get; set; }
        public Guid? ProcessStatus_Index { get; set; }
        public string ProcessStatus_Id { get; set; }
        public string ProcessStatus_Name { get; set; }
        //public string GT_Status { get; set; }
        //public string GTI_Status { get; set; }
        //public string T_Status { get; set; }
        //public string TI_Status { get; set; }

        public decimal? Remaining { get; set; }
        public string Unit_Remaining { get; set; }
        public decimal? Total { get; set; }
        public string Unit_Total { get; set; }
        public string Create_By { get; set; }
        public DateTime? Create_Date { get; set; }
        public string Update_By { get; set; }
        public DateTime? Update_Date { get; set; }
        public string DocumentType_Id { get; set; }
    }
}
