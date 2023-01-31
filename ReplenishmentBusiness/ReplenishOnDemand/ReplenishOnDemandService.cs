using DataAccess;
using ReplenishmentBusiness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Comone.Utils;
using ReplenishmentBusiness.Commons;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Business.Services;
using DataAccess.Models.Master.Table;
using Business.Models.Binbalance;
using Newtonsoft.Json;
using DataAccess.Models.Master.View;
using BinBalanceDataAccess.Models;
using DataAccess.Models.Transfer.Table;
using Business.Models;
using Business.Commons;
using ReplenishmentBusiness.ModelConfig;
using System.Data;

namespace ReplenishmentBusiness.ReplenishOnDemand
{
    public class ReplenishOnDemandService
    {
        private MasterDbContext dbMaster;
        private OutboundDbContext dbOutbound;
        private BinbalanceDbContext dbBa;
        private TransferDbContext dbTf;

        public ReplenishOnDemandService()
        {
            dbMaster = new MasterDbContext();
            dbOutbound = new OutboundDbContext();
            dbBa = new BinbalanceDbContext();
            dbTf = new TransferDbContext();
        }

        #region + Master +
        private string GetDocumentNumber(Ms_DocumentType documentType, DateTime documentDate)
        {
            string formatDocument = documentType.Format_Document?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(formatDocument)) { throw new Exception("FormatDocument not found"); }

            string formatRunning = documentType.Format_Running?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(formatRunning)) { throw new Exception("FormatRunning not found"); }

            bool resetByYear = documentType.IsResetByYear == 1;
            bool resetByMonth = documentType.IsResetByMonth == 1;

            string docYear = documentDate.Year.ToString();
            string docMonth = !resetByYear ? documentDate.Month.ToString() : string.Empty;
            string docDay = !resetByYear && !resetByMonth ? documentDate.Day.ToString() : string.Empty;
            try
            {
                string formatDate = documentType.Format_Date ?? string.Empty;
                if (!string.IsNullOrEmpty(formatDate.Trim()))
                {
                    formatDate = documentDate.ToString(formatDate, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                }

                Ms_DocumentTypeNumber documentTypeNumber = dbTf.Ms_DocumentTypeNumber.FirstOrDefault(
                    w => w.IsActive == 1 && w.IsDelete == 0 &&
                         w.DocumentType_Index == documentType.DocumentType_Index &&
                         w.DocumentTypeNumber_Year == docYear &&
                         w.DocumentTypeNumber_Month == docMonth &&
                         w.DocumentTypeNumber_Day == docDay
                );

                int runningNumber = (documentTypeNumber?.DocumentTypeNumber_Running ?? 0) + 1;
                int runningLength = runningNumber.ToString().Length;
                int formatLength = formatRunning.Length;
                if (formatLength > runningLength)
                {
                    formatRunning = new string('0', formatLength - runningLength) + runningNumber.ToString();
                }
                else
                {
                    formatRunning = runningNumber.ToString().Substring(runningLength - formatLength, formatLength);
                }

                if (documentTypeNumber is null)
                {
                    //Create new Running
                    Ms_DocumentTypeNumber newDocumentTypeNumber = new Ms_DocumentTypeNumber()
                    {
                        DocumentTypeNumber_Index = Guid.NewGuid(),
                        DocumentType_Index = documentType.DocumentType_Index,
                        DocumentTypeNumber_Year = docYear,
                        DocumentTypeNumber_Month = docMonth,
                        DocumentTypeNumber_Day = docDay,
                        DocumentTypeNumber_Running = runningNumber,
                        IsActive = 1,
                        IsDelete = 0,
                        IsSystem = 1,
                        Status_Id = 0,
                        Create_By = "System",
                        Create_Date = DateTime.Now
                    };
                    dbTf.Ms_DocumentTypeNumber.Add(newDocumentTypeNumber);
                }
                else
                {
                    documentTypeNumber.DocumentTypeNumber_Running = runningNumber;
                    documentTypeNumber.Update_By = "System";
                    documentTypeNumber.Update_Date = DateTime.Now;
                }

                var myTransaction = dbMaster.Database.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    dbMaster.SaveChanges();
                    myTransaction.Commit();
                }
                catch (Exception saveEx)
                {
                    myTransaction.Rollback();
                    throw saveEx;
                }

                string formatText = documentType.Format_Text?.Trim() ?? string.Empty;
                string newDocumentNumber = formatDocument.ToUpper().Replace(" ", string.Empty)
                                                         .Replace("[FORMAT_TEXT]", formatText)
                                                         .Replace("[FORMAT_DATE]", formatDate)
                                                         .Replace("[FORMAT_RUNNING]", formatRunning);

                return newDocumentNumber;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region + Filter Replenish OnDemand +

        public List<ReplenishOnDemandViewModel> filterReplenishOnDemand(FilterReplenishOnDemandViewModel model)
        {
            try
            {
                var result = new List<ReplenishOnDemandViewModel>();

                var filterModel = new FilterReplenishOnDemandViewModel();

                var _due_date = model.goodsIssue_Date.toCVDateString();

                if (!string.IsNullOrEmpty(_due_date))
                {
                    var _round = new List<string>() { "01", "02", "03" , "04", "05", "06", "07", "08", "09", "10", "11", "12" };
                    var roundModels = new List<string>();
                    string _roundWave = "";
                    if (model.round_Wave.Count > 0)
                    {
                        foreach (var item in model.round_Wave)
                        {
                            if(!string.IsNullOrEmpty(_roundWave))
                            {
                                _roundWave += ",";
                            }

                            if (_round.Any(x => x.Contains(item.value)))
                            {
                                //roundModels.Add(item.value);
                                _roundWave += item.value;
                            }

                        }
                        //query = query.Where(c => roundModels.Contains(c.Document_Status));
                        //var lstReplenishmentOnDemand = dbMaster.View_ReplenishmentOnDemand.ToList();

                        var dueDate = new SqlParameter("@duedate", _due_date);
                        var roundWave = new SqlParameter("@roundWave", _roundWave);
                        var lstReplenishmentOnDemand = dbMaster.sp_ReplenishmentOnDemand.FromSql("sp_ReplenishmentOnDemand @duedate, @roundWave", dueDate, roundWave).ToList();

                        if (lstReplenishmentOnDemand.Count > 0)
                        {
                            foreach (var reple in lstReplenishmentOnDemand)
                            {
                                ReplenishOnDemandViewModel item = new ReplenishOnDemandViewModel();
                                item.rowIndex = reple.RowIndex.ToString();
                                item.product_Id = reple.Product_Id;
                                item.product_Name = reple.Product_Name;
                                item.su_Ratio = reple.SU_Ratio;
                                item.bu_Order_Qty = reple.BU_Order_Qty;
                                item.order_Qty = reple.Order_Qty;
                                item.order_Unit = reple.Order_Unit;
                                item.su_Order_Qty = reple.SU_Order_Qty;
                                item.su_Order_Unit = reple.SU_Order_Unit;
                                item.su_Weight = reple.SU_Weight;
                                item.su_GrsWeight = reple.SU_GrsWeight;
                                item.su_W = reple.SU_W;
                                item.su_L = reple.SU_L;
                                item.su_H = reple.SU_H;
                                item.maxPiecePick = reple.MaxPiecePick;
                                item.minPiecePick = reple.MinPiecePick;
                                item.isPiecePick = reple.IsPiecePick;
                                item.qtyInASRS = reple.QtyInASRS;
                                item.qtyInPiecePick = reple.QtyInPiecePick;
                                item.qtyInRepleLocation = reple.QtyInRepleLocation;
                                item.qtyInBal = reple.QtyInBal;
                                item.su_QtyInASRS = reple.SU_QtyInASRS;
                                item.su_QtyInPiecePick = reple.SU_QtyInPiecePick;
                                item.su_QtyInRepleLocation = reple.SU_QtyInRepleLocation;
                                item.su_QtyInBal = reple.SU_QtyInBal;
                                item.diff_QtyPiecePickWithOrder = reple.Diff_QtyPiecePickWithOrder;
                                item.diff_SU_QtyPiecePickWithOrder = reple.Diff_SU_QtyPiecePickWithOrder;
                                item.diff_RepleLocation = reple.Diff_RepleLocation;
                                item.configMaxReple = reple.ConfigMaxReple;
                                item.configMinReple = reple.ConfigMinReple;
                                item.pallet_Qty = "";

                                if ((reple.Diff_SU_QtyPiecePickWithOrder * -1) > reple.SU_QtyInRepleLocation)
                                {
                                    SearchReplenishmentBalanceModel modelCal = new SearchReplenishmentBalanceModel();
                                    SearchReplenishmentBalanceItemModel modelCalItem = new SearchReplenishmentBalanceItemModel();

                                    var modelProduct = dbMaster.Ms_Product.Where(w => w.Product_Id == reple.Product_Id).FirstOrDefault();

                                    if (modelProduct != null)
                                    {
                                        modelCalItem.Product_Index = modelProduct.Product_Index;
                                        modelCalItem.Replenish_Qty = (reple.Diff_QtyPiecePickWithOrder * -1) - reple.QtyInRepleLocation ?? 0;

                                        modelCal.Items.Add(modelCalItem);

                                        int calPallet = CalculatePalletBypass(modelCal);

                                        var resCal = calPallet;
                                        item.pallet_Qty = (resCal <= 0) ? "" : resCal.ToString();
                                    }
                                }

                                result.Add(item);
                            }

                        }
                    }  
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<RoundWaveViewModel> dropdownRoundWave(RoundWaveViewModel model)
        {
            try
            {
                var result = new List<RoundWaveViewModel>();

                var filterModel = new RoundWaveViewModel();

                if (!string.IsNullOrEmpty(model.goodsIssue_Date))
                {

                    var plandate = (DateTime)model.goodsIssue_Date.toCVDate();
                    var ShipTo_Id = new[] { "NON" };
                    var waveround_data = dbOutbound.im_PlanGoodsIssue.Where(w => !ShipTo_Id.Contains(w.ShipTo_Id) && w.Round_Id != null && w.PlanGoodsIssue_Due_Date.Value.Date == plandate.Date && w.Document_Status != -1)
                               .GroupBy(a => new { a.Round_Id, a.Round_Name, a.Document_Status, a.PlanGoodsIssue_Due_Date })
                               .Select(c => new
                               {
                                   Round_Id = c.Key.Round_Id,
                                   Round_Name = "Wave " + c.Key.Round_Id, //c.Key.Round_Name
                                   Document_Status = c.Key.Document_Status
                               }).OrderBy(o => o.Round_Id).ToList();
                    foreach (var i in waveround_data)
                    {
                        RoundWaveViewModel item = new RoundWaveViewModel();
                        item.row_Index = new Guid();
                        item.value = i.Round_Id;
                        item.display = i.Round_Name;
                        result.Add(item);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Im_GoodsTransfer getTranferOnDemand()
        {
            try
            {
                Im_GoodsTransfer result = new Im_GoodsTransfer();
                var _dataTransferOnDemand = dbTf.Im_GoodsTransfer.Where(c => c.DocumentType_Index == new Guid("D61AB6E6-FFB7-47B9-A2D3-CD4AF77E98C5") && (c.Document_Status == 0 || c.Document_Status == 2)).OrderBy(c => c.Create_Date).FirstOrDefault();
                if (_dataTransferOnDemand != null)
                {
                    result = _dataTransferOnDemand;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ResponseViewModel confirmTransferAndSendWCSBypass(GoodsTransferViewModel model)
        {
            string State = "Start";
            var olog = new logtxt();

            try
            {
                olog.logging("confirmTransferAndSendWCSBypass", State);

                ResponseViewModel result = new ResponseViewModel();
                var responseMessage = new ResponseMessage();

                bool _responseConfirmTransfer = false;

                if (new AppSettingConfig().GetUrl("confirmTransferDocument") != "")
                {
                    _responseConfirmTransfer = Utils.SendDataApi<bool>(new AppSettingConfig().GetUrl("confirmTransferDocument"), JsonConvert.SerializeObject(model));
                    responseMessage.description = "Confirm Transfer && Assign Job";
                    result.status = "10";
                    result.message = responseMessage;
                }
                olog.logging("confirmTransferAndSendWCSBypass", "responseConfirmTransfer : [ " + model.goodsTransfer_No + " ] [ " + _responseConfirmTransfer + " ]");

                if (_responseConfirmTransfer)
                {
                    olog.logging("confirmTransferAndSendWCSBypass", "Wait Send WCS Bypass");

                    responseMessage.description = "Wait Send WCS Bypass";
                    result.status = "20";
                    result.message = responseMessage;
                    // TO DO WCS

                    var transferModel = new
                    {
                        docNo = model.goodsTransfer_No,
                        updateBy = model.update_By
                    };

                    if(new AppSettingConfig().GetUrl("SendWCSBypass") != "")
                    {
                        var response_ASRS_SO_BYPASS_SORTER = Utils.SendDataApi<ResponseViewModel>(new AppSettingConfig().GetUrl("SendWCSBypass"), JsonConvert.SerializeObject(transferModel));
                        responseMessage.description = response_ASRS_SO_BYPASS_SORTER.message.description;
                        result.status = response_ASRS_SO_BYPASS_SORTER.status;
                        result.message = responseMessage;

                        olog.logging("confirmTransferAndSendWCSBypass", JsonConvert.SerializeObject(response_ASRS_SO_BYPASS_SORTER));
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region + Bypass ASRS +

        public List<string> ActivateBypassReplenishmentFromASRS(FilterReplenishOnDemandViewModel model)
        {

            String State = "Start";
            String msglog = "";
            var olog = new logtxt();

            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                olog.logging("ActivateBypassReplenishmentFromASRS", State);

                olog.logging("ActivateBypassReplenishmentFromASRS", "Request : " + JsonConvert.SerializeObject(model));

                //var lstBypassForReple = model.lstReplenishOnDemand.Select(s => s.product_Id).ToList();

                //var lstView_Replenishment = dbMaster.View_Replenishment.Where(w => lstBypassForReple.Contains(w.Product_Id)).ToList();

                //TO DO Config Index
                //Prepare BinBalance Model
                Guid GoodsReplenishDocumentTypeIndex = Guid.Parse("D61AB6E6-FFB7-47B9-A2D3-CD4AF77E98C5"); // Auto Replenishment ASRS
                Ms_DocumentType goodsReplenishDocumentType = dbMaster.Ms_DocumentType.Find(GoodsReplenishDocumentTypeIndex);
                if (goodsReplenishDocumentType is null)
                {
                    olog.logging("ActivateReplenishmentASRS", "Replenish DocumentType not found");
                    throw new Exception("Replenish DocumentType not found");
                }

                // Fix TOP 100
                Guid StorageLocationTypeIndex = Guid.Parse("02F5CBFC-769A-411B-9146-1D27F92AE82D");   // ASRS
                List<Guid> ReplenishLocationIndexs =
                    JsonConvert.DeserializeObject<List<Guid>>(
                    JsonConvert.SerializeObject(
                        dbMaster.Ms_Location.Where(s => s.IsActive == 1 && (s.Location_Id == "AV-001-02" || s.Location_Id == "AU-094-06" || s.Location_Id == "AT-018-02")
&& (s.BlockPut == null ? 0 : s.BlockPut) == 0 && s.LocationType_Index == StorageLocationTypeIndex).Select(s => s.Location_Index)));
                if ((ReplenishLocationIndexs?.Count ?? 0) == 0)
                {
                    olog.logging("ActivateReplenishmentASRS", "Replenish Location not found");
                    throw new Exception("Replenish Location not found");
                }

                List<Guid> ReplenishItemStatusIndexs = new List<Guid> { Guid.Parse("525BCFF1-2AD9-4ACB-819D-0DEA4E84EA12") };
                SearchReplenishmentBalanceModel binBalance_API_Model = new SearchReplenishmentBalanceModel()
                {
                    ReplenishLocationIndexs = ReplenishLocationIndexs,
                    ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                };

                List<string> errorMsg = new List<string>();
                List<ReplenishmentBalanceModel> binBalances;
                //foreach (Ms_Replenishment replenishment in Replenishments)
                //{
                //    try
                //    {

                //        binBalances = new List<ReplenishmentBalanceModel>();
                //        binBalance_API_Model = new SearchReplenishmentBalanceModel()
                //        {
                //            ReplenishLocationIndexs = ReplenishLocationIndexs,
                //            ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                //        };

                //        olog.logging("ActivateReplenishmentASRS", " GetBinBalanceReplenish Replenishment_Id :  " + replenishment.Replenishment_Id);


                //        binBalances = GetBinBalanceReplenishASRS(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
                //        if ((binBalances?.Count ?? 0) > 0)
                //        {
                //            olog.logging("ActivateReplenishmentASRS", " CreateReplenishDocument Replenishment_Id :  " + replenishment.Replenishment_Id);


                //            GoodsReplenishDocuments.AddRange(
                //                CreateReplenishDocument(replenishment.Replenishment_Index, goodsReplenishDocumentType, binBalances)
                //            );
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        olog.logging("ActivateReplenishmentASRS", "GetBinBalanceReplenish " + ex.Message);

                //        errorMsg.Add(ex.Message);
                //        continue;
                //    }
                //}

                //AddHeader
                Guid GoodsReplenishIndex;
                string GoodsReplenishNo;
                Im_GoodsTransfer GoodsReplenish;
                DateTime ActiveDate = DateTime.Now;
                string ActiveBy = "System";

                GoodsReplenishIndex = Guid.NewGuid();
                GoodsReplenishNo = GetDocumentNumber(goodsReplenishDocumentType, ActiveDate);
                GoodsReplenish = new Im_GoodsTransfer()
                {
                    //Replenishment_Index = ReplenishIndex,

                    GoodsTransfer_Index = GoodsReplenishIndex,
                    GoodsTransfer_No = GoodsReplenishNo,
                    GoodsTransfer_Date = ActiveDate,
                    GoodsTransfer_Time = ActiveDate.ToShortTimeString(),
                    GoodsTransfer_Doc_Date = ActiveDate,
                    GoodsTransfer_Doc_Time = ActiveDate.ToShortTimeString(),
                    Owner_Index = new Guid("02B31868-9D3D-448E-B023-05C121A424F4"),
                    Owner_Id = "3419",
                    Owner_Name = "Amazon",
                    DocumentType_Index = goodsReplenishDocumentType.DocumentType_Index,
                    DocumentType_Id = goodsReplenishDocumentType.DocumentType_Id,
                    DocumentType_Name = goodsReplenishDocumentType.DocumentType_Name,

                    Document_Status = 0, // 1

                    Create_By = ActiveBy,
                    Create_Date = ActiveDate
                };
                dbTf.Im_GoodsTransfer.Add(GoodsReplenish);

                foreach (ReplenishOnDemandViewModel replenishment in model.lstReplenishOnDemand)
                {
                    try
                    {

                        binBalances = new List<ReplenishmentBalanceModel>();
                        binBalance_API_Model = new SearchReplenishmentBalanceModel()
                        {
                            ReplenishLocationIndexs = ReplenishLocationIndexs,
                            ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                        };

                        olog.logging("ActivateReplenishmentASRS", " GetBinBalanceReplenish Product_Id :  " + replenishment.product_Id);


                        binBalances = GetBinBalanceReplenishASRS(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
                        if ((binBalances?.Count ?? 0) > 0)
                        {
                            olog.logging("ActivateReplenishmentASRS", " CreateReplenishDocument Product_Id :  " + replenishment.product_Id);


                            //GoodsReplenishDocuments.AddRange(
                            //    CreateReplenishDocument(goodsReplenishDocumentType, binBalances)
                            //);
                            CreateReplenishDocument(GoodsReplenishIndex, GoodsReplenishNo, goodsReplenishDocumentType, binBalances);
                        }
                    }
                    catch (Exception ex)
                    {
                        olog.logging("ActivateReplenishmentASRS", "GetBinBalanceReplenish " + ex.Message);

                        errorMsg.Add(ex.Message);
                        continue;
                    }
                }

                if (errorMsg.Count > 0)
                {
                    olog.logging("ActivateReplenishmentASRS", "Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                    throw new Exception("Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                }

                GoodsReplenishDocuments.Add(GoodsReplenishNo);
                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                olog.logging("ActivateReplenishmentASRS", ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
                throw new Exception(ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
            }
        }

        private List<ReplenishmentBalanceModel> GetBinBalanceReplenishASRS(Ms_DocumentType documentType, ReplenishOnDemandViewModel replenishment, SearchReplenishmentBalanceModel binBalance_API_Model)
        {
            try
            {
                IQueryable<View_ProductLocation> queryProductLocations = dbMaster.View_ProductLocation.AsQueryable().Where(w => w.IsActive == 1 && w.IsDelete == 0);

                //bool planByProduct = replenishment.Plan_By_Product == 1;
                //bool planByLocation = replenishment.Plan_By_Location == 1;

                //if (planByProduct)
                //{
                //    List<Ms_Replenishment_Product> replenishProducts = dbMaster.Ms_Replenishment_Product.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                //    if ((replenishProducts?.Count ?? 0) > 0)
                //    {
                //        List<Guid?> listProductIndex = replenishProducts.Where(w => w.Product_Index.HasValue).Select(s => s.Product_Index).Distinct().ToList();
                //        List<Guid?> listProductTypeIndex = replenishProducts.Where(w => !w.Product_Index.HasValue).Select(s => (Guid?)s.ProductType_Index).Distinct().ToList();

                //        List<Ms_Product> products = new List<Ms_Product>();
                //        if ((listProductTypeIndex?.Count ?? 0) > 0)
                //        {
                //            products = dbMaster.Ms_Product.Where(
                //                    w => w.IsActive == 1 && w.IsDelete == 0 && listProductTypeIndex.Contains(w.ProductCategory_Index)
                //            ).ToList();

                //            if ((products?.Count ?? 0) > 0)
                //            {
                //                (listProductIndex ?? new List<Guid?>()).AddRange(products.Select(s => (Guid?)s.Product_Index).ToList());
                //            }
                //        }

                //        if ((listProductIndex?.Count ?? 0) > 0)
                //        {
                //            listProductIndex = listProductIndex.Distinct().ToList();
                //            queryProductLocations = queryProductLocations.Where(w => listProductIndex.Contains(w.Product_Index));
                //        }
                //    }
                //}

                //if (planByLocation)
                //{
                //    List<Ms_Replenishment_Location> replenishLocations = dbMaster.Ms_Replenishment_Location.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                //    if ((replenishLocations?.Count ?? 0) > 0)
                //    {
                //        List<Guid?> listLocationIndex = replenishLocations.Where(w => w.Location_Index.HasValue).Select(s => s.Location_Index).Distinct().ToList();
                //        List<Guid?> listZoneIndex = replenishLocations.Where(w => w.Zone_Index.HasValue).Select(s => s.Zone_Index).Distinct().ToList();

                //        List<Ms_ZoneLocation> zoneLocations = new List<Ms_ZoneLocation>();
                //        if ((listZoneIndex?.Count ?? 0) > 0)
                //        {
                //            zoneLocations = dbMaster.Ms_ZoneLocation.Where(
                //                w => w.IsActive == 1 && w.IsDelete == 0 && listZoneIndex.Contains(w.Zone_Index)
                //            ).ToList();

                //            if ((zoneLocations?.Count ?? 0) > 0)
                //            {
                //                (listLocationIndex ?? new List<Guid?>()).AddRange(zoneLocations.Select(s => (Guid?)s.Location_Index).ToList());
                //            }
                //        }

                //        if ((listLocationIndex?.Count ?? 0) > 0)
                //        {
                //            listLocationIndex = listLocationIndex.Distinct().ToList();
                //            queryProductLocations = queryProductLocations.Where(w => listLocationIndex.Contains(w.Location_Index));
                //        }
                //    }
                //}

                List<View_ProductLocation> productLocations = queryProductLocations.Where(w => w.Product_Id == replenishment.product_Id).ToList();
                if ((productLocations?.Count ?? 0) == 0) { throw new Exception("ProductLocation not found"); }

                Boolean checkLocation = false;
                //var LocVC = productLocations.Where(c => c.Location_Name.Contains("VC")).ToList();
                //if (LocVC.Count > 0)
                //{
                //    checkLocation = true;
                //}
                //var LocPA = productLocations.Where(c => c.Location_Name.Contains("PA")).ToList();
                //if (LocPA.Count > 0)
                //{
                //    checkLocation = true;
                //}
                //var LocPB = productLocations.Where(c => c.Location_Name.Contains("PB")).ToList();
                //if (LocPB.Count > 0)
                //{
                //    checkLocation = true;
                //}



                //productLocations.ForEach(e => binBalance_API_Model.Items.Add(
                //    new SearchReplenishmentBalanceItemModel()
                //    {
                //        Product_Index = e.Product_Index,
                //        Location_Index = e.Location_Index,
                //        Location_Id = e.Location_Id,
                //        Location_Name = e.Location_Name,
                //        Minimum_Qty = 0, // e.Qty
                //        Replenish_Qty = 0, // Reple
                //        Pending_Replenish_Qty = 0 //GetPendingReplenishQty(binBalance_API_Model.Owner_Index, documentType.DocumentType_Index, e.Product_Index, e.Location_Index),

                //    }
                //));

                var modelProduct = dbMaster.Ms_Product.Where(c => c.Product_Id == replenishment.product_Id).FirstOrDefault();

                if (modelProduct == null)
                {
                    throw new InvalidOperationException("Product Invalid");
                }

                binBalance_API_Model.Items.Add(
                    new SearchReplenishmentBalanceItemModel()
                    {
                        Product_Index = modelProduct.Product_Index,
                        Location_Index = new Guid("7738EA53-37F6-4AC9-8E87-9F3FEF779CCD"),
                        Location_Id = "REPLE-DUMMY",
                        Location_Name = "REPLE-DUMMY",
                        Minimum_Qty = 0, // e.Qty
                        Replenish_Qty = (replenishment.diff_QtyPiecePickWithOrder * -1) - replenishment.qtyInRepleLocation ?? 0, // Reple
                        Pending_Replenish_Qty = 0 //GetPendingReplenishQty(binBalance_API_Model.Owner_Index, documentType.DocumentType_Index, e.Product_Index, e.Location_Index),

                    }
                );

                //binBalance_API_Model.Items.RemoveAll(r => r.Pending_Replenish_Qty >= r.Minimum_Qty);
                binBalance_API_Model.Items.RemoveAll(r => r.Pending_Replenish_Qty >= r.Replenish_Qty);

                if (binBalance_API_Model.Items.Count == 0) { throw new Exception("Already Pending Replenish"); }

                List<ReplenishmentBalanceModel> BinBalances = new List<ReplenishmentBalanceModel>();
                //Send API BinBalance
                if (checkLocation == true)
                {
                    // VC01
                    //  BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalanceVC"), JsonConvert.SerializeObject(binBalance_API_Model));
                }
                else
                {
                    // ASRS
                    //BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));
                    BinBalances = SearchReplenishmentBinBalanceASRS(JsonConvert.SerializeObject(binBalance_API_Model));
                }

                //  List<ReplenishmentBalanceModel> BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));

                return BinBalances;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> CreateReplenishDocument(Guid _GoodsReplenishIndex, string _GoodsReplenishNo, Ms_DocumentType GoodsReplenishDocumentType, List<ReplenishmentBalanceModel> BinBalances)
        {
            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                dbBa.Database.SetCommandTimeout(120);
                var olog = new logtxt();

                List<ReplenishmentBalanceModel> OwnerBinBalances;

                Guid GoodsReplenishIndex, GoodsReplenishItemIndex;
                string GoodsReplenishNo;

                GoodsReplenishIndex = _GoodsReplenishIndex;
                GoodsReplenishNo = _GoodsReplenishNo;

                Im_GoodsTransfer GoodsReplenish;
                Im_GoodsTransferItem GoodsReplenishItem;
                DateTime ActiveDate = DateTime.Now;
                string ActiveBy = "System";

                var modelassjob = new View_AssignJobLocViewModel();

                List<ReserveBinBalanceItemModel> reserveModel = new List<ReserveBinBalanceItemModel>();
                List<Guid> OwnerIndexs = BinBalances.Select(s => s.Owner_Index).Distinct().ToList();
                foreach (Guid OwnerIndex in OwnerIndexs)
                {
                    reserveModel.Clear();
                    OwnerBinBalances = BinBalances.Where(s => s.Owner_Index == OwnerIndex).ToList();

                    ////Header
                    //GoodsReplenishIndex = Guid.NewGuid();
                    //GoodsReplenishNo = GetDocumentNumber(GoodsReplenishDocumentType, ActiveDate);
                    //GoodsReplenish = new Im_GoodsTransfer()
                    //{
                    //    //Replenishment_Index = ReplenishIndex,

                    //    GoodsTransfer_Index = GoodsReplenishIndex,
                    //    GoodsTransfer_No = GoodsReplenishNo,
                    //    GoodsTransfer_Date = ActiveDate,
                    //    GoodsTransfer_Time = ActiveDate.ToShortTimeString(),
                    //    GoodsTransfer_Doc_Date = ActiveDate,
                    //    GoodsTransfer_Doc_Time = ActiveDate.ToShortTimeString(),
                    //    Owner_Index = OwnerBinBalances[0].BinBalance.Owner_Index,
                    //    Owner_Id = OwnerBinBalances[0].BinBalance.Owner_Id,
                    //    Owner_Name = OwnerBinBalances[0].BinBalance.Owner_Name,
                    //    DocumentType_Index = GoodsReplenishDocumentType.DocumentType_Index,
                    //    DocumentType_Id = GoodsReplenishDocumentType.DocumentType_Id,
                    //    DocumentType_Name = GoodsReplenishDocumentType.DocumentType_Name,

                    //    Document_Status = 0, // 1

                    //    Create_By = ActiveBy,
                    //    Create_Date = ActiveDate
                    //};
                    //dbTf.Im_GoodsTransfer.Add(GoodsReplenish);


                    modelassjob.goodsTransfer_Index = GoodsReplenishIndex;
                    modelassjob.Create_By = ActiveBy;
                    modelassjob.Template = "1";

                    //Items
                    foreach (ReplenishmentBalanceModel Item in OwnerBinBalances)
                    {
                        if ((Item.BinBalance.BinBalance_QtyBal ?? 0) <= 0)
                        {
                            continue;
                        }

                        GoodsReplenishItemIndex = Guid.NewGuid();

                        GoodsReplenishItem = new Im_GoodsTransferItem();

                        GoodsReplenishItem.GoodsTransferItem_Index = GoodsReplenishItemIndex;
                        GoodsReplenishItem.GoodsTransfer_Index = GoodsReplenishIndex;
                        GoodsReplenishItem.GoodsReceiveItem_Index = Item.BinBalance.GoodsReceiveItem_Index;
                        GoodsReplenishItem.GoodsReceive_Index = Item.BinBalance.GoodsReceive_Index;
                        GoodsReplenishItem.GoodsReceiveItemLocation_Index = Item.BinBalance.GoodsReceiveItemLocation_Index;
                        GoodsReplenishItem.LineNum = (OwnerBinBalances.IndexOf(Item) + 1).ToString();

                        GoodsReplenishItem.TagItem_Index = Item.BinBalance.TagItem_Index;
                        GoodsReplenishItem.Tag_Index = Item.BinBalance.Tag_Index;
                        GoodsReplenishItem.Tag_No = Item.BinBalance.Tag_No;
                        GoodsReplenishItem.Owner_Index = Item.BinBalance.Owner_Index;
                        GoodsReplenishItem.Owner_Id = Item.BinBalance.Owner_Id;
                        GoodsReplenishItem.Owner_Name = Item.BinBalance.Owner_Name;
                        GoodsReplenishItem.GoodsReceive_MFG_Date = Item.BinBalance.GoodsReceive_MFG_Date; //addnew
                        GoodsReplenishItem.GoodsReceive_MFG_Date_To = Item.BinBalance.GoodsReceive_MFG_Date; //addnew
                        GoodsReplenishItem.GoodsReceive_EXP_Date = Item.BinBalance.GoodsReceive_EXP_Date;
                        GoodsReplenishItem.GoodsReceive_EXP_Date_To = Item.BinBalance.GoodsReceive_EXP_Date; //addnew
                        GoodsReplenishItem.Product_Index = Item.BinBalance.Product_Index;
                        GoodsReplenishItem.Product_Id = Item.BinBalance.Product_Id;
                        GoodsReplenishItem.Product_Name = Item.BinBalance.Product_Name;
                        GoodsReplenishItem.Product_SecondName = Item.BinBalance.Product_SecondName;
                        GoodsReplenishItem.Product_ThirdName = Item.BinBalance.Product_ThirdName;
                        GoodsReplenishItem.Product_Lot = Item.BinBalance.Product_Lot;
                        GoodsReplenishItem.Product_Lot_To = Item.BinBalance.Product_Lot; //addnew
                        GoodsReplenishItem.ItemStatus_Index = Item.BinBalance.ItemStatus_Index;
                        GoodsReplenishItem.ItemStatus_Id = Item.BinBalance.ItemStatus_Id;
                        GoodsReplenishItem.ItemStatus_Name = Item.BinBalance.ItemStatus_Name;
                        GoodsReplenishItem.ItemStatus_Index_To = Item.BinBalance.ItemStatus_Index;
                        GoodsReplenishItem.ItemStatus_Id_To = Item.BinBalance.ItemStatus_Id;
                        GoodsReplenishItem.ItemStatus_Name_To = Item.BinBalance.ItemStatus_Name;
                        GoodsReplenishItem.Location_Index = Item.BinBalance.Location_Index;
                        GoodsReplenishItem.Location_Id = Item.BinBalance.Location_Id;
                        GoodsReplenishItem.Location_Name = Item.BinBalance.Location_Name;
                        GoodsReplenishItem.Location_Index_To = Item.Location_Index;
                        GoodsReplenishItem.Location_Id_To = Item.Location_Id;
                        GoodsReplenishItem.Location_Name_To = Item.Location_Name;

                        decimal? SaleUnitRatio = 1;
                        var modelSaleUnit = dbMaster.Ms_ProductConversion.Where(c => c.Product_Index == Item.BinBalance.Product_Index && c.SALE_UNIT == 1).FirstOrDefault();
                        if (modelSaleUnit != null)
                        {
                            SaleUnitRatio = modelSaleUnit.ProductConversion_Ratio;
                        }

                        GoodsReplenishItem.Qty = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value / (SaleUnitRatio ?? 1), 6);
                        GoodsReplenishItem.Ratio = SaleUnitRatio;

                        //GoodsReplenishItem.Qty = decimal.Round(Item.Replenish_Qty / (Item.BinBalance.BinBalance_Ratio ?? 1), 6);
                        //GoodsReplenishItem.Ratio = Item.BinBalance.BinBalance_Ratio;
                        GoodsReplenishItem.TotalQty = Item.BinBalance.BinBalance_QtyBal.Value;
                        GoodsReplenishItem.ProductConversion_Index = (modelSaleUnit != null) ? modelSaleUnit.ProductConversion_Index : Item.BinBalance.ProductConversion_Index;
                        GoodsReplenishItem.ProductConversion_Id = (modelSaleUnit != null) ? modelSaleUnit.ProductConversion_Id : Item.BinBalance.ProductConversion_Id;
                        GoodsReplenishItem.ProductConversion_Name = (modelSaleUnit != null) ? modelSaleUnit.ProductConversion_Name : Item.BinBalance.ProductConversion_Name;

                        GoodsReplenishItem.UnitVolume = Item.BinBalance.BinBalance_UnitVolumeBal;
                        GoodsReplenishItem.Volume = decimal.Round((Item.BinBalance.BinBalance_QtyBal.Value / Item.BinBalance.BinBalance_QtyBal ?? 1) * (Item.BinBalance.BinBalance_VolumeBal ?? 0), 6);

                        GoodsReplenishItem.UnitGrsWeight = Item.BinBalance.BinBalance_UnitGrsWeightBal;
                        GoodsReplenishItem.UnitGrsWeight_Index = Item.BinBalance.BinBalance_UnitGrsWeightBal_Index;
                        GoodsReplenishItem.UnitGrsWeight_Id = Item.BinBalance.BinBalance_UnitGrsWeightBal_Id;
                        GoodsReplenishItem.UnitGrsWeight_Name = Item.BinBalance.BinBalance_UnitGrsWeightBal_Name;
                        GoodsReplenishItem.UnitGrsWeightRatio = Item.BinBalance.BinBalance_UnitGrsWeightBalRatio;

                        GoodsReplenishItem.GrsWeight = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6);
                        GoodsReplenishItem.GrsWeight_Index = Item.BinBalance.BinBalance_GrsWeightBal_Index;
                        GoodsReplenishItem.GrsWeight_Id = Item.BinBalance.BinBalance_GrsWeightBal_Id;
                        GoodsReplenishItem.GrsWeight_Name = Item.BinBalance.BinBalance_GrsWeightBal_Name;
                        GoodsReplenishItem.GrsWeightRatio = Item.BinBalance.BinBalance_GrsWeightBalRatio;

                        GoodsReplenishItem.UnitNetWeight = Item.BinBalance.BinBalance_UnitNetWeightBal;
                        GoodsReplenishItem.UnitNetWeight_Index = Item.BinBalance.BinBalance_UnitNetWeightBal_Index;
                        GoodsReplenishItem.UnitNetWeight_Id = Item.BinBalance.BinBalance_UnitNetWeightBal_Id;
                        GoodsReplenishItem.UnitNetWeight_Name = Item.BinBalance.BinBalance_UnitNetWeightBal_Name;
                        GoodsReplenishItem.UnitNetWeightRatio = Item.BinBalance.BinBalance_UnitNetWeightBalRatio;

                        GoodsReplenishItem.NetWeight = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.BinBalance_UnitNetWeightBal ?? 0), 6);
                        GoodsReplenishItem.NetWeight_Index = Item.BinBalance.BinBalance_NetWeightBal_Index;
                        GoodsReplenishItem.NetWeight_Id = Item.BinBalance.BinBalance_NetWeightBal_Id;
                        GoodsReplenishItem.NetWeight_Name = Item.BinBalance.BinBalance_NetWeightBal_Name;
                        GoodsReplenishItem.NetWeightRatio = Item.BinBalance.BinBalance_NetWeightBalRatio;

                        GoodsReplenishItem.UnitWeight = Item.BinBalance.BinBalance_UnitWeightBal;
                        GoodsReplenishItem.UnitWeight_Index = Item.BinBalance.BinBalance_UnitWeightBal_Index;
                        GoodsReplenishItem.UnitWeight_Id = Item.BinBalance.BinBalance_UnitWeightBal_Id;
                        GoodsReplenishItem.UnitWeight_Name = Item.BinBalance.BinBalance_UnitWeightBal_Name;
                        GoodsReplenishItem.UnitWeightRatio = Item.BinBalance.BinBalance_UnitWeightBalRatio;

                        //GoodsReplenishItem.Weight = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6);
                        GoodsReplenishItem.Weight_Index = Item.BinBalance.BinBalance_WeightBal_Index;
                        GoodsReplenishItem.Weight_Id = Item.BinBalance.BinBalance_WeightBal_Id;
                        GoodsReplenishItem.Weight_Name = Item.BinBalance.BinBalance_WeightBal_Name;
                        GoodsReplenishItem.WeightRatio = Item.BinBalance.BinBalance_WeightBalRatio;

                        GoodsReplenishItem.UnitWidth = Item.BinBalance.BinBalance_UnitWidthBal;
                        GoodsReplenishItem.UnitWidth_Index = Item.BinBalance.BinBalance_UnitWidthBal_Index;
                        GoodsReplenishItem.UnitWidth_Id = Item.BinBalance.BinBalance_UnitWidthBal_Id;
                        GoodsReplenishItem.UnitWidth_Name = Item.BinBalance.BinBalance_UnitWidthBal_Name;
                        GoodsReplenishItem.UnitWidthRatio = Item.BinBalance.BinBalance_UnitWidthBalRatio;

                        GoodsReplenishItem.Width = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.BinBalance_UnitWidthBal ?? 0), 6);
                        GoodsReplenishItem.Width_Index = Item.BinBalance.BinBalance_WidthBal_Index;
                        GoodsReplenishItem.Width_Id = Item.BinBalance.BinBalance_WidthBal_Id;
                        GoodsReplenishItem.Width_Name = Item.BinBalance.BinBalance_WidthBal_Name;
                        GoodsReplenishItem.WidthRatio = Item.BinBalance.BinBalance_WidthBalRatio;

                        GoodsReplenishItem.UnitLength = Item.BinBalance.BinBalance_UnitLengthBal;
                        GoodsReplenishItem.UnitLength_Index = Item.BinBalance.BinBalance_UnitLengthBal_Index;
                        GoodsReplenishItem.UnitLength_Id = Item.BinBalance.BinBalance_UnitLengthBal_Id;
                        GoodsReplenishItem.UnitLength_Name = Item.BinBalance.BinBalance_UnitLengthBal_Name;
                        GoodsReplenishItem.UnitLengthRatio = Item.BinBalance.BinBalance_UnitLengthBalRatio;

                        GoodsReplenishItem.Length = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.BinBalance_UnitLengthBal ?? 0), 6);
                        GoodsReplenishItem.Length_Index = Item.BinBalance.BinBalance_LengthBal_Index;
                        GoodsReplenishItem.Length_Id = Item.BinBalance.BinBalance_LengthBal_Id;
                        GoodsReplenishItem.Length_Name = Item.BinBalance.BinBalance_LengthBal_Name;
                        GoodsReplenishItem.LengthRatio = Item.BinBalance.BinBalance_LengthBalRatio;

                        GoodsReplenishItem.UnitHeight = Item.BinBalance.BinBalance_UnitHeightBal;
                        GoodsReplenishItem.UnitHeight_Index = Item.BinBalance.BinBalance_UnitHeightBal_Index;
                        GoodsReplenishItem.UnitHeight_Id = Item.BinBalance.BinBalance_UnitHeightBal_Id;
                        GoodsReplenishItem.UnitHeight_Name = Item.BinBalance.BinBalance_UnitHeightBal_Name;
                        GoodsReplenishItem.UnitHeightRatio = Item.BinBalance.BinBalance_UnitHeightBalRatio;

                        GoodsReplenishItem.Height = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.BinBalance_UnitHeightBal ?? 0), 6);
                        GoodsReplenishItem.Height_Index = Item.BinBalance.BinBalance_HeightBal_Index;
                        GoodsReplenishItem.Height_Id = Item.BinBalance.BinBalance_HeightBal_Id;
                        GoodsReplenishItem.Height_Name = Item.BinBalance.BinBalance_HeightBal_Name;
                        GoodsReplenishItem.HeightRatio = Item.BinBalance.BinBalance_HeightBalRatio;

                        GoodsReplenishItem.UnitPrice = Item.BinBalance.UnitPrice;
                        GoodsReplenishItem.UnitPrice_Index = Item.BinBalance.UnitPrice_Index;
                        GoodsReplenishItem.UnitPrice_Id = Item.BinBalance.UnitPrice_Id;
                        GoodsReplenishItem.UnitPrice_Name = Item.BinBalance.UnitPrice_Name;

                        GoodsReplenishItem.Price = decimal.Round(Item.BinBalance.BinBalance_QtyBal.Value * (Item.BinBalance.UnitPrice ?? 0), 6);
                        GoodsReplenishItem.Price_Index = Item.BinBalance.Price_Index;
                        GoodsReplenishItem.Price_Id = Item.BinBalance.Price_Id;
                        GoodsReplenishItem.Price_Name = Item.BinBalance.Price_Name;

                        GoodsReplenishItem.DocumentRef_No5 = Item.BinBalance.BinBalance_QtyBal?.ToString();

                        GoodsReplenishItem.Document_Status = 0;
                        GoodsReplenishItem.Create_By = ActiveBy;
                        GoodsReplenishItem.Create_Date = ActiveDate;

                        GoodsReplenishItem.ERP_Location = Item.BinBalance.ERP_Location;
                        GoodsReplenishItem.ERP_Location_To = Item.BinBalance.ERP_Location;
                        //GoodsReplenishItem.UDF_1 = Item.BinBalance.GoodsReceive_No;

                        if (GoodsReplenishDocumentType.DocumentType_Index.ToString().ToUpper() != "9056FF09-29DF-4BBA-8FC5-6C524387F993")
                        {
                            ////Comment ไว้ก่อนเดี๋ยวเปิด
                            ////getGRI
                            //var listGRItem = new List<DocumentViewModel> { new DocumentViewModel { documentItem_Index = Item.BinBalance.GoodsReceiveItem_Index } };
                            //var GRItem = new DocumentViewModel();
                            //GRItem.listDocumentViewModel = listGRItem;
                            //var GoodsReceiveItem = Utils.SendDataApi<List<GoodsReceiveItemV2ViewModel>>(new AppSettingConfig().GetUrl("FindGoodsReceiveItem"), JsonConvert.SerializeObject(GRItem));
                            //GoodsReplenishItem.UDF_1 = Item.BinBalance.GoodsReceive_No;
                            //GoodsReplenishItem.UDF_2 = GoodsReceiveItem?.FirstOrDefault().ref_Document_No;

                            ////getPGRI
                            //var listPGRItem = new List<DocumentViewModel> { new DocumentViewModel { documentItem_Index = GoodsReceiveItem?.FirstOrDefault().ref_DocumentItem_Index } };
                            //var PGRItem = new DocumentViewModel();
                            //PGRItem.listDocumentViewModel = listPGRItem;
                            //var PlanGoodsReceiveItem = Utils.SendDataApi<List<PlanGoodsReceiveItemViewModel>>(new AppSettingConfig().GetUrl("FindPlanGoodsReceiveItem"), JsonConvert.SerializeObject(PGRItem));
                            //GoodsReplenishItem.UDF_3 = PlanGoodsReceiveItem?.FirstOrDefault().documentRef_No2;
                        }

                        var datacheckTag = dbBa.wm_BinBalance.Where(c => c.Location_Id == GoodsReplenishItem.Location_Id_To
                                           && c.BinBalance_QtyBal > 0
                                           && c.BinBalance_QtyReserve >= 0
                                           && c.Product_Index == GoodsReplenishItem.Product_Index).FirstOrDefault();

                        if (datacheckTag != null)
                        {
                            //GoodsReplenishItem.Tag_No_To = datacheckTag.Tag_No;
                        }

                        dbTf.Im_GoodsTransferItem.Add(GoodsReplenishItem);

                        reserveModel.Add(
                            new ReserveBinBalanceItemModel()
                            {
                                BinBalance_Index = Item.BinBalance.BinBalance_Index,
                                Ref_Document_Index = GoodsReplenishIndex,
                                Ref_DocumentItem_Index = GoodsReplenishItemIndex,
                                Process_Index = GoodsReplenishDocumentType.Process_Index.Value,
                                Ref_Document_No = GoodsReplenishNo,
                                Ref_Wave_Index = string.Empty,
                                Reserve_Qty = Item.BinBalance.BinBalance_QtyBal ?? 0,//Item.Replenish_Qty,
                                Reserve_By = ActiveBy,
                                IsReturnBinBalanceModel = false,
                                IsReturninCardReserveModel = true
                            }
                        );
                    }


                    var transaction = dbTf.Database.BeginTransaction(IsolationLevel.Serializable);
                    try
                    {
                        dbTf.SaveChanges();
                        //Send API to Reserve
                        ReserveBinBalanceResultModel result = Utils.SendDataApi<ReserveBinBalanceResultModel>(new AppSettingConfig().GetUrl("ReserveBinBalance"), JsonConvert.SerializeObject(new ReserveBinBalanceModel() { Items = reserveModel }));
                        if ((result?.ResultIsUse ?? false) == false)
                        {
                            throw new Exception("ReserveBinBalance Exception : " + result.ResultMsg);
                        }

                        transaction.Commit();

                        //var resultAssignjob = Utils.SendDataApi<string>(new AppSettingConfig().GetUrl("AssignJobTransfer"), JsonConvert.SerializeObject(modelassjob));

                        //olog.logging("CreateReplenishDocument", "SendWCSPutAwayVC GoodsReplenishNo : " + GoodsReplenishNo);

                        //var modelTransferReple = new { docNo = GoodsReplenishNo };

                        //var resultSendWCSPutAwayVC = Utils.SendDataApi<dynamic>(new AppSettingConfig().GetUrl("SendWCSPutAwayVC"), JsonConvert.SerializeObject(modelTransferReple));

                        GoodsReplenishDocuments.Add(GoodsReplenishNo);
                    }
                    catch (Exception exSave)
                    {
                        //TO DO Logging ?
                        olog.logging("CreateReplenishDocument", "Error : [ " + GoodsReplenishNo + " ] : " + exSave.ToString());
                        transaction.Rollback();
                        throw exSave;
                    }
                }

                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion

        #region + Get Data from Binbalance +

        private SearchReplenishmentBalanceModel GetSearchReplenishmentBalanceModel(string jsonData)
        {
            SearchReplenishmentBalanceModel model = JsonConvert.DeserializeObject<SearchReplenishmentBalanceModel>(jsonData ?? string.Empty);
            if (model is null)
            {
                throw new Exception("Invalid JSon : Can not Convert to Model");
            }

            if (model.Items is null || model.Items.Count == 0)
            {
                throw new Exception("Invalid JSon : Replenish Item not found");
            }

            if (model.ReplenishLocationIndexs is null || model.ReplenishLocationIndexs.Count == 0)
            {
                throw new Exception("Invalid JSon : Replenish Location not found");
            }

            if (model.ReplenishItemStatusIndexs is null || model.ReplenishItemStatusIndexs.Count == 0)
            {
                throw new Exception("Invalid JSon : Replenish ItemStatus not found");
            }

            return model;
        }

        private List<ReplenishmentBalanceModel> SearchReplenishmentBinBalanceASRS(string jsonData)
        {
            var olog = new logtxt();

            try
            {
                SearchReplenishmentBalanceModel data = GetSearchReplenishmentBalanceModel(jsonData);
                List<ReplenishmentBalanceModel> ReplenishmentBinBalance = new List<ReplenishmentBalanceModel>();

                List<Guid> lstLocation = data.ReplenishLocationIndexs.ToList();


                List<wm_BinBalance> StorageBinBalances;
                decimal ReplenishQty, SumLocationBinBalanceQty, SumStorageBalanceQty, PendingReplenishQty, StorageQty;

                foreach (SearchReplenishmentBalanceItemModel Item in data.Items)
                {

                    SumLocationBinBalanceQty = dbBa.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             //(s.Location_Index.Equals(Item.Location_Index)) &&
                             (s.Location_Index.Equals("7738EA53-37F6-4AC9-8E87-9F3FEF779CCD")) &&
                             (s.BinBalance_QtyReserve >= 0) &&
                             (s.BinBalance_QtyBal > 0)
                    ).Sum(s => s.BinBalance_QtyBal) ?? 0; // (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;

                    ReplenishQty = (Item.Replenish_Qty > Item.Minimum_Qty ? Item.Replenish_Qty : Item.Minimum_Qty) - Item.Pending_Replenish_Qty - SumLocationBinBalanceQty;

                    if (ReplenishQty <= 0)
                    {
                        //BinBalance no need to Replenishment
                        continue;
                    }

                    StorageBinBalances = dbBa.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             (s.BinBalance_QtyBal - s.BinBalance_QtyReserve > 0) &&
                             //(data.ReplenishLocationIndexs.Contains(s.Location_Index)) &&
                             (lstLocation.Contains(s.Location_Index)) &&
                             (s.BinBalance_QtyReserve == 0) &&
                             (data.ReplenishItemStatusIndexs.Contains(s.ItemStatus_Index)) &&
                             (s.ERP_Location == "AB01") &&
                             (s.Location_Id != "BUF-IP")
                    ).OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(d => d.Location_Name).ToList();

                    if ((StorageBinBalances?.Count ?? 0) == 0)
                    {
                        //Storage BinBalance not found
                        continue;
                    }

                    SumStorageBalanceQty = StorageBinBalances.Sum(s => (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;
                    if (ReplenishQty > SumStorageBalanceQty)
                    {


                    }

                    PendingReplenishQty = ReplenishQty;
                    foreach (wm_BinBalance StorageBalance in StorageBinBalances.OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(q => q.BinBalance_QtyBal).ThenBy(d => d.Location_Name))
                    {

                        #region chk shelflife
                        int? dateDiffGetdate = 0;
                        int? productShelfLife_D = 0;
                        int? remainingShelfLife = 0;

                        if (!string.IsNullOrEmpty(StorageBalance.GoodsReceive_EXP_Date.ToString()))
                        {
                            dateDiffGetdate = (Convert.ToDateTime(StorageBalance.GoodsReceive_EXP_Date) - DateTime.Now).Days;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(StorageBalance.GoodsReceive_Date.ToString()))
                            {
                                dateDiffGetdate = (Convert.ToDateTime(StorageBalance.GoodsReceive_Date) - DateTime.Now).Days;
                            }
                        }

                        var resProduct = dbMaster.Ms_Product.Where(c => c.Product_Index == StorageBalance.Product_Index).FirstOrDefault();
                        if (resProduct != null)
                        {
                            //productShelfLife_D = resProduct.ProductShelfLife_D;
                            productShelfLife_D = (resProduct.ProductShelfLife_D ?? 0) == 0 ? 0 : resProduct.ProductShelfLife_D;
                        }

                        remainingShelfLife = (dateDiffGetdate > 0 && productShelfLife_D > 0) ? dateDiffGetdate - productShelfLife_D : 0;

                        if(remainingShelfLife < 0)
                        {
                            olog.logging("SearchReplenishmentBinBalanceASRS", "remainingShelfLife < 0 : PalletID [ " + StorageBalance.Tag_No + " ]" + resProduct.Product_Id + " " + resProduct.Product_Name + " remainingShelfLife = " + remainingShelfLife);
                            continue;
                        }

                        #endregion

                        if (PendingReplenishQty <= 0)
                        {
                            break;
                        }

                        StorageQty = StorageBalance.BinBalance_QtyBal.Value - StorageBalance.BinBalance_QtyReserve.Value;
                        if (StorageQty > PendingReplenishQty)
                        {
                            StorageQty = PendingReplenishQty;
                        }

                        ReplenishmentBinBalance.Add(
                            new ReplenishmentBalanceModel()
                            {
                                BinBalance = StorageBalance,
                                Owner_Index = StorageBalance.Owner_Index,
                                Location_Index = Item.Location_Index,
                                Location_Id = Item.Location_Id,
                                Location_Name = Item.Location_Name,
                                Replenish_Qty = StorageQty
                            }
                        );
                        PendingReplenishQty -= StorageQty;
                    }
                }
                return ReplenishmentBinBalance;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ReplenishmentBalanceModel> SearchReplenishmentBinBalancePIECEPICK(string jsonData)
        {
            try
            {
                SearchReplenishmentBalanceModel data = GetSearchReplenishmentBalanceModel(jsonData);
                List<ReplenishmentBalanceModel> ReplenishmentBinBalance = new List<ReplenishmentBalanceModel>();

                List<wm_BinBalance> StorageBinBalances;
                decimal ReplenishQty, SumLocationBinBalanceQty, SumStorageBalanceQty, PendingReplenishQty, StorageQty, QtyBalLocation;

                foreach (SearchReplenishmentBalanceItemModel Item in data.Items)
                {
                    SumLocationBinBalanceQty = dbBa.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             (s.Location_Index.Equals(Item.Location_Index)) &&
                              (s.BinBalance_QtyReserve >= 0) &&
                             (s.BinBalance_QtyBal > 0)
                    ).Sum(s => s.BinBalance_QtyBal) ?? 0; // (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;




                    ReplenishQty = (Item.Replenish_Qty > Item.Minimum_Qty ? Item.Replenish_Qty : Item.Minimum_Qty) - Item.Pending_Replenish_Qty - SumLocationBinBalanceQty;
                    if (ReplenishQty <= 0)
                    {
                        //BinBalance no need to Replenishment
                        continue;
                    }

                    StorageBinBalances = dbBa.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             (s.BinBalance_QtyBal - s.BinBalance_QtyReserve > 0) &&
                             (data.ReplenishLocationIndexs.Contains(s.Location_Index)) &&
                             //    (s.BinBalance_QtyReserve == 0) &&
                             (data.ReplenishItemStatusIndexs.Contains(s.ItemStatus_Index)) &&
                             (s.ERP_Location == "AB01")
                    ).OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(d => d.Location_Name).ToList();

                    if ((StorageBinBalances?.Count ?? 0) == 0)
                    {
                        //Storage BinBalance not found
                        continue;
                    }

                    SumStorageBalanceQty = StorageBinBalances.Sum(s => (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;
                    if (ReplenishQty > SumStorageBalanceQty)
                    {
                        //Not Enough Storage to Replenish
                        //TO DO Add All ?
                    }

                    PendingReplenishQty = ReplenishQty;
                    foreach (wm_BinBalance StorageBalance in StorageBinBalances.OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(d => d.Location_Name))
                    {
                        if (PendingReplenishQty <= 0)
                        {
                            break;
                        }

                        StorageQty = StorageBalance.BinBalance_QtyBal.Value - StorageBalance.BinBalance_QtyReserve.Value;
                        if (StorageQty > PendingReplenishQty)
                        {
                            StorageQty = PendingReplenishQty;
                        }

                        ReplenishmentBinBalance.Add(
                            new ReplenishmentBalanceModel()
                            {
                                BinBalance = StorageBalance,
                                Owner_Index = StorageBalance.Owner_Index,
                                Location_Index = Item.Location_Index,
                                Location_Id = Item.Location_Id,
                                Location_Name = Item.Location_Name,
                                Replenish_Qty = StorageQty
                            }
                        );
                        PendingReplenishQty -= StorageQty;
                    }
                }
                return ReplenishmentBinBalance;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ReplenishmentBalanceModel> SearchReplenishmentBinBalancePIECEPICK_V2(string jsonData)
        {
            var olog = new logtxt();
            try
            {
                SearchReplenishmentBalanceModel data = GetSearchReplenishmentBalanceModel(jsonData);
                List<ReplenishmentBalanceModel> ReplenishmentBinBalance = new List<ReplenishmentBalanceModel>();

                List<wm_BinBalance> StorageBinBalances = new List<wm_BinBalance>();
                decimal ReplenishQty, SumLocationBinBalanceQty, SumStorageBalanceQty, PendingReplenishQty, StorageQty, QtyBalLocation;

                foreach (SearchReplenishmentBalanceItemModel Item in data.Items)
                {
                    //SumLocationBinBalanceQty = dbBa.wm_BinBalance.Where(
                    //    s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                    //         (s.Product_Index.Equals(Item.Product_Index)) &&
                    //         (s.Location_Index.Equals(Item.Location_Index)) &&
                    //          (s.BinBalance_QtyReserve >= 0) &&
                    //         (s.BinBalance_QtyBal > 0)
                    //).Sum(s => s.BinBalance_QtyBal - s.BinBalance_QtyReserve) ?? 0; // (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;

                    SumLocationBinBalanceQty = 0;

                    ReplenishQty = (Item.Replenish_Qty > Item.Minimum_Qty ? Item.Replenish_Qty : Item.Minimum_Qty) - Item.Pending_Replenish_Qty - SumLocationBinBalanceQty;
                    if (ReplenishQty <= 0)
                    {
                        //BinBalance no need to Replenishment
                        continue;
                    }

                    BinbalanceDbContext dbBalance = new BinbalanceDbContext();

                    StorageBinBalances = dbBalance.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             (s.BinBalance_QtyBal - s.BinBalance_QtyReserve > 0) &&
                             (data.ReplenishLocationIndexs.Contains(s.Location_Index)) &&
                             //    (s.BinBalance_QtyReserve == 0) &&
                             (data.ReplenishItemStatusIndexs.Contains(s.ItemStatus_Index)) &&
                             (s.ERP_Location == "AB01")
                    ).OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(d => d.Location_Name).ToList();

                    if ((StorageBinBalances?.Count ?? 0) == 0)
                    {
                        //Storage BinBalance not found
                        continue;
                    }

                    SumStorageBalanceQty = StorageBinBalances.Sum(s => (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;
                    if (ReplenishQty > SumStorageBalanceQty)
                    {
                        //Not Enough Storage to Replenish
                        //TO DO Add All ?
                    }

                    PendingReplenishQty = ReplenishQty;
                    foreach (wm_BinBalance StorageBalance in StorageBinBalances.OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(o => o.BinBalance_QtyBal).ThenBy(d => d.Location_Name))
                    {
                        #region chk shelflife
                        int? dateDiffGetdate = 0;
                        int? productShelfLife_D = 0;
                        int? remainingShelfLife = 0;

                        if (!string.IsNullOrEmpty(StorageBalance.GoodsReceive_EXP_Date.ToString()))
                        {
                            dateDiffGetdate = (Convert.ToDateTime(StorageBalance.GoodsReceive_EXP_Date) - DateTime.Now).Days;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(StorageBalance.GoodsReceive_Date.ToString()))
                            {
                                dateDiffGetdate = (Convert.ToDateTime(StorageBalance.GoodsReceive_Date) - DateTime.Now).Days;
                            }
                        }

                        var resProduct = dbMaster.Ms_Product.Where(c => c.Product_Index == StorageBalance.Product_Index).FirstOrDefault();
                        if (resProduct != null)
                        {
                            //productShelfLife_D = resProduct.ProductShelfLife_D;
                            productShelfLife_D = (resProduct.ProductShelfLife_D ?? 0) == 0 ? 0 : resProduct.ProductShelfLife_D;
                        }

                        remainingShelfLife = (dateDiffGetdate > 0 && productShelfLife_D > 0) ? dateDiffGetdate - productShelfLife_D : 0;

                        if (remainingShelfLife < 0)
                        {
                            olog.logging("SearchReplenishmentBinBalancePIECEPICK_V2", "remainingShelfLife < 0 : PalletID [ " + StorageBalance.Tag_No + " ]" + resProduct.Product_Id + " " + resProduct.Product_Name + " remainingShelfLife = " + remainingShelfLife);
                            continue;
                        }

                        #endregion

                        if (PendingReplenishQty <= 0)
                        {
                            break;
                        }

                        StorageQty = StorageBalance.BinBalance_QtyBal.Value - StorageBalance.BinBalance_QtyReserve.Value;
                        if (StorageQty > PendingReplenishQty)
                        {
                            StorageQty = PendingReplenishQty;
                        }

                        ReplenishmentBinBalance.Add(
                            new ReplenishmentBalanceModel()
                            {
                                BinBalance = StorageBalance,
                                Owner_Index = StorageBalance.Owner_Index,
                                Location_Index = Item.Location_Index,
                                Location_Id = Item.Location_Id,
                                Location_Name = Item.Location_Name,
                                Replenish_Qty = StorageQty
                            }
                        );
                        PendingReplenishQty -= StorageQty;
                    }
                }
                return ReplenishmentBinBalance;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int CalculatePalletBypass(SearchReplenishmentBalanceModel model)
        {
            var olog = new logtxt();

            try
            {
                Guid StorageLocationTypeIndex = Guid.Parse("02F5CBFC-769A-411B-9146-1D27F92AE82D");   // ASRS
                List<Guid> ReplenishLocationIndexs =
                    JsonConvert.DeserializeObject<List<Guid>>(
                    JsonConvert.SerializeObject(
                        dbMaster.Ms_Location.Where(s => s.IsActive == 1 && s.LocationType_Index == StorageLocationTypeIndex).Select(s => s.Location_Index)));
                if ((ReplenishLocationIndexs?.Count ?? 0) == 0)
                {
                    olog.logging("ActivateReplenishmentASRS", "Replenish Location not found");
                    throw new Exception("Replenish Location not found");
                }

                SearchReplenishmentBalanceModel data = model;
                List<ReplenishmentBalanceModel> ReplenishmentBinBalance = new List<ReplenishmentBalanceModel>();

                List<Guid> lstLocation = ReplenishLocationIndexs;

                List<wm_BinBalance> StorageBinBalances;
                decimal ReplenishQty, SumLocationBinBalanceQty, SumStorageBalanceQty, PendingReplenishQty, StorageQty;

                foreach (SearchReplenishmentBalanceItemModel Item in data.Items)
                {

                    ReplenishQty = Item.Replenish_Qty;

                    StorageBinBalances = dbBa.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals("02B31868-9D3D-448E-B023-05C121A424F4") : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             (s.BinBalance_QtyBal - s.BinBalance_QtyReserve > 0) &&
                             //(data.ReplenishLocationIndexs.Contains(s.Location_Index)) &&
                             (lstLocation.Contains(s.Location_Index)) &&
                             (s.BinBalance_QtyReserve == 0) &&
                             (s.ItemStatus_Index == new Guid("525BCFF1-2AD9-4ACB-819D-0DEA4E84EA12")) &&
                             (s.ERP_Location == "AB01") &&
                             (s.Location_Id != "BUF-IP")
                    ).OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(d => d.Location_Name).ToList();

                    if ((StorageBinBalances?.Count ?? 0) == 0)
                    {
                        //Storage BinBalance not found
                        continue;
                    }

                    SumStorageBalanceQty = StorageBinBalances.Sum(s => (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;
                    if (ReplenishQty > SumStorageBalanceQty)
                    {

                    }

                    PendingReplenishQty = ReplenishQty;
                    foreach (wm_BinBalance StorageBalance in StorageBinBalances.OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(q => q.BinBalance_QtyBal).ThenBy(d => d.Location_Name))
                    {

                        #region chk shelflife
                        int? dateDiffGetdate = 0;
                        int? productShelfLife_D = 0;
                        int? remainingShelfLife = 0;

                        if (!string.IsNullOrEmpty(StorageBalance.GoodsReceive_EXP_Date.ToString()))
                        {
                            dateDiffGetdate = (Convert.ToDateTime(StorageBalance.GoodsReceive_EXP_Date) - DateTime.Now).Days;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(StorageBalance.GoodsReceive_Date.ToString()))
                            {
                                dateDiffGetdate = (Convert.ToDateTime(StorageBalance.GoodsReceive_Date) - DateTime.Now).Days;
                            }
                        }

                        var resProduct = dbMaster.Ms_Product.Where(c => c.Product_Index == StorageBalance.Product_Index).FirstOrDefault();
                        if (resProduct != null)
                        {
                            //productShelfLife_D = resProduct.ProductShelfLife_D;
                            productShelfLife_D = (resProduct.ProductShelfLife_D ?? 0) == 0 ? 0 : resProduct.ProductShelfLife_D;
                        }

                        remainingShelfLife = (dateDiffGetdate > 0 && productShelfLife_D > 0) ? dateDiffGetdate - productShelfLife_D : 0;

                        if (remainingShelfLife < 0)
                        {
                            //olog.logging("SearchReplenishmentBinBalanceASRS", "remainingShelfLife < 0 : PalletID [ " + StorageBalance.Tag_No + " ]" + resProduct.Product_Id + " " + resProduct.Product_Name + " remainingShelfLife = " + remainingShelfLife);
                            continue;
                        }

                        #endregion

                        if (PendingReplenishQty <= 0)
                        {
                            break;
                        }

                        StorageQty = StorageBalance.BinBalance_QtyBal.Value - StorageBalance.BinBalance_QtyReserve.Value;
                        if (StorageQty > PendingReplenishQty)
                        {
                            StorageQty = PendingReplenishQty;
                        }

                        ReplenishmentBinBalance.Add(
                            new ReplenishmentBalanceModel()
                            {
                                BinBalance = StorageBalance,
                                Owner_Index = StorageBalance.Owner_Index,
                                Location_Index = Item.Location_Index,
                                Location_Id = Item.Location_Id,
                                Location_Name = Item.Location_Name,
                                Replenish_Qty = StorageQty
                            }
                        );
                        PendingReplenishQty -= StorageQty;
                    }
                }
                return ReplenishmentBinBalance.Count();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region + Active Replenishment PiecePick On demand +

        public List<string> ActivateReplenishmentPiecePickOndemand(FilterReplenishOnDemandViewModel model)
        {
            String State = "Start";
            String msglog = "";
            var olog = new logtxt();

            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                olog.logging("ActivateReplenishmentPiecePickOndemand", State);
                olog.logging("ActivateReplenishmentPiecePickOndemand", "Request : " + JsonConvert.SerializeObject(model));

                string _productId = "";

                foreach (var item in model.lstReplenishOnDemand)
                {
                    if (!string.IsNullOrEmpty(_productId))
                    {
                        _productId += ",";
                    }

                    _productId += item.product_Id;
                }

                var productId = new SqlParameter("@productId", _productId);
                var roweffect = dbMaster.Database.ExecuteSqlCommand("EXEC sp_UpdateReplenishOnDemandActive @productId", productId);

                System.Threading.Thread.Sleep(10000);

                //Find Task Replenish
                DateTime currentDate = DateTime.Today;
                TimeSpan currentTime = new TimeSpan(0, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                int dayOfWeek = (int)currentDate.DayOfWeek;
                List<Ms_Replenishment> Replenishments = dbMaster.Ms_Replenishment.Where(
                   w => w.IsActive == 1 && (currentTime >= w.Trigger_Time && currentTime <= w.Trigger_Time_End) && (
                      ((dayOfWeek == 0 ? w.IsSunday :
                        dayOfWeek == 1 ? w.IsMonday :
                        dayOfWeek == 2 ? w.IsTuesday :
                        dayOfWeek == 3 ? w.IsWednesday :
                        dayOfWeek == 4 ? w.IsThursday :
                        dayOfWeek == 5 ? w.IsFriday :
                        dayOfWeek == 6 ? w.IsSaturday : false) == true && w.Trigger_Date == null) ||
                        (w.Trigger_Date.HasValue ? w.Trigger_Date : currentDate.AddDays(-1)) >= currentDate &&
                        (w.Trigger_Date_End.HasValue ? w.Trigger_Date_End : w.Trigger_Date) <= currentDate)
                ).ToList();

                if (Replenishments.Count == 0)
                {
                    //no task found.
                    olog.logging("ActivateReplenishmentPiecePickOndemand", "Task Replenish not found");
                    throw new Exception("Task Replenish not found");
                }

                //TO DO Config Index
                //Prepare BinBalance Model
                Guid GoodsReplenishDocumentTypeIndex = Guid.Parse("47BF1845-33D1-47FE-B38D-7BDDF0E48A7E"); // Auto Replenishment PP
                Ms_DocumentType goodsReplenishDocumentType = dbMaster.Ms_DocumentType.Find(GoodsReplenishDocumentTypeIndex);
                if (goodsReplenishDocumentType is null)
                {
                    olog.logging("ActivateReplenishmentPiecePickOndemand", "Replenish DocumentType not found");
                    throw new Exception("Replenish DocumentType not found");
                }

                // Fix TOP 100
                Guid PiecepickLocationTypeIndex = Guid.Parse("8A545442-77A3-43A4-939A-6B9102DFE8C6");   // Reple Area
                Guid SelectiveLocationTypeIndex = Guid.Parse("F9EDDAEC-A893-4F63-A700-526C69CC08C0");   // Selective
                Guid SelectiveOnGroundLocationTypeIndex = Guid.Parse("BA0142A8-98B7-4E0B-A1CE-6266716F5F67");   // Selective on Ground
                Guid DummyLocationTypeIndex = Guid.Parse("6B29D097-FB5D-4981-A09A-A46F636C82F1");   // Dummy ByPass
                List<Guid> ReplenishLocationIndexs =
                    JsonConvert.DeserializeObject<List<Guid>>(
                    JsonConvert.SerializeObject(
                        dbMaster.Ms_Location.Where(s => s.IsActive == 1 && (s.LocationType_Index == PiecepickLocationTypeIndex
                            || s.LocationType_Index == SelectiveLocationTypeIndex
                            || s.LocationType_Index == SelectiveOnGroundLocationTypeIndex
                            || s.LocationType_Index == DummyLocationTypeIndex)
                        && !s.Location_Id.Contains("BUF")
                        ).Select(s => s.Location_Index)));
                if ((ReplenishLocationIndexs?.Count ?? 0) == 0)
                {
                    olog.logging("ActivateReplenishmentPiecePickOndemand", "Replenish Location not found");
                    throw new Exception("Replenish Location not found");
                }

                List<Guid> ReplenishItemStatusIndexs = new List<Guid> { Guid.Parse("525BCFF1-2AD9-4ACB-819D-0DEA4E84EA12") };
                SearchReplenishmentBalanceModel binBalance_API_Model = new SearchReplenishmentBalanceModel()
                {
                    ReplenishLocationIndexs = ReplenishLocationIndexs,
                    ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                };

                List<string> errorMsg = new List<string>();
                List<ReplenishmentBalanceModel> binBalances;

                ////AddHeader
                //Guid GoodsReplenishIndex;
                //string GoodsReplenishNo;
                //Im_GoodsTransfer GoodsReplenish;
                //DateTime ActiveDate = DateTime.Now;
                //string ActiveBy = "System";

                //GoodsReplenishIndex = Guid.NewGuid();
                //GoodsReplenishNo = GetDocumentNumber(goodsReplenishDocumentType, ActiveDate);
                //GoodsReplenish = new Im_GoodsTransfer()
                //{
                //    //Replenishment_Index = ReplenishIndex,

                //    GoodsTransfer_Index = GoodsReplenishIndex,
                //    GoodsTransfer_No = GoodsReplenishNo,
                //    GoodsTransfer_Date = ActiveDate,
                //    GoodsTransfer_Time = ActiveDate.ToShortTimeString(),
                //    GoodsTransfer_Doc_Date = ActiveDate,
                //    GoodsTransfer_Doc_Time = ActiveDate.ToShortTimeString(),
                //    Owner_Index = new Guid("02B31868-9D3D-448E-B023-05C121A424F4"),
                //    Owner_Id = "3419",
                //    Owner_Name = "Amazon",
                //    DocumentType_Index = goodsReplenishDocumentType.DocumentType_Index,
                //    DocumentType_Id = goodsReplenishDocumentType.DocumentType_Id,
                //    DocumentType_Name = goodsReplenishDocumentType.DocumentType_Name,

                //    Document_Status = 0, // 1

                //    Create_By = ActiveBy,
                //    Create_Date = ActiveDate
                //};
                //dbTf.Im_GoodsTransfer.Add(GoodsReplenish);

                //foreach (Ms_Replenishment replenishment in Replenishments)
                foreach (ReplenishOnDemandViewModel replenishment in model.lstReplenishOnDemand)
                {
                    try
                    {
                        List<View_Replenishment> _lstViewReplenish = new List<View_Replenishment>();

                        var _modelViewReplenish = dbMaster.View_Replenishment.Where(c => c.Product_Id == replenishment.product_Id).ToList();

                        if(_modelViewReplenish.Count == 0)
                        {
                            olog.logging("ActivateReplenishmentPiecePickOndemand", " View_Replenishment Not found. Product_Id :  " + replenishment.product_Id);
                            continue;
                        }

                        View_Replenishment _modelTop1PA = new View_Replenishment();
                        View_Replenishment _modelTop1PB = new View_Replenishment();
                        View_Replenishment _modelTop1VC = new View_Replenishment();

                        //ถ้า Min_Qty มีค่ามากกว่าหรือเท่ากับ ondemand ที่ต้องเติม ให้ เติมแค่ฝั่งเดียวคือ PA

                        decimal _replenish_Qty = (replenishment.diff_QtyPiecePickWithOrder ?? 0) * -1; // จำนวณที่ต้องการเติมจากหน้า frontend
                        decimal _su_replenish_Qty = (replenishment.diff_SU_QtyPiecePickWithOrder ?? 0) * -1; // จำนวณ SU ที่ต้องการเติมจากหน้า frontend
                        decimal _su_ratio = (replenishment.su_Ratio ?? 1); // จำนวณ SU Ratio frontend

                        decimal _replenish_Qty_PA = 0;
                        decimal _replenish_Qty_PB = 0;
                        decimal _replenish_Qty_VC = 0;

                        //TO DO _su_replenish_Qty มีทศนิยม

                        //

                        if (_replenish_Qty == 0)
                        {
                            olog.logging("ActivateReplenishmentPiecePickOndemand", " _replenish_Qty Equal 0 : " + replenishment.product_Id);
                            continue;
                        }

                        if (_modelViewReplenish.FirstOrDefault().Min_Qty >= (_su_replenish_Qty))
                        {
                            _modelTop1PA = dbMaster.View_Replenishment.Where(c => c.Product_Id == replenishment.product_Id && c.LocType == "PA").OrderBy(o => o.Location_Name).Take(1).FirstOrDefault();
                            if(_modelTop1PA != null)
                            {
                                _modelTop1PA.Replenish_Qty = _replenish_Qty;
                                _lstViewReplenish.Add(_modelTop1PA);
                            }

                            _modelTop1VC = dbMaster.View_Replenishment.Where(c => c.Product_Id == replenishment.product_Id && c.LocType == "VC").OrderBy(o => o.Location_Name).Take(1).FirstOrDefault();
                            if (_modelTop1VC != null)
                            {
                                _modelTop1VC.Replenish_Qty = _replenish_Qty;
                                _lstViewReplenish.Add(_modelTop1VC);
                            }
                        }
                        else
                        {
                            if (Convert.ToBoolean(Convert.ToInt16(_su_replenish_Qty) % 2))
                            {
                                // หาร 2 ไม่ลงตัว
                                _replenish_Qty_PA = Math.Floor(_su_replenish_Qty / 2) + 1; // saleunit
                                _replenish_Qty_PB = Math.Floor(_su_replenish_Qty / 2); // saleunit
                            }
                            else
                            {
                                // หาร 2 ลงตัว
                                _replenish_Qty_PA = Math.Floor(_su_replenish_Qty / 2); // saleunit
                                _replenish_Qty_PB = Math.Floor(_su_replenish_Qty / 2); // saleunit
                            }

                            _modelTop1PA = dbMaster.View_Replenishment.Where(c => c.Product_Id == replenishment.product_Id && c.LocType == "PA").OrderBy(o => o.Location_Name).Take(1).FirstOrDefault();
                            if (_modelTop1PA != null)
                            {
                                _modelTop1PA.Replenish_Qty = _replenish_Qty_PA * _su_ratio;
                                _lstViewReplenish.Add(_modelTop1PA);
                            }

                            _modelTop1PB = dbMaster.View_Replenishment.Where(c => c.Product_Id == replenishment.product_Id && c.LocType == "PB").OrderBy(o => o.Location_Name).Take(1).FirstOrDefault();
                            if (_modelTop1PB != null)
                            {
                                _modelTop1PB.Replenish_Qty = _replenish_Qty_PB * _su_ratio;
                                _lstViewReplenish.Add(_modelTop1PB);
                            }

                            _modelTop1VC = dbMaster.View_Replenishment.Where(c => c.Product_Id == replenishment.product_Id && c.LocType == "VC").OrderBy(o => o.Location_Name).Take(1).FirstOrDefault();
                            if (_modelTop1VC != null)
                            {
                                _modelTop1VC.Replenish_Qty = _replenish_Qty;
                                _lstViewReplenish.Add(_modelTop1VC);
                            }
                        }

                        //binBalances = new List<ReplenishmentBalanceModel>();
                        //binBalance_API_Model = new SearchReplenishmentBalanceModel()
                        //{
                        //    ReplenishLocationIndexs = ReplenishLocationIndexs,
                        //    ReplenishItemStatusIndexs = ReplenishItemStatusIndexs,
                            
                        //};

                        foreach (View_Replenishment item in _lstViewReplenish)
                        {
                            try
                            {
                                Ms_Replenishment _replenishment = dbMaster.Ms_Replenishment.Where(c => c.Replenishment_Index == item.Replenishment_Index).FirstOrDefault();

                                if(_replenishment == null)
                                {
                                    olog.logging("ActivateReplenishmentPiecePickOndemand", " Ms_Replenishment Not found. Replenishment_Id :  " + _replenishment.Replenishment_Id);
                                    continue;
                                }

                                binBalances = new List<ReplenishmentBalanceModel>();
                                binBalance_API_Model = new SearchReplenishmentBalanceModel()
                                {
                                    ReplenishLocationIndexs = ReplenishLocationIndexs,
                                    ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                                };

                                olog.logging("ActivateReplenishmentPiecePickOndemand", " GetBinBalanceReplenish Replenishment_Id :  " + _replenishment.Replenishment_Id);


                                binBalances = GetBinBalanceReplenishVC(goodsReplenishDocumentType, _replenishment, binBalance_API_Model, item.Replenish_Qty ?? 0);
                                if ((binBalances?.Count ?? 0) > 0)
                                {
                                    olog.logging("ActivateReplenishmentPiecePickOndemand", " CreateReplenishDocument Replenishment_Id :  " + _replenishment.Replenishment_Id);

                                    GoodsReplenishDocuments.AddRange(
                                        CreateReplenishDocumentOndemand(_replenishment.Replenishment_Index, goodsReplenishDocumentType, binBalances) //replenishment.Replenishment_Index
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                olog.logging("ActivateReplenishmentPiecePickOndemand", "GetBinBalanceReplenish " + ex.Message);

                                errorMsg.Add(ex.Message);
                                continue;
                            }
                        }

                        //olog.logging("ActivateReplenishmentPiecePickOndemand", " GetBinBalanceReplenish Replenishment_Id :  " + replenishment.Replenishment_Id);

                        //binBalances = GetBinBalanceReplenishVC(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
                        //if ((binBalances?.Count ?? 0) > 0)
                        //{
                        //    olog.logging("ActivateReplenishmentPiecePickOndemand", " CreateReplenishDocument Replenishment_Id :  " + replenishment.Replenishment_Id);

                        //    //CreateReplenishDocumentOndemand(GoodsReplenishIndex, GoodsReplenishNo, goodsReplenishDocumentType, binBalances); //replenishment.Replenishment_Index
                        //}
                    }
                    catch (Exception ex)
                    {
                        olog.logging("ActivateReplenishmentPiecePickOndemand", "GetBinBalanceReplenish " + ex.Message);

                        errorMsg.Add(ex.Message);
                        continue;
                    }
                }

                if (errorMsg.Count > 0)
                {
                    olog.logging("ActivateReplenishmentPiecePickOndemand", "Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                    throw new Exception("Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                }

                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                olog.logging("ActivateReplenishmentPiecePickOndemand", ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
                throw new Exception(ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
            }
        }

        private decimal GetPendingReplenishQty(Guid? OwnerIndex, Guid documentTypeIndex, Guid productIndex, Guid locationIndex)
        {
            try
            {
                decimal pendingReplenishQty = 0;

                List<Im_GoodsTransfer> goodsTransfers = dbTf.Im_GoodsTransfer.Where(
                    w => w.Owner_Index.IsEquals(OwnerIndex) &&
                         w.DocumentType_Index == documentTypeIndex && (w.Document_Status != -1 && w.Document_Status != 3)).ToList(); // w.Document_Status == 1
                if ((goodsTransfers?.Count ?? 0) > 0)
                {
                    List<Guid> listTransferIndex = goodsTransfers.Select(s => s.GoodsTransfer_Index).ToList();
                    pendingReplenishQty = dbTf.Im_GoodsTransferItem.Where(
                        w => listTransferIndex.Contains(w.GoodsTransfer_Index) &&
                             w.Document_Status != -1 &&
                             w.Product_Index == productIndex &&
                             w.Location_Index_To == locationIndex
                    ).Sum(s => s.TotalQty) ?? 0;
                }

                return pendingReplenishQty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ReplenishmentBalanceModel> GetBinBalanceReplenishVC(Ms_DocumentType documentType, Ms_Replenishment replenishment, SearchReplenishmentBalanceModel binBalance_API_Model , decimal _replenish_Qty)
        {
            try
            {
                IQueryable<View_ProductLocation> queryProductLocations = dbMaster.View_ProductLocation.AsQueryable().Where(w => w.IsActive == 1 && w.IsDelete == 0);

                bool planByProduct = replenishment.Plan_By_Product == 1;
                bool planByLocation = replenishment.Plan_By_Location == 1;

                if (planByProduct)
                {
                    List<Ms_Replenishment_Product> replenishProducts = dbMaster.Ms_Replenishment_Product.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishProducts?.Count ?? 0) > 0)
                    {
                        List<Guid?> listProductIndex = replenishProducts.Where(w => w.Product_Index.HasValue).Select(s => s.Product_Index).Distinct().ToList();
                        List<Guid?> listProductTypeIndex = replenishProducts.Where(w => !w.Product_Index.HasValue).Select(s => (Guid?)s.ProductType_Index).Distinct().ToList();

                        List<Ms_Product> products = new List<Ms_Product>();
                        if ((listProductTypeIndex?.Count ?? 0) > 0)
                        {
                            products = dbMaster.Ms_Product.Where(
                                    w => w.IsActive == 1 && w.IsDelete == 0 && listProductTypeIndex.Contains(w.ProductCategory_Index)
                            ).ToList();

                            if ((products?.Count ?? 0) > 0)
                            {
                                (listProductIndex ?? new List<Guid?>()).AddRange(products.Select(s => (Guid?)s.Product_Index).ToList());
                            }
                        }

                        if ((listProductIndex?.Count ?? 0) > 0)
                        {
                            listProductIndex = listProductIndex.Distinct().ToList();
                            queryProductLocations = queryProductLocations.Where(w => listProductIndex.Contains(w.Product_Index));
                        }
                    }
                }

                if (planByLocation)
                {
                    List<Ms_Replenishment_Location> replenishLocations = dbMaster.Ms_Replenishment_Location.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishLocations?.Count ?? 0) > 0)
                    {
                        List<Guid?> listLocationIndex = replenishLocations.Where(w => w.Location_Index.HasValue).Select(s => s.Location_Index).Distinct().ToList();
                        List<Guid?> listZoneIndex = replenishLocations.Where(w => w.Zone_Index.HasValue).Select(s => s.Zone_Index).Distinct().ToList();

                        List<Ms_ZoneLocation> zoneLocations = new List<Ms_ZoneLocation>();
                        if ((listZoneIndex?.Count ?? 0) > 0)
                        {
                            zoneLocations = dbMaster.Ms_ZoneLocation.Where(
                                w => w.IsActive == 1 && w.IsDelete == 0 && listZoneIndex.Contains(w.Zone_Index)
                            ).ToList();

                            if ((zoneLocations?.Count ?? 0) > 0)
                            {
                                (listLocationIndex ?? new List<Guid?>()).AddRange(zoneLocations.Select(s => (Guid?)s.Location_Index).ToList());
                            }
                        }

                        if ((listLocationIndex?.Count ?? 0) > 0)
                        {
                            listLocationIndex = listLocationIndex.Distinct().ToList();
                            queryProductLocations = queryProductLocations.Where(w => listLocationIndex.Contains(w.Location_Index));
                        }
                    }
                }

                List<View_ProductLocation> productLocations = queryProductLocations.ToList();
                if ((productLocations?.Count ?? 0) == 0) { throw new Exception("ProductLocation not found"); }

                Boolean checkLocation = false;
                var LocVC = productLocations.Where(c => c.Location_Name.Contains("VC")).ToList();
                if (LocVC.Count > 0)
                {
                    checkLocation = true;
                }
                var LocPA = productLocations.Where(c => c.Location_Name.Contains("PA")).ToList();
                if (LocPA.Count > 0)
                {
                    checkLocation = true;
                }
                var LocPB = productLocations.Where(c => c.Location_Name.Contains("PB")).ToList();
                if (LocPB.Count > 0)
                {
                    checkLocation = true;
                }

                productLocations.ForEach(e => binBalance_API_Model.Items.Add(
                    new SearchReplenishmentBalanceItemModel()
                    {
                        Product_Index = e.Product_Index,
                        Location_Index = e.Location_Index,
                        Location_Id = e.Location_Id,
                        Location_Name = e.Location_Name,
                        Minimum_Qty = 0,
                        Replenish_Qty = _replenish_Qty,//e.Replenish_Qty,
                        Pending_Replenish_Qty = 0 //GetPendingReplenishQty(binBalance_API_Model.Owner_Index, documentType.DocumentType_Index, e.Product_Index, e.Location_Index),

                    }
                ));

                //binBalance_API_Model.Items.RemoveAll(r => r.Pending_Replenish_Qty >= r.Minimum_Qty);

                //if (binBalance_API_Model.Items.Count == 0) { throw new Exception("Already Pending Replenish"); }

                List<ReplenishmentBalanceModel> BinBalances = new List<ReplenishmentBalanceModel>();
                //Send API BinBalance
                if (checkLocation == true)
                {
                    // VC01
                    //BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalanceVC"), JsonConvert.SerializeObject(binBalance_API_Model));

                    BinBalances = SearchReplenishmentBinBalancePIECEPICK_V2(JsonConvert.SerializeObject(binBalance_API_Model));

                }
                else
                {

                }

                //  List<ReplenishmentBalanceModel> BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));

                return BinBalances;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<string> CreateReplenishDocumentOndemand(Guid ReplenishIndex, Ms_DocumentType GoodsReplenishDocumentType, List<ReplenishmentBalanceModel> BinBalances)
        {
            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                dbBa.Database.SetCommandTimeout(120);
                var olog = new logtxt();

                List<ReplenishmentBalanceModel> OwnerBinBalances;

                Guid GoodsReplenishIndex, GoodsReplenishItemIndex;
                string GoodsReplenishNo;

                //GoodsReplenishIndex = _GoodsReplenishIndex;
                //GoodsReplenishNo = _GoodsReplenishNo;

                Im_GoodsTransfer GoodsReplenish;
                Im_GoodsTransferItem GoodsReplenishItem;
                DateTime ActiveDate = DateTime.Now;
                string ActiveBy = "System";


                var modelassjob = new View_AssignJobLocViewModel();


                List<ReserveBinBalanceItemModel> reserveModel = new List<ReserveBinBalanceItemModel>();
                List<Guid> OwnerIndexs = BinBalances.Select(s => s.Owner_Index).Distinct().ToList();
                foreach (Guid OwnerIndex in OwnerIndexs)
                {
                    reserveModel.Clear();
                    OwnerBinBalances = BinBalances.Where(s => s.Owner_Index == OwnerIndex).ToList();

                    //Header
                    GoodsReplenishIndex = Guid.NewGuid();
                    GoodsReplenishNo = GetDocumentNumber(GoodsReplenishDocumentType, ActiveDate);
                    GoodsReplenish = new Im_GoodsTransfer()
                    {
                        Replenishment_Index = ReplenishIndex,

                        GoodsTransfer_Index = GoodsReplenishIndex,
                        GoodsTransfer_No = GoodsReplenishNo,
                        GoodsTransfer_Date = ActiveDate,
                        GoodsTransfer_Time = ActiveDate.ToShortTimeString(),
                        GoodsTransfer_Doc_Date = ActiveDate,
                        GoodsTransfer_Doc_Time = ActiveDate.ToShortTimeString(),
                        Owner_Index = OwnerBinBalances[0].BinBalance.Owner_Index,
                        Owner_Id = OwnerBinBalances[0].BinBalance.Owner_Id,
                        Owner_Name = OwnerBinBalances[0].BinBalance.Owner_Name,
                        DocumentType_Index = GoodsReplenishDocumentType.DocumentType_Index,
                        DocumentType_Id = GoodsReplenishDocumentType.DocumentType_Id,
                        DocumentType_Name = GoodsReplenishDocumentType.DocumentType_Name,

                        Document_Status = 0, // 1

                        Create_By = ActiveBy,
                        Create_Date = ActiveDate
                    };
                    dbTf.Im_GoodsTransfer.Add(GoodsReplenish);

                    modelassjob.goodsTransfer_Index = GoodsReplenishIndex;
                    modelassjob.Create_By = ActiveBy;
                    modelassjob.Template = "1";

                    //Items
                    foreach (ReplenishmentBalanceModel Item in OwnerBinBalances)
                    {
                        if ((Item.BinBalance.BinBalance_QtyBal ?? 0) <= 0)
                        {
                            continue;
                        }

                        GoodsReplenishItemIndex = Guid.NewGuid();
                        GoodsReplenishItem = new Im_GoodsTransferItem();

                        GoodsReplenishItem.GoodsTransferItem_Index = GoodsReplenishItemIndex;
                        GoodsReplenishItem.GoodsTransfer_Index = GoodsReplenishIndex;
                        GoodsReplenishItem.GoodsReceiveItem_Index = Item.BinBalance.GoodsReceiveItem_Index;
                        GoodsReplenishItem.GoodsReceive_Index = Item.BinBalance.GoodsReceive_Index;
                        GoodsReplenishItem.GoodsReceiveItemLocation_Index = Item.BinBalance.GoodsReceiveItemLocation_Index;
                        GoodsReplenishItem.LineNum = (OwnerBinBalances.IndexOf(Item) + 1).ToString();

                        GoodsReplenishItem.TagItem_Index = Item.BinBalance.TagItem_Index;
                        GoodsReplenishItem.Tag_Index = Item.BinBalance.Tag_Index;
                        GoodsReplenishItem.Tag_No = Item.BinBalance.Tag_No;
                        GoodsReplenishItem.Owner_Index = Item.BinBalance.Owner_Index;
                        GoodsReplenishItem.Owner_Id = Item.BinBalance.Owner_Id;
                        GoodsReplenishItem.Owner_Name = Item.BinBalance.Owner_Name;
                        GoodsReplenishItem.GoodsReceive_MFG_Date = Item.BinBalance.GoodsReceive_MFG_Date; //addnew
                        GoodsReplenishItem.GoodsReceive_MFG_Date_To = Item.BinBalance.GoodsReceive_MFG_Date; //addnew
                        GoodsReplenishItem.GoodsReceive_EXP_Date = Item.BinBalance.GoodsReceive_EXP_Date;
                        GoodsReplenishItem.GoodsReceive_EXP_Date_To = Item.BinBalance.GoodsReceive_EXP_Date; //addnew
                        GoodsReplenishItem.Product_Index = Item.BinBalance.Product_Index;
                        GoodsReplenishItem.Product_Id = Item.BinBalance.Product_Id;
                        GoodsReplenishItem.Product_Name = Item.BinBalance.Product_Name;
                        GoodsReplenishItem.Product_SecondName = Item.BinBalance.Product_SecondName;
                        GoodsReplenishItem.Product_ThirdName = Item.BinBalance.Product_ThirdName;
                        GoodsReplenishItem.Product_Lot = Item.BinBalance.Product_Lot;
                        GoodsReplenishItem.Product_Lot_To = Item.BinBalance.Product_Lot; //addnew
                        GoodsReplenishItem.ItemStatus_Index = Item.BinBalance.ItemStatus_Index;
                        GoodsReplenishItem.ItemStatus_Id = Item.BinBalance.ItemStatus_Id;
                        GoodsReplenishItem.ItemStatus_Name = Item.BinBalance.ItemStatus_Name;
                        GoodsReplenishItem.ItemStatus_Index_To = Item.BinBalance.ItemStatus_Index;
                        GoodsReplenishItem.ItemStatus_Id_To = Item.BinBalance.ItemStatus_Id;
                        GoodsReplenishItem.ItemStatus_Name_To = Item.BinBalance.ItemStatus_Name;
                        GoodsReplenishItem.Location_Index = Item.BinBalance.Location_Index;
                        GoodsReplenishItem.Location_Id = Item.BinBalance.Location_Id;
                        GoodsReplenishItem.Location_Name = Item.BinBalance.Location_Name;
                        GoodsReplenishItem.Location_Index_To = Item.Location_Index;
                        GoodsReplenishItem.Location_Id_To = Item.Location_Id;
                        GoodsReplenishItem.Location_Name_To = Item.Location_Name;

                        decimal? SaleUnitRatio = 1;
                        var modelSaleUnit = dbMaster.Ms_ProductConversion.Where(c => c.Product_Index == Item.BinBalance.Product_Index && c.SALE_UNIT == 1).FirstOrDefault();
                        if (modelSaleUnit != null)
                        {
                            SaleUnitRatio = modelSaleUnit.ProductConversion_Ratio;
                        }

                        GoodsReplenishItem.Qty = decimal.Round(Item.Replenish_Qty / (SaleUnitRatio ?? 1), 6);
                        GoodsReplenishItem.Ratio = SaleUnitRatio;

                        //GoodsReplenishItem.Qty = decimal.Round(Item.Replenish_Qty / (Item.BinBalance.BinBalance_Ratio ?? 1), 6);
                        //GoodsReplenishItem.Ratio = Item.BinBalance.BinBalance_Ratio;
                        GoodsReplenishItem.TotalQty = Item.Replenish_Qty;
                        GoodsReplenishItem.ProductConversion_Index = (modelSaleUnit != null) ? modelSaleUnit.ProductConversion_Index : Item.BinBalance.ProductConversion_Index;
                        GoodsReplenishItem.ProductConversion_Id = (modelSaleUnit != null) ? modelSaleUnit.ProductConversion_Id : Item.BinBalance.ProductConversion_Id;
                        GoodsReplenishItem.ProductConversion_Name = (modelSaleUnit != null) ? modelSaleUnit.ProductConversion_Name : Item.BinBalance.ProductConversion_Name;

                        GoodsReplenishItem.UnitVolume = Item.BinBalance.BinBalance_UnitVolumeBal;
                        GoodsReplenishItem.Volume = decimal.Round((Item.Replenish_Qty / Item.BinBalance.BinBalance_QtyBal ?? 1) * (Item.BinBalance.BinBalance_VolumeBal ?? 0), 6);

                        GoodsReplenishItem.UnitGrsWeight = Item.BinBalance.BinBalance_UnitGrsWeightBal;
                        GoodsReplenishItem.UnitGrsWeight_Index = Item.BinBalance.BinBalance_UnitGrsWeightBal_Index;
                        GoodsReplenishItem.UnitGrsWeight_Id = Item.BinBalance.BinBalance_UnitGrsWeightBal_Id;
                        GoodsReplenishItem.UnitGrsWeight_Name = Item.BinBalance.BinBalance_UnitGrsWeightBal_Name;
                        GoodsReplenishItem.UnitGrsWeightRatio = Item.BinBalance.BinBalance_UnitGrsWeightBalRatio;

                        GoodsReplenishItem.GrsWeight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6);
                        GoodsReplenishItem.GrsWeight_Index = Item.BinBalance.BinBalance_GrsWeightBal_Index;
                        GoodsReplenishItem.GrsWeight_Id = Item.BinBalance.BinBalance_GrsWeightBal_Id;
                        GoodsReplenishItem.GrsWeight_Name = Item.BinBalance.BinBalance_GrsWeightBal_Name;
                        GoodsReplenishItem.GrsWeightRatio = Item.BinBalance.BinBalance_GrsWeightBalRatio;

                        GoodsReplenishItem.UnitNetWeight = Item.BinBalance.BinBalance_UnitNetWeightBal;
                        GoodsReplenishItem.UnitNetWeight_Index = Item.BinBalance.BinBalance_UnitNetWeightBal_Index;
                        GoodsReplenishItem.UnitNetWeight_Id = Item.BinBalance.BinBalance_UnitNetWeightBal_Id;
                        GoodsReplenishItem.UnitNetWeight_Name = Item.BinBalance.BinBalance_UnitNetWeightBal_Name;
                        GoodsReplenishItem.UnitNetWeightRatio = Item.BinBalance.BinBalance_UnitNetWeightBalRatio;

                        GoodsReplenishItem.NetWeight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitNetWeightBal ?? 0), 6);
                        GoodsReplenishItem.NetWeight_Index = Item.BinBalance.BinBalance_NetWeightBal_Index;
                        GoodsReplenishItem.NetWeight_Id = Item.BinBalance.BinBalance_NetWeightBal_Id;
                        GoodsReplenishItem.NetWeight_Name = Item.BinBalance.BinBalance_NetWeightBal_Name;
                        GoodsReplenishItem.NetWeightRatio = Item.BinBalance.BinBalance_NetWeightBalRatio;

                        GoodsReplenishItem.UnitWeight = Item.BinBalance.BinBalance_UnitWeightBal;
                        GoodsReplenishItem.UnitWeight_Index = Item.BinBalance.BinBalance_UnitWeightBal_Index;
                        GoodsReplenishItem.UnitWeight_Id = Item.BinBalance.BinBalance_UnitWeightBal_Id;
                        GoodsReplenishItem.UnitWeight_Name = Item.BinBalance.BinBalance_UnitWeightBal_Name;
                        GoodsReplenishItem.UnitWeightRatio = Item.BinBalance.BinBalance_UnitWeightBalRatio;

                        //GoodsReplenishItem.Weight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6);
                        GoodsReplenishItem.Weight_Index = Item.BinBalance.BinBalance_WeightBal_Index;
                        GoodsReplenishItem.Weight_Id = Item.BinBalance.BinBalance_WeightBal_Id;
                        GoodsReplenishItem.Weight_Name = Item.BinBalance.BinBalance_WeightBal_Name;
                        GoodsReplenishItem.WeightRatio = Item.BinBalance.BinBalance_WeightBalRatio;

                        GoodsReplenishItem.UnitWidth = Item.BinBalance.BinBalance_UnitWidthBal;
                        GoodsReplenishItem.UnitWidth_Index = Item.BinBalance.BinBalance_UnitWidthBal_Index;
                        GoodsReplenishItem.UnitWidth_Id = Item.BinBalance.BinBalance_UnitWidthBal_Id;
                        GoodsReplenishItem.UnitWidth_Name = Item.BinBalance.BinBalance_UnitWidthBal_Name;
                        GoodsReplenishItem.UnitWidthRatio = Item.BinBalance.BinBalance_UnitWidthBalRatio;

                        GoodsReplenishItem.Width = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitWidthBal ?? 0), 6);
                        GoodsReplenishItem.Width_Index = Item.BinBalance.BinBalance_WidthBal_Index;
                        GoodsReplenishItem.Width_Id = Item.BinBalance.BinBalance_WidthBal_Id;
                        GoodsReplenishItem.Width_Name = Item.BinBalance.BinBalance_WidthBal_Name;
                        GoodsReplenishItem.WidthRatio = Item.BinBalance.BinBalance_WidthBalRatio;

                        GoodsReplenishItem.UnitLength = Item.BinBalance.BinBalance_UnitLengthBal;
                        GoodsReplenishItem.UnitLength_Index = Item.BinBalance.BinBalance_UnitLengthBal_Index;
                        GoodsReplenishItem.UnitLength_Id = Item.BinBalance.BinBalance_UnitLengthBal_Id;
                        GoodsReplenishItem.UnitLength_Name = Item.BinBalance.BinBalance_UnitLengthBal_Name;
                        GoodsReplenishItem.UnitLengthRatio = Item.BinBalance.BinBalance_UnitLengthBalRatio;

                        GoodsReplenishItem.Length = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitLengthBal ?? 0), 6);
                        GoodsReplenishItem.Length_Index = Item.BinBalance.BinBalance_LengthBal_Index;
                        GoodsReplenishItem.Length_Id = Item.BinBalance.BinBalance_LengthBal_Id;
                        GoodsReplenishItem.Length_Name = Item.BinBalance.BinBalance_LengthBal_Name;
                        GoodsReplenishItem.LengthRatio = Item.BinBalance.BinBalance_LengthBalRatio;

                        GoodsReplenishItem.UnitHeight = Item.BinBalance.BinBalance_UnitHeightBal;
                        GoodsReplenishItem.UnitHeight_Index = Item.BinBalance.BinBalance_UnitHeightBal_Index;
                        GoodsReplenishItem.UnitHeight_Id = Item.BinBalance.BinBalance_UnitHeightBal_Id;
                        GoodsReplenishItem.UnitHeight_Name = Item.BinBalance.BinBalance_UnitHeightBal_Name;
                        GoodsReplenishItem.UnitHeightRatio = Item.BinBalance.BinBalance_UnitHeightBalRatio;

                        GoodsReplenishItem.Height = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitHeightBal ?? 0), 6);
                        GoodsReplenishItem.Height_Index = Item.BinBalance.BinBalance_HeightBal_Index;
                        GoodsReplenishItem.Height_Id = Item.BinBalance.BinBalance_HeightBal_Id;
                        GoodsReplenishItem.Height_Name = Item.BinBalance.BinBalance_HeightBal_Name;
                        GoodsReplenishItem.HeightRatio = Item.BinBalance.BinBalance_HeightBalRatio;

                        GoodsReplenishItem.UnitPrice = Item.BinBalance.UnitPrice;
                        GoodsReplenishItem.UnitPrice_Index = Item.BinBalance.UnitPrice_Index;
                        GoodsReplenishItem.UnitPrice_Id = Item.BinBalance.UnitPrice_Id;
                        GoodsReplenishItem.UnitPrice_Name = Item.BinBalance.UnitPrice_Name;

                        GoodsReplenishItem.Price = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.UnitPrice ?? 0), 6);
                        GoodsReplenishItem.Price_Index = Item.BinBalance.Price_Index;
                        GoodsReplenishItem.Price_Id = Item.BinBalance.Price_Id;
                        GoodsReplenishItem.Price_Name = Item.BinBalance.Price_Name;

                        GoodsReplenishItem.DocumentRef_No5 = Item.BinBalance.BinBalance_QtyBal?.ToString();

                        GoodsReplenishItem.Document_Status = 0;
                        GoodsReplenishItem.Create_By = ActiveBy;
                        GoodsReplenishItem.Create_Date = ActiveDate;

                        GoodsReplenishItem.ERP_Location = Item.BinBalance.ERP_Location;
                        GoodsReplenishItem.ERP_Location_To = Item.BinBalance.ERP_Location;
                        //GoodsReplenishItem.UDF_1 = Item.BinBalance.GoodsReceive_No;

                        if (GoodsReplenishDocumentType.DocumentType_Index.ToString().ToUpper() != "9056FF09-29DF-4BBA-8FC5-6C524387F993")
                        {
                            //getGRI
                            var listGRItem = new List<DocumentViewModel> { new DocumentViewModel { documentItem_Index = Item.BinBalance.GoodsReceiveItem_Index } };
                            var GRItem = new DocumentViewModel();
                            GRItem.listDocumentViewModel = listGRItem;
                            var GoodsReceiveItem = Utils.SendDataApi<List<GoodsReceiveItemV2ViewModel>>(new AppSettingConfig().GetUrl("FindGoodsReceiveItem"), JsonConvert.SerializeObject(GRItem));
                            GoodsReplenishItem.UDF_1 = Item.BinBalance.GoodsReceive_No;
                            GoodsReplenishItem.UDF_2 = GoodsReceiveItem?.FirstOrDefault().ref_Document_No;

                            //getPGRI
                            var listPGRItem = new List<DocumentViewModel> { new DocumentViewModel { documentItem_Index = GoodsReceiveItem?.FirstOrDefault().ref_DocumentItem_Index } };
                            var PGRItem = new DocumentViewModel();
                            PGRItem.listDocumentViewModel = listPGRItem;
                            var PlanGoodsReceiveItem = Utils.SendDataApi<List<PlanGoodsReceiveItemViewModel>>(new AppSettingConfig().GetUrl("FindPlanGoodsReceiveItem"), JsonConvert.SerializeObject(PGRItem));
                            GoodsReplenishItem.UDF_3 = PlanGoodsReceiveItem?.FirstOrDefault().documentRef_No2;
                        }

                        var datacheckTag = dbBa.wm_BinBalance.Where(c => c.Location_Id == GoodsReplenishItem.Location_Id_To
                                           && c.BinBalance_QtyBal > 0
                                           && c.BinBalance_QtyReserve >= 0
                                           && c.Product_Index == GoodsReplenishItem.Product_Index).FirstOrDefault();

                        if (datacheckTag != null)
                        {
                            //GoodsReplenishItem.Tag_No_To = datacheckTag.Tag_No;
                        }

                        dbTf.Im_GoodsTransferItem.Add(GoodsReplenishItem);

                        reserveModel.Add(
                            new ReserveBinBalanceItemModel()
                            {
                                BinBalance_Index = Item.BinBalance.BinBalance_Index,
                                Ref_Document_Index = GoodsReplenishIndex,
                                Ref_DocumentItem_Index = GoodsReplenishItemIndex,
                                Process_Index = GoodsReplenishDocumentType.Process_Index.Value,
                                Ref_Document_No = GoodsReplenishNo,
                                Ref_Wave_Index = string.Empty,
                                Reserve_Qty = Item.Replenish_Qty,
                                Reserve_By = ActiveBy,
                                IsReturnBinBalanceModel = false,
                                IsReturninCardReserveModel = true
                            }
                        );
                    }

                    var transaction = dbTf.Database.BeginTransaction(IsolationLevel.Serializable);
                    try
                    {
                        dbTf.SaveChanges();
                        //Send API to Reserve
                        ReserveBinBalanceResultModel result = Utils.SendDataApi<ReserveBinBalanceResultModel>(new AppSettingConfig().GetUrl("ReserveBinBalance"), JsonConvert.SerializeObject(new ReserveBinBalanceModel() { Items = reserveModel }));
                        if ((result?.ResultIsUse ?? false) == false)
                        {
                            throw new Exception("ReserveBinBalance Exception : " + result.ResultMsg);
                        }

                        transaction.Commit();

                        var resultAssignjob = Utils.SendDataApi<string>(new AppSettingConfig().GetUrl("AssignJobTransfer"), JsonConvert.SerializeObject(modelassjob));

                        olog.logging("CreateReplenishDocumentOndemand", "SendWCSPutAwayVC GoodsReplenishNo : " + GoodsReplenishNo);

                        var modelTransferReple = new { docNo = GoodsReplenishNo };

                        var resultSendWCSPutAwayVC = Utils.SendDataApi<dynamic>(new AppSettingConfig().GetUrl("SendWCSPutAwayVC"), JsonConvert.SerializeObject(modelTransferReple));

                        GoodsReplenishDocuments.Add(GoodsReplenishNo);
                    }
                    catch (Exception exSave)
                    {
                        //TO DO Logging ?
                        olog.logging("CreateReplenishDocumentOndemand", "Error : [ " + GoodsReplenishNo + " ] : " + exSave.ToString());
                        transaction.Rollback();
                        throw exSave;
                    }
                }

                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}
