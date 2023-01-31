using System;
using System.Collections.Generic;

namespace Business.Models.Binbalance
{
    public class SearchReplenishmentBalanceModel
    {
        public Guid? Owner_Index { get; set; }

        public List<Guid> ReplenishLocationIndexs { get; set; }

        public List<Guid> ReplenishItemStatusIndexs { get; set; }

        public List<SearchReplenishmentBalanceItemModel> Items { get; set; } = new List<SearchReplenishmentBalanceItemModel>();
    }

    public class SearchReplenishmentBalanceItemModel
    {
        public Guid Product_Index { get; set; }

        public Guid Location_Index { get; set; }

        public string Location_Id { get; set; }

        public string Location_Name { get; set; }

        public decimal Minimum_Qty { get; set; }

        public decimal Replenish_Qty { get; set; }

        public decimal Pending_Replenish_Qty { get; set; }
    }

    public class SearchReplenishmentOnDemandBalanceItemModel
    {
        public Guid Product_Index { get; set; }

        public string Product_Id { get; set; }

        public string Product_Name { get; set; }

        public Guid Location_Index { get; set; }

        public string Location_Id { get; set; }

        public string Location_Name { get; set; }

        public decimal Minimum_Qty { get; set; }

        public decimal Replenish_Qty { get; set; }

        public decimal Pending_Replenish_Qty { get; set; }
    }
}
