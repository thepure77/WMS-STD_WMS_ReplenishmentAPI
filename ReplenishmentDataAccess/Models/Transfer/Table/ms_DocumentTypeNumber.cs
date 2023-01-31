using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Transfer.Table
{
    public partial class Ms_DocumentTypeNumber
    {
        [Key]
        public Guid DocumentTypeNumber_Index { get; set; }

        public Guid DocumentType_Index { get; set; }

        public string DocumentTypeNumber_Year { get; set; }

        public string DocumentTypeNumber_Month { get; set; }

        public string DocumentTypeNumber_Day { get; set; }

        public int DocumentTypeNumber_Running { get; set; }

        public int? IsActive { get; set; }

        public int? IsDelete { get; set; }

        public int? IsSystem { get; set; }

        public int? Status_Id { get; set; }

        [StringLength(200)]
        public string Create_By { get; set; }

        public DateTime? Create_Date { get; set; }

        [StringLength(200)]
        public string Update_By { get; set; }

        public DateTime? Update_Date { get; set; }

        [StringLength(200)]
        public string Cancel_By { get; set; }

        public DateTime? Cancel_Date { get; set; }



    }
}
