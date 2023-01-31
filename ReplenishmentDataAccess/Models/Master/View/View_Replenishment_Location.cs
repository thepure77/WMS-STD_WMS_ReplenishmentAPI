using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Master.View
{
    public class View_Replenishment_Location
    {
        [Key]
        public Guid? Replenishment_Location_Index { get; set; }

        public Guid? Replenishment_Index { get; set; }

        public Guid? Zone_Index { get; set; }

        public string Zone_Id { get; set; }

        public string Zone_Name { get; set; }

        public Guid? Location_Index { get; set; }

        public string Location_Id { get; set; }

        public string Location_Name { get; set; }
    }
}
