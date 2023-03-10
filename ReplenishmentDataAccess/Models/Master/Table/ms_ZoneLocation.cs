using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models.Master.Table
{
    public partial class Ms_ZoneLocation
    {
        [Key]
        public Guid ZoneLocation_Index { get; set; }

        public Guid Zone_Index { get; set; }

        public Guid Location_Index { get; set; }

        public int? IsActive { get; set; }

        public int? IsDelete { get; set; }

        public int? IsSystem { get; set; }

        public int? Status_Id { get; set; }

        [StringLength(200)]
        public string ZoneLocation_Id { get; set; }

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
