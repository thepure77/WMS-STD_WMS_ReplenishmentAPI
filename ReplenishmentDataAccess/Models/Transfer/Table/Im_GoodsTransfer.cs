using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Transfer.Table
{
    public partial class Im_GoodsTransfer
    {

        [Key]
        public Guid GoodsTransfer_Index { get; set; }

        public Guid Owner_Index { get; set; }

        [StringLength(50)]
        public string Owner_Id { get; set; }

        [StringLength(50)]
        public string Owner_Name { get; set; }

        public Guid DocumentType_Index { get; set; }

        [StringLength(50)]
        public string DocumentType_Id { get; set; }

        [StringLength(200)]
        public string DocumentType_Name { get; set; }

        [StringLength(50)]
        public string GoodsTransfer_No { get; set; }

        public DateTime GoodsTransfer_Date { get; set; }

        public string GoodsTransfer_Time { get; set; }
        public DateTime? GoodsTransfer_Doc_Date { get; set; }

        public string GoodsTransfer_Doc_Time { get; set; }

        [StringLength(200)]
        public string DocumentRef_No1 { get; set; }

        [StringLength(200)]
        public string DocumentRef_No2 { get; set; }

        [StringLength(200)]
        public string DocumentRef_No3 { get; set; }

        [StringLength(200)]
        public string DocumentRef_No4 { get; set; }

        [StringLength(200)]
        public string DocumentRef_No5 { get; set; }

        public int? Document_Status { get; set; }

        [StringLength(200)]
        public string UDF_1 { get; set; }

        [StringLength(200)]
        public string UDF_2 { get; set; }

        [StringLength(200)]
        public string UDF_3 { get; set; }

        [StringLength(200)]
        public string UDF_4 { get; set; }

        [StringLength(200)]
        public string UDF_5 { get; set; }

        public int? DocumentPriority_Status { get; set; }

        [StringLength(200)]
        public string Create_By { get; set; }

        public DateTime? Create_Date { get; set; }

        [StringLength(200)]
        public string Update_By { get; set; }

        public DateTime? Update_Date { get; set; }

        [StringLength(200)]
        public string Cancel_By { get; set; }

        public DateTime? Cancel_Date { get; set; }

        public Guid? Replenishment_Index { get; set; }

        public int? WCS_status { get; set; }
        public DateTime? WCS_Date { get; set; }
    }
}
