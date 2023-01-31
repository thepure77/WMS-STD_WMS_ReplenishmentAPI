using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Master.Table
{
    public partial class Ms_ProductConversion
    {
        [Key]
        public Guid ProductConversion_Index { get; set; }

        public Guid Product_Index { get; set; }



        public string Product_Id { get; set; }



        public string Product_Name { get; set; }



        public string ProductConversion_Id { get; set; }



        public string ProductConversion_Name { get; set; }

        public string ProductConversion_SecondName { get; set; }


        public decimal ProductConversion_Ratio { get; set; }


        public decimal? ProductConversion_Weight { get; set; }

        public Guid? ProductConversion_Weight_Index { get; set; }


        public string ProductConversion_Weight_Id { get; set; }


        public string ProductConversion_Weight_Name { get; set; }


        public decimal? ProductConversion_WeightRatio { get; set; }


        public decimal? ProductConversion_GrsWeight { get; set; }

        public Guid? ProductConversion_GrsWeight_Index { get; set; }


        public string ProductConversion_GrsWeight_Id { get; set; }


        public string ProductConversion_GrsWeight_Name { get; set; }


        public decimal? ProductConversion_GrsWeightRatio { get; set; }


        public decimal? ProductConversion_Width { get; set; }

        public Guid? ProductConversion_Width_Index { get; set; }


        public string ProductConversion_Width_Id { get; set; }


        public string ProductConversion_Width_Name { get; set; }


        public decimal? ProductConversion_WidthRatio { get; set; }


        public decimal? ProductConversion_Length { get; set; }

        public Guid? ProductConversion_Length_Index { get; set; }


        public string ProductConversion_Length_Id { get; set; }


        public string ProductConversion_Length_Name { get; set; }


        public decimal? ProductConversion_LengthRatio { get; set; }


        public decimal? ProductConversion_Height { get; set; }

        public Guid? ProductConversion_Height_Index { get; set; }


        public string ProductConversion_Height_Id { get; set; }


        public string ProductConversion_Height_Name { get; set; }


        public decimal? ProductConversion_HeightRatio { get; set; }


        public decimal? ProductConversion_Volume { get; set; }

        public Guid? ProductConversion_Volume_Index { get; set; }


        public string ProductConversion_Volume_Id { get; set; }


        public string ProductConversion_Volume_Name { get; set; }


        public decimal ProductConversion_VolumeRatio { get; set; }


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

        public int? SALE_UNIT { get; set; }
        public int? IN_UNIT { get; set; }


    }
}
