using System;
using System.Collections.Generic;
using System.Text;

namespace ReplenishmentBusiness.Models
{
    public class RoundWaveViewModel
    {
        public Guid row_Index { get; set; }

        public string value { get; set; }

        public string display { get; set; }

        public Guid round_Index { get; set; }

        public string round_Id { get; set; }

        public string round_Name { get; set; }

        public string goodsIssue_Date { get; set; }
    }
}
