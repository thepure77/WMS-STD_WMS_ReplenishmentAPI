using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Master.Table
{

    public partial class sp_ReplenishmentOnDemand
    {
        [Key]
        public long RowIndex { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        public decimal? SU_Ratio { get; set; }

        public decimal? BU_Order_Qty { get; set; }

        public decimal? Order_Qty { get; set; }

        public string Order_Unit { get; set; }

        public decimal? SU_Order_Qty { get; set; }

        public string SU_Order_Unit { get; set; }

        public decimal? SU_Weight { get; set; }

        public decimal? SU_GrsWeight { get; set; }

        public decimal? SU_W { get; set; }

        public decimal? SU_L { get; set; }

        public decimal? SU_H { get; set; }

        public int MaxPiecePick { get; set; }

        public int MinPiecePick { get; set; }

        public string IsPiecePick { get; set; }

        public decimal QtyInASRS { get; set; }

        public decimal QtyInPiecePick { get; set; }

        public decimal QtyInRepleLocation { get; set; }

        public decimal QtyInBal { get; set; }

        public decimal? SU_QtyInASRS { get; set; }

        public decimal? SU_QtyInPiecePick { get; set; }

        public decimal? SU_QtyInRepleLocation { get; set; }

        public decimal? SU_QtyInBal { get; set; }

        public decimal? Diff_QtyPiecePickWithOrder { get; set; }

        public decimal? Diff_SU_QtyPiecePickWithOrder { get; set; }

        public decimal? Diff_RepleLocation { get; set; }

        public int ConfigMaxReple { get; set; }

        public int ConfigMinReple { get; set; }

    }
}
