using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Business.Commons;

namespace Business.Models
{
    public class TraceTransferModel : Pagination
    {
        public long? rowIndex { get; set; }
        public bool export { get; set; }
        public string goodsTransfer_No { get; set; }
        public string tag_No { get; set; }
        public string product_Id { get; set; }
        public string product_Name { get; set; }
        public string product_Lot { get; set; }
        public decimal? qty { get; set; }
        public decimal? ratio { get; set; }
        public decimal? totalQty { get; set; }
        public string productConversion_Name { get; set; }
        public string location_Id { get; set; }
        public string location_Id_To { get; set; }
        public string goodsTransfer_Date { get; set; }
        public string transfer_Date { get; set; }
        public string transfer_Date_To { get; set; }
        public Guid? goodsReceiveItemLocation_Index { get; set; }
        public string erp_Location { get; set; }
        public string erp_Location_To { get; set; }
        public string gt_Status { get; set; }
        public Guid? processStatus_Index { get; set; }
        public string processStatus_Id { get; set; }
        public string processStatus_Name { get; set; }
        //public string gti_Status { get; set; }
        //public string t_Status { get; set; }
        //public string ti_Status { get; set; }
        public decimal? remaining { get; set; }
        public string unit_Remaining { get; set; }
        public decimal? total { get; set; }
        public string unit_Total { get; set; }
        public string create_By { get; set; }
        public string create_Date { get; set; }
        public string update_By { get; set; }
        public string update_Date { get; set; }
        public string documentType_Id { get; set; }

    }

    public class actionResultTrace_TransferViewModel
    {
        public IList<TraceTransferModel> itemsTrace { get; set; }
        public Pagination pagination { get; set; }
    }

    //public class Pagination
    //{
    //    public int CurrentPage { get; set; }

    //    public int PerPage { get; set; }

    //    public int TotalRow { get; set; }

    //    public string Key { get; set; }

    //    public bool AdvanceSearch { get; set; }

    //    public int NumPerPage { get; set; }

    //}
}
