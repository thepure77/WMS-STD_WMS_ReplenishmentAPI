using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Business.Commons;

namespace Business.Models
{
    public class SearchReplenishmentViewModel : Pagination
    {
        public SearchReplenishmentViewModel()
        {
            sort = new List<SortViewModel>();

            Status = new List<StatusViewModel>();

        }

        [Key]
        public Guid? Replenishment_Index { get; set; }

        public string Replenishment_Id { get; set; }

        public DateTime? Trigger_Date { get; set; }

        public DateTime? Trigger_Date_End { get; set; }

        public bool? IsMonday { get; set; }

        public bool? IsTuesday { get; set; }

        public bool? IsWednesday { get; set; }

        public bool? IsThursday { get; set; }

        public bool? IsFriday { get; set; }

        public bool? IsSaturday { get; set; }

        public bool? IsSunday { get; set; }

        public int? Plan_By_Product { get; set; }

        public int? Plan_By_Location { get; set; }

        public int? Plan_By_Status { get; set; }

        public int? IsActive { get; set; }

        public string Create_By { get; set; }

        public DateTime? Create_Date { get; set; }

        public DateTime? Create_Date_End { get; set; }

        public List<SortViewModel> sort { get; set; }
        public List<StatusViewModel> Status { get; set; }

        public class SortViewModel
        {
            public string Value { get; set; }
            public string Display { get; set; }
            public int Seq { get; set; }
        }

        public class StatusViewModel
        {
            public int Value { get; set; }
            public string Display { get; set; }
            public int Seq { get; set; }
        }

        public class SortModel
        {
            public string ColId { get; set; }
            public string Sort { get; set; }

            public string PairAsSqlExpression
            {
                get
                {
                    return $"{ColId} {Sort}";
                }
            }
        }
    }
}
