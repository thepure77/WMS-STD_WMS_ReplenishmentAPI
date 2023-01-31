using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ReplenishmentBusiness.Models
{
    public class FilterReplenishOnDemandViewModel
    {
        public string goodsIssue_Date { get; set; }

        public List<RoundWaveViewModel> round_Wave { get; set; }

        public List<ReplenishOnDemandViewModel> lstReplenishOnDemand { get; set; }
    }

    public class ReplenishOnDemandViewModel
    {
        public string rowIndex { get; set; }

        public string product_Id { get; set; }

        public string product_Name { get; set; }

        public decimal? su_Ratio { get; set; }

        public decimal? bu_Order_Qty { get; set; }

        public decimal? order_Qty { get; set; }

        public string order_Unit { get; set; }

        public decimal? su_Order_Qty { get; set; }

        public string su_Order_Unit { get; set; }

        public decimal? su_Weight { get; set; }

        public decimal? su_GrsWeight { get; set; }

        public decimal? su_W { get; set; }

        public decimal? su_L { get; set; }

        public decimal? su_H { get; set; }

        public int maxPiecePick { get; set; }

        public int minPiecePick { get; set; }

        public string isPiecePick { get; set; }

        public decimal qtyInASRS { get; set; }

        public decimal qtyInPiecePick { get; set; }

        public decimal qtyInRepleLocation { get; set; }

        public decimal qtyInBal { get; set; }

        public decimal? su_QtyInASRS { get; set; }

        public decimal? su_QtyInPiecePick { get; set; }

        public decimal? su_QtyInRepleLocation { get; set; }

        public decimal? su_QtyInBal { get; set; }

        public decimal? diff_QtyPiecePickWithOrder { get; set; }

        public decimal? diff_SU_QtyPiecePickWithOrder { get; set; }

        public decimal? diff_RepleLocation { get; set; }

        public int configMaxReple { get; set; }

        public int configMinReple { get; set; }

        public string pallet_Qty { get; set; }
    }

    public class GoodsTransferViewModel
    {
        [Key]
        public Guid goodsTransfer_Index { get; set; }

        public Guid owner_Index { get; set; }

        public Guid? locationType_Index { get; set; }

        public Guid? tagItem_Index { get; set; }

        public Guid? goodsReceive_Index { get; set; }

        public Guid? goodsReceiveItem_Index { get; set; }

        public Guid? product_Index { get; set; }

        public Guid? itemStatus_Index { get; set; }

        public Guid? productConversion_Index { get; set; }

        public string owner_Id { get; set; }

        public string owner_Name { get; set; }

        public Guid documentType_Index { get; set; }

        public string documentType_Id { get; set; }

        public string documentType_Name { get; set; }

        public int? documentPriority_Status { get; set; }

        public int? document_Status { get; set; }

        public string goodsTransfer_No { get; set; }

        public string goodsTransfer_Date { get; set; }

        public string goodsTransfer_Time { get; set; }

        public string goodsTransfer_Doc_Date { get; set; }

        public string goodsTransfer_Doc_Time { get; set; }

        public string documentRef_No1 { get; set; }

        public string documentRef_No2 { get; set; }

        public string documentRef_No3 { get; set; }

        public string documentRef_No4 { get; set; }

        public string documentRef_No5 { get; set; }

        public string documentRef_Remark { get; set; }

        public string udf_1 { get; set; }

        public string udf_2 { get; set; }

        public string udf_3 { get; set; }

        public string udf_4 { get; set; }

        public string udf_5 { get; set; }

        public string create_By { get; set; }

        public string create_Date { get; set; }

        public string update_By { get; set; }

        public string update_Date { get; set; }

        public string cancel_By { get; set; }

        public string cancel_Date { get; set; }
    }
}
