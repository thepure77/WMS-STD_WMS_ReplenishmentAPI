using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Business.Commons;

namespace Business.Models
{
    public class ReplenishmentViewModel
    {
        [Key]
        public Guid? Replenishment_Index { get; set; }

        public string Replenishment_Id { get; set; }

        public string Replenishment_Remark { get; set; }

        public TimeSpan Trigger_Time { get; set; }

        public TimeSpan Trigger_Time_End { get; set; }

        public DateTime? Trigger_Date { get; set; }

        public DateTime? Trigger_Date_End { get; set; }

        public bool IsMonday { get; set; }

        public bool IsTuesday { get; set; }

        public bool IsWednesday { get; set; }

        public bool IsThursday { get; set; }

        public bool IsFriday { get; set; }

        public bool IsSaturday { get; set; }

        public bool IsSunday { get; set; }

        public int Plan_By_Product { get; set; }

        public int Plan_By_Location { get; set; }

        public int Plan_By_Status { get; set; }

        public int IsActive { get; set; }

        public string Create_By { get; set; }

        public DateTime Create_Date { get; set; }

        public string Update_By { get; set; }

        public DateTime? Update_Date { get; set; }

        public DateTime? Last_Trigger_Date { get; set; }

        public List<ReplenishmentProductViewModel> ReplenishmentProducts { get; set; }

        public List<ReplenishmentLocationViewModel> ReplenishmentLocations { get; set; }
    }

    public class ReplenishmentProductViewModel
    {
        public Guid? Replenishment_Product_Index { get; set; }

        public Guid ProductType_Index { get; set; }

        public string ProductType_Id { get; set; }

        public string ProductType_Name { get; set; }

        public Guid? Product_Index { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }
    }

    public class ReplenishmentLocationViewModel
    {
        public Guid? Replenishment_Location_Index { get; set; }

        public Guid? Zone_Index { get; set; }

        public string Zone_Id { get; set; }

        public string Zone_Name { get; set; }

        public Guid? Location_Index { get; set; }

        public string Location_Id { get; set; }

        public string Location_Name { get; set; }
    }

    public class ListReplenishmentViewModel 
    {
        public List<ReplenishmentViewModel> ReplenishmentViewModels { get; set; }
        public Pagination Pagination { get; set; }
    }

    public partial class View_AssignJobLocViewModel
    {
        public string Template { get; set; }



        public Guid? location_Index { get; set; }
        public string location_Id { get; set; }
        public string location_Name { get; set; }
        public Guid goodsTransfer_Index { get; set; }
        public string goodsTransfer_No { get; set; }
        public Guid goodsTransferItem_Index { get; set; }
        public Guid? warehouse_Index { get; set; }
        public Guid? zone_Index { get; set; }
        public Guid? route_Index { get; set; }
        public string product_Id { get; set; }
        public DateTime? goodsTransfer_Date { get; set; }

        public decimal qty { get; set; }

        public decimal? totalQty { get; set; }



        public string Create_By { get; set; }
    }

    public class Validated_ImportModel : Result
    {
        public Guid Import_GuID { get; set; }

        public List<dynamic> Valid_Data { get; set; } = new List<dynamic>();

        public List<dynamic> Invalid_Data { get; set; } = new List<dynamic>();
    }
}
