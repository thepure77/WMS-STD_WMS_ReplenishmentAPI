using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccess.Models.Outbound.Table
{
    public class im_PlanGoodsIssueItem
    {
        [Key]
        public Guid PlanGoodsIssueItem_Index { get; set; }

        public Guid PlanGoodsIssue_Index { get; set; }

        public string LineNum { get; set; }

        public Guid? Product_Index { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        public string Product_SecondName { get; set; }

        public string Product_ThirdName { get; set; }

        public string Product_Lot { get; set; }

        public Guid? ItemStatus_Index { get; set; }

        public string ItemStatus_Id { get; set; }

        public string ItemStatus_Name { get; set; }

        public decimal? Qty { get; set; }

        public decimal? Ratio { get; set; }

        public decimal? TotalQty { get; set; }

        public Guid? ProductConversion_Index { get; set; }

        [StringLength(50)]
        public string ProductConversion_Id { get; set; }

        [StringLength(200)]
        public string ProductConversion_Name { get; set; }

        public DateTime? MFG_Date { get; set; }

        public DateTime? EXP_Date { get; set; }

        public decimal? UnitWeight { get; set; }

        public decimal? Weight { get; set; }

        public decimal? UnitWidth { get; set; }

        public decimal? UnitLength { get; set; }

        public decimal? UnitHeight { get; set; }

        public decimal? UnitVolume { get; set; }

        public decimal? Volume { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Price { get; set; }

        public string DocumentRef_No1 { get; set; }

        public string DocumentRef_No2 { get; set; }

        public string DocumentRef_No3 { get; set; }

        public string DocumentRef_No4 { get; set; }

        public string DocumentRef_No5 { get; set; }

        public string DocumentItem_Remark { get; set; }

        public int? Document_Status { get; set; }

        public string UDF_1 { get; set; }

        public string UDF_2 { get; set; }

        public string UDF_3 { get; set; }

        public string UDF_4 { get; set; }

        public string UDF_5 { get; set; }

        public string Create_By { get; set; }

        public DateTime? Create_Date { get; set; }

        public string Update_By { get; set; }

        public DateTime? Update_Date { get; set; }

        public string Cancel_By { get; set; }

        public DateTime? Cancel_Date { get; set; }

        public decimal? QtyBackOrder { get; set; }

        public int? BackOrderStatus { get; set; }

        public string PlanGoodsIssue_Size { get; set; }

        public decimal? QtyQA { get; set; }

        public int? IsQA { get; set; }

        public decimal? Qty_Inner_Pack { get; set; }

        public decimal? Qty_Sup_Pack { get; set; }

        public string ImageUri { get; set; }

        public string ZoneCode { get; set; }

        public string Batch_Id { get; set; }

        public string QA_By { get; set; }

        public DateTime? QA_Date { get; set; }

        public int? RunWave_Status { get; set; }

        public string PlanGoodsIssue_No { get; set; }

        public decimal? CountQty { get; set; }
    }
}
