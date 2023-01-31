using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Transfer.Table
{
    public partial class Im_GoodsTransferItem
    {
        [Key]
        public Guid GoodsTransferItem_Index { get; set; }
        public Guid GoodsTransfer_Index { get; set; }
        public Guid? GoodsReceive_Index { get; set; }
        public Guid? GoodsReceiveItem_Index { get; set; }
        public Guid? GoodsReceiveItemLocation_Index { get; set; }
        public string LineNum { get; set; }
        public Guid? TagItem_Index { get; set; }
        public Guid? Tag_Index { get; set; }
        public string Tag_No { get; set; }
        public Guid? Tag_Index_To { get; set; }
        public Guid? Product_Index { get; set; }
        public Guid? Product_Index_To { get; set; }
        public string Product_Lot { get; set; }
        public string Product_Lot_To { get; set; }
        public Guid? ItemStatus_Index { get; set; }
        public Guid? ItemStatus_Index_To { get; set; }
        public Guid? ProductConversion_Index { get; set; }
        public string ProductConversion_Id { get; set; }
        public string ProductConversion_Name { get; set; }
        public Guid? Owner_Index { get; set; }
        public Guid? Owner_Index_To { get; set; }
        public Guid? Location_Index { get; set; }
        public Guid? Location_Index_To { get; set; }
        public DateTime? GoodsReceive_MFG_Date { get; set; }
        public DateTime? GoodsReceive_MFG_Date_To { get; set; }
        public DateTime? GoodsReceive_EXP_Date { get; set; }
        public DateTime? GoodsReceive_EXP_Date_To { get; set; }
        public decimal Qty { get; set; }
        public decimal? Ratio { get; set; }
        public decimal? TotalQty { get; set; }

        public decimal? UnitWeight { get; set; }
        public Guid? UnitWeight_Index { get; set; }
        public string UnitWeight_Id { get; set; }
        public string UnitWeight_Name { get; set; }
        public decimal? UnitWeightRatio { get; set; }

        public decimal? Weight { get; set; }
        public Guid? Weight_Index { get; set; }
        public string Weight_Id { get; set; }
        public string Weight_Name { get; set; }
        public decimal? WeightRatio { get; set; }

        public decimal? UnitNetWeight { get; set; }
        public Guid? UnitNetWeight_Index { get; set; }
        public string UnitNetWeight_Id { get; set; }
        public string UnitNetWeight_Name { get; set; }
        public decimal? UnitNetWeightRatio { get; set; }

        public decimal? NetWeight { get; set; }
        public Guid? NetWeight_Index { get; set; }
        public string NetWeight_Id { get; set; }
        public string NetWeight_Name { get; set; }
        public decimal? NetWeightRatio { get; set; }

        public decimal? UnitGrsWeight { get; set; }
        public Guid? UnitGrsWeight_Index { get; set; }
        public string UnitGrsWeight_Id { get; set; }
        public string UnitGrsWeight_Name { get; set; }
        public decimal? UnitGrsWeightRatio { get; set; }

        public decimal? GrsWeight { get; set; }
        public Guid? GrsWeight_Index { get; set; }
        public string GrsWeight_Id { get; set; }
        public string GrsWeight_Name { get; set; }
        public decimal? GrsWeightRatio { get; set; }

        public decimal? UnitWidth { get; set; }
        public Guid? UnitWidth_Index { get; set; }
        public string UnitWidth_Id { get; set; }
        public string UnitWidth_Name { get; set; }
        public decimal? UnitWidthRatio { get; set; }

        public decimal? Width { get; set; }
        public Guid? Width_Index { get; set; }
        public string Width_Id { get; set; }
        public string Width_Name { get; set; }
        public decimal? WidthRatio { get; set; }

        public decimal? UnitLength { get; set; }
        public Guid? UnitLength_Index { get; set; }
        public string UnitLength_Id { get; set; }
        public string UnitLength_Name { get; set; }
        public decimal? UnitLengthRatio { get; set; }

        public decimal? Length { get; set; }
        public Guid? Length_Index { get; set; }
        public string Length_Id { get; set; }
        public string Length_Name { get; set; }
        public decimal? LengthRatio { get; set; }

        public decimal? UnitHeight { get; set; }
        public Guid? UnitHeight_Index { get; set; }
        public string UnitHeight_Id { get; set; }
        public string UnitHeight_Name { get; set; }
        public decimal? UnitHeightRatio { get; set; }

        public decimal? Height { get; set; }
        public Guid? Height_Index { get; set; }
        public string Height_Id { get; set; }
        public string Height_Name { get; set; }
        public decimal? HeightRatio { get; set; }

        public decimal? UnitVolume { get; set; }

        public decimal? Volume { get; set; }

        public decimal? UnitPrice { get; set; }

        public Guid? UnitPrice_Index { get; set; }

        public string UnitPrice_Id { get; set; }
        public string UnitPrice_Name { get; set; }

        public decimal? Price { get; set; }

        public Guid? Price_Index { get; set; }

        public string Price_Id { get; set; }

        public string Price_Name { get; set; }

        public string DocumentRef_No1 { get; set; }
        public string DocumentRef_No2 { get; set; }
        public string DocumentRef_No3 { get; set; }
        public string DocumentRef_No4 { get; set; }
        public string DocumentRef_No5 { get; set; }
        public int? Document_Status { get; set; }
        public string UDF_1 { get; set; }
        public string UDF_2 { get; set; }
        public string UDF_3 { get; set; }
        public string UDF_4 { get; set; }
        public string UDF_5 { get; set; }
        public Guid? Ref_Process_Index { get; set; }
        public string Ref_Document_No { get; set; }
        public Guid? Ref_Document_Index { get; set; }
        public Guid? Ref_DocumentItem_Index { get; set; }
        public string Create_By { get; set; }
        public DateTime? Create_Date { get; set; }
        public string Update_By { get; set; }
        public DateTime? Update_Date { get; set; }
        public string Cancel_By { get; set; }
        public DateTime? Cancel_Date { get; set; }
        public string Tag_No_To { get; set; }
        public string Product_Id { get; set; }
        public string Product_Name { get; set; }
        public string Product_SecondName { get; set; }
        public string Product_ThirdName { get; set; }
        public string Product_Id_To { get; set; }
        public string Product_Name_To { get; set; }
        public string Product_SecondName_To { get; set; }
        public string Product_ThirdName_To { get; set; }
        public string ItemStatus_Id { get; set; }
        public string ItemStatus_Name { get; set; }
        public string ItemStatus_Id_To { get; set; }
        public string ItemStatus_Name_To { get; set; }
        public string Owner_Id { get; set; }
        public string Owner_Name { get; set; }
        public string Owner_Id_To { get; set; }
        public string Owner_Name_To { get; set; }
        public string Location_Id { get; set; }
        public string Location_Name { get; set; }
        public string Location_Id_To { get; set; }
        public string Location_Name_To { get; set; }
        public string Mat_Doc { get; set; }
        public string FI_Doc { get; set; }

        public string ERP_Location { get; set; }
        public string ERP_Location_To { get; set; }
    }
}
