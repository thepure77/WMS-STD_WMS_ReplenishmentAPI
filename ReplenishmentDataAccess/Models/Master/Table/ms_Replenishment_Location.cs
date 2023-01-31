using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.Table
{
    public partial class Ms_Replenishment_Location
    {
        [Key]
        public Guid Replenishment_Location_Index { get; set; }

        public Guid Replenishment_Index { get; set; }

        public Guid? Zone_Index { get; set; }

        public Guid? Location_Index { get; set; }
    }
}
