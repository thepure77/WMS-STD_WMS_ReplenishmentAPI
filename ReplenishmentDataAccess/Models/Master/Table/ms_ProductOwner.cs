using DataAccess.Models.Master.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MasterDataDataAccess.Models
{

    public partial class MS_ProductOwner
    {
        [Key]
        public Guid ProductOwner_Index { get; set; }

        [StringLength(50)]
        public string ProductOwner_Id { get; set; }

        public Guid? Owner_Index { get; set; }

        public Guid? Product_Index { get; set; }

        public int IsActive { get; set; }

        public int IsDelete { get; set; }

        public int IsSystem { get; set; }

        public int Status_Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Create_By { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Create_Date { get; set; }

        [StringLength(200)]
        public string Update_By { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Update_Date { get; set; }

        [StringLength(200)]
        public string Cancel_By { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime? Cancel_Date { get; set; }


        public string Owner_Id { get; set; }

        public string Owner_Name { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        public string Ref_No1 { get; set; }

        public string Ref_No2 { get; set; }

        public string Ref_No3 { get; set; }

        public string Ref_No4 { get; set; }

        public string Ref_No5 { get; set; }

        public string Remark { get; set; }

        public string UDF_1 { get; set; }

        public string UDF_2 { get; set; }

        public string UDF_3 { get; set; }

        public string UDF_4 { get; set; }

        public string UDF_5 { get; set; }

        public virtual MS_Owner MS_Owner { get; set; }

        public virtual Ms_Product MS_Product { get; set; }
    }
}
