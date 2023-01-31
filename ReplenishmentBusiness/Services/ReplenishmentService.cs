using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

using DataAccess;
using DataAccess.Models.Master.Table;
using DataAccess.Models.Master.View;
using DataAccess.Models.Transfer.Table;

using Business.Commons;
using Business.Extensions;
using Business.Models;
using Business.Models.Binbalance;
using static Business.Models.SearchReplenishmentViewModel;
using ReplenishmentBusiness.ModelConfig;
using System.Data.SqlClient;
using Comone.Utils;
using DataAccess.Models.Transfer.StoredProcedure;
using System.Threading;
using BinBalanceDataAccess.Models;
using ReplenishmentBusiness.Models;
//using Comone.Utils;
//using Comone.Utils;

namespace Business.Services
{
    public static class ReplenishmentExtensions
    {
        public static DateTime TrimTime(this DateTime data)
        {
            return DateTime.Parse(data.ToShortDateString());
        }

        public static bool IsEquals(this Guid field, Guid? condition)
        {
            return condition.HasValue ? field.Equals(condition) : true;
        }

        public static bool IsEquals(this int field, int? condition)
        {
            return condition.HasValue ? field.Equals(condition) : true;
        }

        public static bool IsEquals(this bool field, bool? condition)
        {
            return condition.HasValue ? field.Equals(condition) : true;
        }

        public static bool Like(this string field, string condition)
        {
            return condition != null ? (field?.Contains(condition) ?? false) : true;
        }

        public static bool DateBetweenField(this DateTime? field_from, DateTime? field_to, DateTime? condition)
        {
            return condition.HasValue ? (field_from.HasValue ? (field_to.HasValue ? condition >= field_from && condition <= field_to : field_from.Equals(condition)) : false) : true;
        }

        public static bool DateBetweenCondition(this DateTime field, DateTime? condition_from, DateTime? condition_to, bool trimTime = false)
        {
            return condition_from.HasValue ? (condition_to.HasValue ? condition_from <= (trimTime ? field.TrimTime() : field) && condition_to >= (trimTime ? field.TrimTime() : field) : (trimTime ? field.TrimTime() : field).Equals(condition_from)) : true;
        }

        public static bool DateBetweenCondition(this DateTime? field, DateTime? condition_from, DateTime? condition_to)
        {
            return condition_from.HasValue ? (field.HasValue ? (condition_to.HasValue ? condition_from <= field && condition_to >= field : field.Equals(condition_from)) : false) : true;
        }
    }

    public class ReplenishmentService
    {



        private MasterDbContext db;
        private TransferDbContext dbTf;
        private BinbalanceDbContext dbBa;


        public ReplenishmentService()
        {
            db = new MasterDbContext();
            dbTf = new TransferDbContext();
            dbBa = new BinbalanceDbContext();

        }

        public ReplenishmentService(MasterDbContext db, TransferDbContext dbTf, BinbalanceDbContext dbBa)
        {
            this.db = db;
            this.dbTf = dbTf;
            this.dbBa = dbBa;

        }

        private ReplenishmentViewModel GetReplenishmentViewModel(string jsonData, bool requiredPrimaryKey = false)
        {
            if (string.IsNullOrEmpty(jsonData?.Trim() ?? string.Empty))
            {
                throw new Exception("Invalid JSon : Null");
            }

            ReplenishmentViewModel model = JsonConvert.DeserializeObject<ReplenishmentViewModel>(jsonData);
            if (model == null)
            {
                throw new Exception("Invalid JSon : Cannot convert to Model");
            }

            if (requiredPrimaryKey)
            {
                if (!model.Replenishment_Index.HasValue)
                {
                    throw new Exception("Invalid JSon : Required Primary Key Model");
                }
            }

            return model;
        }

        private SearchReplenishmentViewModel GetSearchReplenishmentViewModel(string jsonData, bool requiredPrimaryKey = false)
        {
            SearchReplenishmentViewModel model = JsonConvert.DeserializeObject<SearchReplenishmentViewModel>(jsonData ?? string.Empty);
            if (model is null)
            {
                model = new SearchReplenishmentViewModel();
            }

            if (requiredPrimaryKey)
            {
                if (!model.Replenishment_Index.HasValue)
                {
                    throw new Exception("Invalid JSon : Required Primary Key Model");
                }
            }

            return model;
        }

        #region + List Config Replenishment +
        public ListReplenishmentViewModel ListConfigReplenishment(string jsonData)
        {
            try
            {
                SearchReplenishmentViewModel data = GetSearchReplenishmentViewModel(jsonData);


                IQueryable<Ms_Replenishment> query = db.Ms_Replenishment.AsQueryable();
                query = query.Where(
                        w => w.IsActive.IsEquals(data.IsActive) &&
                             w.Replenishment_Id.Like(data.Replenishment_Id) &&
                             w.Trigger_Date.DateBetweenCondition(data.Trigger_Date, data.Trigger_Date_End) &&
                             w.IsMonday.IsEquals(data.IsMonday) &&
                             w.IsTuesday.IsEquals(data.IsTuesday) &&
                             w.IsWednesday.IsEquals(data.IsWednesday) &&
                             w.IsThursday.IsEquals(data.IsThursday) &&
                             w.IsFriday.IsEquals(data.IsFriday) &&
                             w.IsSaturday.IsEquals(data.IsSaturday) &&
                             w.IsSunday.IsEquals(data.IsSunday) &&
                             w.Create_By.Like(data.Create_By) &&
                             w.Create_Date.DateBetweenCondition(data.Create_Date, data.Create_Date_End, true)
                    );

                if (data.Status.Count == 1)
                {
                    if (data.Status.FirstOrDefault().Value == 0)
                        query = query.Where(c => c.IsActive == 0);
                    else if (data.Status.FirstOrDefault().Value == 1)
                        query = query.Where(c => c.IsActive == 1);
                }

                var sortModels = new List<SortModel>();

                if (data.sort.Count > 0)
                {
                    foreach (var item in data.sort)
                    {

                        if (item.Value == "Replenishment_Id")
                        {
                            sortModels.Add(new SortModel
                            {
                                ColId = "Replenishment_Id",
                                Sort = "desc"
                            });
                        }
                        if (item.Value == "Create_Date")
                        {
                            sortModels.Add(new SortModel
                            {
                                ColId = "Create_Date",
                                Sort = "desc"
                            });
                        }
                        if (item.Value == "Create_Date")
                        {
                            sortModels.Add(new SortModel
                            {
                                ColId = "Create_Date",
                                Sort = "desc"
                            });
                        }


                    }
                    query = query.KWOrderBy(sortModels);
                }

                List<Ms_Replenishment> replenishments = query.ToList();

                ListReplenishmentViewModel result = new ListReplenishmentViewModel()
                {
                    ReplenishmentViewModels = JsonConvert.DeserializeObject<List<ReplenishmentViewModel>>(JsonConvert.SerializeObject(replenishments)),
                    Pagination = new Pagination() { TotalRow = replenishments.Count, CurrentPage = data.CurrentPage, PerPage = data.PerPage }
                };
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region + Get Config Replenishment +
        public ReplenishmentViewModel GetConfigReplenishment(string jsonData)
        {
            try
            {
                SearchReplenishmentViewModel data = GetSearchReplenishmentViewModel(jsonData, true);
                Ms_Replenishment Replenishments = db.Ms_Replenishment.Find(data.Replenishment_Index);

                if (Replenishments == null)
                {
                    throw new Exception("Data not found");
                }

                List<View_Replenishment_Product> ReplenishmentProducts = db.View_Replenishment_Product.Where(s => s.Replenishment_Index.Equals(data.Replenishment_Index)).ToList();
                List<View_Replenishment_Location> ReplenishmentLocations = db.View_Replenishment_Location.Where(s => s.Replenishment_Index.Equals(data.Replenishment_Index)).ToList();

                ReplenishmentViewModel result = JsonConvert.DeserializeObject<ReplenishmentViewModel>(JsonConvert.SerializeObject(Replenishments));
                result.ReplenishmentProducts = JsonConvert.DeserializeObject<List<ReplenishmentProductViewModel>>(JsonConvert.SerializeObject(ReplenishmentProducts));
                result.ReplenishmentLocations = JsonConvert.DeserializeObject<List<ReplenishmentLocationViewModel>>(JsonConvert.SerializeObject(ReplenishmentLocations));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region + Save Config Replenishment +
        public string SaveConfigReplenishment(string jsonData)
        {
            try
            {
                ReplenishmentViewModel data = GetReplenishmentViewModel(jsonData);

                Ms_Replenishment model = db.Ms_Replenishment.Find(data.Replenishment_Index);
                bool bAddProduct = data.Plan_By_Product == 1 && (data.ReplenishmentProducts?.Count ?? 0) > 0;
                bool bAddLocation = data.Plan_By_Location == 1 && (data.ReplenishmentLocations?.Count ?? 0) > 0;

                Guid replenishmentIndex;
                string UserBy;
                DateTime UserDate = DateTime.Now;

                if (model is null)
                {
                    replenishmentIndex = Guid.NewGuid();
                    UserBy = data.Create_By;
                    data.Replenishment_Id = "Replenishment_Id".GenAutonumber();

                    Ms_Replenishment NewReplenishment = new Ms_Replenishment
                    {
                        Replenishment_Index = replenishmentIndex,
                        Replenishment_Id = data.Replenishment_Id,
                        Replenishment_Remark = data.Replenishment_Remark,
                        Trigger_Time = data.Trigger_Time,
                        Trigger_Time_End = data.Trigger_Time_End,
                        Trigger_Date = data.Trigger_Date,
                        Trigger_Date_End = data.Trigger_Date_End,
                        IsMonday = data.IsMonday,
                        IsTuesday = data.IsTuesday,
                        IsWednesday = data.IsWednesday,
                        IsThursday = data.IsThursday,
                        IsFriday = data.IsFriday,
                        IsSaturday = data.IsSaturday,
                        IsSunday = data.IsSunday,
                        Plan_By_Product = data.Plan_By_Product,
                        Plan_By_Location = data.Plan_By_Location,
                        Plan_By_Status = data.Plan_By_Status,
                        IsActive = data.IsActive,
                        IsDelete = 0,
                        IsSystem = 0,
                        Status_Id = 0,
                        Create_By = UserBy,
                        Create_Date = UserDate
                    };

                    db.Ms_Replenishment.Add(NewReplenishment);
                }
                else
                {
                    replenishmentIndex = data.Replenishment_Index.Value;
                    UserBy = data.Update_By;

                    model.Replenishment_Remark = data.Replenishment_Remark;
                    model.Trigger_Time = data.Trigger_Time;
                    model.Trigger_Time_End = data.Trigger_Time_End;
                    model.Trigger_Date = data.Trigger_Date;
                    model.Trigger_Date_End = data.Trigger_Date_End;
                    model.IsMonday = data.IsMonday;
                    model.IsTuesday = data.IsTuesday;
                    model.IsWednesday = data.IsWednesday;
                    model.IsThursday = data.IsThursday;
                    model.IsFriday = data.IsFriday;
                    model.IsSaturday = data.IsSaturday;
                    model.IsSunday = data.IsSunday;
                    model.Plan_By_Product = data.Plan_By_Product;
                    model.Plan_By_Location = data.Plan_By_Location;
                    model.Plan_By_Status = data.Plan_By_Status;
                    model.IsActive = data.IsActive;
                    model.Update_By = UserBy;
                    model.Update_Date = UserDate;

                    List<Ms_Replenishment_Product> ReplenishmentProducts = db.Ms_Replenishment_Product.Where(s => s.Replenishment_Index == data.Replenishment_Index).ToList();
                    List<Ms_Replenishment_Location> ReplenishmentLocations = db.Ms_Replenishment_Location.Where(s => s.Replenishment_Index == data.Replenishment_Index).ToList();

                    if (bAddProduct)
                    {
                        db.Ms_Replenishment_Product.RemoveRange(ReplenishmentProducts);
                    }

                    if (bAddLocation)
                    {
                        db.Ms_Replenishment_Location.RemoveRange(ReplenishmentLocations);
                    }
                }

                if (bAddProduct)
                {
                    List<Ms_Replenishment_Product> NewProducts = new List<Ms_Replenishment_Product>();

                    data.ReplenishmentProducts.ForEach(e =>
                    {
                        NewProducts.Add(new Ms_Replenishment_Product
                        {
                            Replenishment_Product_Index = Guid.NewGuid(),
                            Replenishment_Index = replenishmentIndex,
                            ProductType_Index = e.ProductType_Index,
                            Product_Index = e.Product_Index
                        });
                    });

                    db.Ms_Replenishment_Product.AddRange(NewProducts);
                }

                if (bAddLocation)
                {
                    List<Ms_Replenishment_Location> NewLocations = new List<Ms_Replenishment_Location>();

                    data.ReplenishmentLocations.ForEach(e =>
                    {
                        NewLocations.Add(new Ms_Replenishment_Location
                        {
                            Replenishment_Location_Index = Guid.NewGuid(),
                            Replenishment_Index = replenishmentIndex,
                            Zone_Index = e.Zone_Index,
                            Location_Index = e.Location_Index
                        });
                    });

                    db.Ms_Replenishment_Location.AddRange(NewLocations);
                }

                var MyTransaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    db.SaveChanges();
                    MyTransaction.Commit();
                }
                catch (Exception saveEx)
                {
                    MyTransaction.Rollback();
                    throw saveEx;
                }

                return data.Replenishment_Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region + Delete Config Replenishment +
        public bool DeleteConfigReplenishment(string jsonData)
        {
            try
            {
                ReplenishmentViewModel data = GetReplenishmentViewModel(jsonData, true);

                //Ms_Replenishment Replenishment = db.Ms_Replenishment.Find(data.Replenishment_Index);
                Ms_Replenishment Replenishment = db.Ms_Replenishment.Where(c => c.Replenishment_Index == data.Replenishment_Index).FirstOrDefault();
                if (Replenishment == null)
                {
                    throw new Exception("Data not found");
                }

                List<Ms_Replenishment_Product> ReplenishmentProducts = db.Ms_Replenishment_Product.Where(s => s.Replenishment_Index == data.Replenishment_Index).ToList();
                List<Ms_Replenishment_Location> ReplenishmentLocations = db.Ms_Replenishment_Location.Where(s => s.Replenishment_Index == data.Replenishment_Index).ToList();

                View_Replenishment _ViewReplenishment = db.View_Replenishment.Find(data.Replenishment_Index);
                List<Ms_ProductLocation> ProductLocation = db.Ms_ProductLocation.Where(s => s.Product_Index == _ViewReplenishment.Product_Index && s.Location_Index == _ViewReplenishment.Location_Index).ToList();

                if ((ReplenishmentProducts?.Count ?? 0) > 0)
                {
                    db.Ms_Replenishment_Product.RemoveRange(ReplenishmentProducts);
                }

                if ((ReplenishmentLocations?.Count ?? 0) > 0)
                {
                    db.Ms_Replenishment_Location.RemoveRange(ReplenishmentLocations);
                }

                if ((ProductLocation?.Count ?? 0) > 0)
                {
                    db.Ms_ProductLocation.RemoveRange(ProductLocation);
                }

                db.Ms_Replenishment.Remove(Replenishment);

                var MyTransaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    db.SaveChanges();
                    MyTransaction.Commit();
                }
                catch (Exception saveEx)
                {
                    MyTransaction.Rollback();
                    throw saveEx;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region + Activate Replenishment +
        public List<string> ActivateReplenishment()
        {
            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                //Find Task Replenish
                DateTime currentDate = DateTime.Today;
                TimeSpan currentTime = new TimeSpan(0, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                int dayOfWeek = (int)currentDate.DayOfWeek;
                List<Ms_Replenishment> Replenishments = db.Ms_Replenishment.Where(
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
                    //no task found
                    throw new Exception("Task Replenish not found");
                }

                //TO DO Config Index
                //Prepare BinBalance Model
                Guid GoodsReplenishDocumentTypeIndex = Guid.Parse("9056FF09-29DF-4BBA-8FC5-6C524387F995");
                Ms_DocumentType goodsReplenishDocumentType = db.Ms_DocumentType.Find(GoodsReplenishDocumentTypeIndex);
                if (goodsReplenishDocumentType is null)
                {
                    throw new Exception("Replenish DocumentType not found");
                }

                // Fix TOP 100
                Guid StorageLocationTypeIndex = Guid.Parse("F9EDDAEC-A893-4F63-A700-526C69CC08C0");
                List<Guid> ReplenishLocationIndexs =
                    JsonConvert.DeserializeObject<List<Guid>>(
                    JsonConvert.SerializeObject(
                        db.Ms_Location.Where(s => s.IsActive == 1 && s.LocationType_Index == StorageLocationTypeIndex).Select(s => s.Location_Index)));
                if ((ReplenishLocationIndexs?.Count ?? 0) == 0)
                {
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
                foreach (Ms_Replenishment replenishment in Replenishments)
                {
                    try
                    {
                        binBalances = GetBinBalanceReplenish(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
                        if ((binBalances?.Count ?? 0) > 0)
                        {
                            GoodsReplenishDocuments.AddRange(
                                CreateReplenishDocument(replenishment.Replenishment_Index, goodsReplenishDocumentType, binBalances)
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg.Add(ex.Message);
                        continue;
                    }
                }

                if (errorMsg.Count > 0)
                {
                    throw new Exception("Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                }

                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
            }
        }

        public List<string> ActivateReplenishmentASRS()
        {

            String State = "Start";
            String msglog = "";
            var olog = new logtxt();

            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                olog.logging("ActivateReplenishmentASRS", State);

                //Find Task Replenish
                DateTime currentDate = DateTime.Today;
                TimeSpan currentTime = new TimeSpan(0, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                int dayOfWeek = (int)currentDate.DayOfWeek;
                List<Ms_Replenishment> Replenishments = db.Ms_Replenishment.Where(
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
                    olog.logging("ActivateReplenishmentASRS", "Task Replenish not found");
                    throw new Exception("Task Replenish not found");
                }

                //TO DO Config Index
                //Prepare BinBalance Model
                Guid GoodsReplenishDocumentTypeIndex = Guid.Parse("D61AB6E6-FFB7-47B9-A2D3-CD4AF77E98C5"); // Auto Replenishment ASRS
                Ms_DocumentType goodsReplenishDocumentType = db.Ms_DocumentType.Find(GoodsReplenishDocumentTypeIndex);
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
                        db.Ms_Location.Where(s => s.IsActive == 1 && s.LocationType_Index == StorageLocationTypeIndex).Select(s => s.Location_Index)));
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
                foreach (Ms_Replenishment replenishment in Replenishments)
                {
                    try
                    {

                        binBalances = new List<ReplenishmentBalanceModel>();
                        binBalance_API_Model = new SearchReplenishmentBalanceModel()
                        {
                            ReplenishLocationIndexs = ReplenishLocationIndexs,
                            ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                        };

                        olog.logging("ActivateReplenishmentASRS", " GetBinBalanceReplenish Replenishment_Id :  " + replenishment.Replenishment_Id);


                        binBalances = GetBinBalanceReplenish(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
                        if ((binBalances?.Count ?? 0) > 0)
                        {
                            olog.logging("ActivateReplenishmentASRS", " CreateReplenishDocument Replenishment_Id :  " + replenishment.Replenishment_Id);


                            GoodsReplenishDocuments.AddRange(
                                CreateReplenishDocument(replenishment.Replenishment_Index, goodsReplenishDocumentType, binBalances)
                            );
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

                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                olog.logging("ActivateReplenishmentASRS", ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
                throw new Exception(ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
            }
        }

        public List<string> ActivateReplenishmentPiecePick()
        {
            String State = "Start";
            String msglog = "";
            var olog = new logtxt();

            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                olog.logging("ActivateReplenishmentPiecePick", State);

                var roweffect = db.Database.ExecuteSqlCommand("EXEC sp_UpdateReplenishActive");

                System.Threading.Thread.Sleep(10000);

                //Find Task Replenish
                DateTime currentDate = DateTime.Today;
                TimeSpan currentTime = new TimeSpan(0, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                int dayOfWeek = (int)currentDate.DayOfWeek;
                List<Ms_Replenishment> Replenishments = db.Ms_Replenishment.Where(
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
                    olog.logging("ActivateReplenishmentPiecePick", "Task Replenish not found");
                    throw new Exception("Task Replenish not found");
                }

                //TO DO Config Index
                //Prepare BinBalance Model
                Guid GoodsReplenishDocumentTypeIndex = Guid.Parse("47BF1845-33D1-47FE-B38D-7BDDF0E48A7E"); // Auto Replenishment PP
                Ms_DocumentType goodsReplenishDocumentType = db.Ms_DocumentType.Find(GoodsReplenishDocumentTypeIndex);
                if (goodsReplenishDocumentType is null)
                {
                    olog.logging("ActivateReplenishmentPiecePick", "Replenish DocumentType not found");
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
                        db.Ms_Location.Where(s => s.IsActive == 1 && (s.LocationType_Index == PiecepickLocationTypeIndex
                            || s.LocationType_Index == SelectiveLocationTypeIndex
                            || s.LocationType_Index == SelectiveOnGroundLocationTypeIndex
                            || s.LocationType_Index == DummyLocationTypeIndex)
                        && !s.Location_Id.Contains("BUF")
                        ).Select(s => s.Location_Index)));
                if ((ReplenishLocationIndexs?.Count ?? 0) == 0)
                {
                    olog.logging("ActivateReplenishmentPiecePick", "Replenish Location not found");
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
                foreach (Ms_Replenishment replenishment in Replenishments)
                {
                    try
                    {

                        binBalances = new List<ReplenishmentBalanceModel>();
                        binBalance_API_Model = new SearchReplenishmentBalanceModel()
                        {
                            ReplenishLocationIndexs = ReplenishLocationIndexs,
                            ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
                        };

                        olog.logging("ActivateReplenishmentPiecePick", " GetBinBalanceReplenish Replenishment_Id :  " + replenishment.Replenishment_Id);


                        binBalances = GetBinBalanceReplenishVC(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
                        if ((binBalances?.Count ?? 0) > 0)
                        {
                            olog.logging("ActivateReplenishmentPiecePick", " CreateReplenishDocument Replenishment_Id :  " + replenishment.Replenishment_Id);


                            GoodsReplenishDocuments.AddRange(
                                CreateReplenishDocument(replenishment.Replenishment_Index, goodsReplenishDocumentType, binBalances)
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        olog.logging("ActivateReplenishmentPiecePick", "GetBinBalanceReplenish " + ex.Message);

                        errorMsg.Add(ex.Message);
                        continue;
                    }
                }

                if (errorMsg.Count > 0)
                {
                    olog.logging("ActivateReplenishmentPiecePick", "Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                    throw new Exception("Receieved Error : " + string.Join(",", errorMsg.Distinct()));
                }

                return GoodsReplenishDocuments;
            }
            catch (Exception ex)
            {
                olog.logging("ActivateReplenishmentPiecePick", ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
                throw new Exception(ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
            }
        }

        private List<ReplenishmentBalanceModel> GetBinBalanceReplenish(Ms_DocumentType documentType, Ms_Replenishment replenishment, SearchReplenishmentBalanceModel binBalance_API_Model)
        {
            try
            {
                IQueryable<View_ProductLocation> queryProductLocations = db.View_ProductLocation.AsQueryable().Where(w => w.IsActive == 1 && w.IsDelete == 0);

                bool planByProduct = replenishment.Plan_By_Product == 1;
                bool planByLocation = replenishment.Plan_By_Location == 1;

                if (planByProduct)
                {
                    List<Ms_Replenishment_Product> replenishProducts = db.Ms_Replenishment_Product.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishProducts?.Count ?? 0) > 0)
                    {
                        List<Guid?> listProductIndex = replenishProducts.Where(w => w.Product_Index.HasValue).Select(s => s.Product_Index).Distinct().ToList();
                        List<Guid?> listProductTypeIndex = replenishProducts.Where(w => !w.Product_Index.HasValue).Select(s => (Guid?)s.ProductType_Index).Distinct().ToList();

                        List<Ms_Product> products = new List<Ms_Product>();
                        if ((listProductTypeIndex?.Count ?? 0) > 0)
                        {
                            products = db.Ms_Product.Where(
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
                    List<Ms_Replenishment_Location> replenishLocations = db.Ms_Replenishment_Location.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishLocations?.Count ?? 0) > 0)
                    {
                        List<Guid?> listLocationIndex = replenishLocations.Where(w => w.Location_Index.HasValue).Select(s => s.Location_Index).Distinct().ToList();
                        List<Guid?> listZoneIndex = replenishLocations.Where(w => w.Zone_Index.HasValue).Select(s => s.Zone_Index).Distinct().ToList();

                        List<Ms_ZoneLocation> zoneLocations = new List<Ms_ZoneLocation>();
                        if ((listZoneIndex?.Count ?? 0) > 0)
                        {
                            zoneLocations = db.Ms_ZoneLocation.Where(
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
                        Minimum_Qty = e.Qty,
                        Replenish_Qty = 0,
                        Pending_Replenish_Qty = GetPendingReplenishQty(binBalance_API_Model.Owner_Index, documentType.DocumentType_Index, e.Product_Index, e.Location_Index),

                    }
                ));

                binBalance_API_Model.Items.RemoveAll(r => r.Pending_Replenish_Qty >= r.Minimum_Qty);

                if (binBalance_API_Model.Items.Count == 0) { throw new Exception("Already Pending Replenish"); }

                List<ReplenishmentBalanceModel> BinBalances = new List<ReplenishmentBalanceModel>();
                //Send API BinBalance
                if (checkLocation == true)
                {
                    // VC01
                    //  BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalanceVC"), JsonConvert.SerializeObject(binBalance_API_Model));
                    //  BinBalances = notthin
                }
                else
                {
                    // ASRS
                    BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));
                    //BinBalances = SearchReplenishmentBinBalanceASRS(JsonConvert.SerializeObject(binBalance_API_Model));
                }

                //  List<ReplenishmentBalanceModel> BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));

                return BinBalances;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ReplenishmentBalanceModel> GetBinBalanceReplenishVC(Ms_DocumentType documentType, Ms_Replenishment replenishment, SearchReplenishmentBalanceModel binBalance_API_Model)
        {
            try
            {
                IQueryable<View_ProductLocation> queryProductLocations = db.View_ProductLocation.AsQueryable().Where(w => w.IsActive == 1 && w.IsDelete == 0);

                bool planByProduct = replenishment.Plan_By_Product == 1;
                bool planByLocation = replenishment.Plan_By_Location == 1;

                if (planByProduct)
                {
                    List<Ms_Replenishment_Product> replenishProducts = db.Ms_Replenishment_Product.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishProducts?.Count ?? 0) > 0)
                    {
                        List<Guid?> listProductIndex = replenishProducts.Where(w => w.Product_Index.HasValue).Select(s => s.Product_Index).Distinct().ToList();
                        List<Guid?> listProductTypeIndex = replenishProducts.Where(w => !w.Product_Index.HasValue).Select(s => (Guid?)s.ProductType_Index).Distinct().ToList();

                        List<Ms_Product> products = new List<Ms_Product>();
                        if ((listProductTypeIndex?.Count ?? 0) > 0)
                        {
                            products = db.Ms_Product.Where(
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
                    List<Ms_Replenishment_Location> replenishLocations = db.Ms_Replenishment_Location.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishLocations?.Count ?? 0) > 0)
                    {
                        List<Guid?> listLocationIndex = replenishLocations.Where(w => w.Location_Index.HasValue).Select(s => s.Location_Index).Distinct().ToList();
                        List<Guid?> listZoneIndex = replenishLocations.Where(w => w.Zone_Index.HasValue).Select(s => s.Zone_Index).Distinct().ToList();

                        List<Ms_ZoneLocation> zoneLocations = new List<Ms_ZoneLocation>();
                        if ((listZoneIndex?.Count ?? 0) > 0)
                        {
                            zoneLocations = db.Ms_ZoneLocation.Where(
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
                        Minimum_Qty = e.Qty,
                        Replenish_Qty = e.Replenish_Qty,
                        Pending_Replenish_Qty = GetPendingReplenishQty(binBalance_API_Model.Owner_Index, documentType.DocumentType_Index, e.Product_Index, e.Location_Index),

                    }
                ));

                binBalance_API_Model.Items.RemoveAll(r => r.Pending_Replenish_Qty >= r.Minimum_Qty);

                if (binBalance_API_Model.Items.Count == 0) { throw new Exception("Already Pending Replenish"); }

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
                    // ASRS
                    //      BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));

                }

                //  List<ReplenishmentBalanceModel> BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalance"), JsonConvert.SerializeObject(binBalance_API_Model));

                return BinBalances;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<ReplenishmentBalanceModel> GetBinBalanceReplenishASRS(Ms_DocumentType documentType, Ms_Replenishment replenishment, SearchReplenishmentBalanceModel binBalance_API_Model)
        {
            try
            {
                IQueryable<View_ProductLocation> queryProductLocations = db.View_ProductLocation.AsQueryable().Where(w => w.IsActive == 1 && w.IsDelete == 0);

                bool planByProduct = replenishment.Plan_By_Product == 1;
                bool planByLocation = replenishment.Plan_By_Location == 1;

                if (planByProduct)
                {
                    List<Ms_Replenishment_Product> replenishProducts = db.Ms_Replenishment_Product.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishProducts?.Count ?? 0) > 0)
                    {
                        List<Guid?> listProductIndex = replenishProducts.Where(w => w.Product_Index.HasValue).Select(s => s.Product_Index).Distinct().ToList();
                        List<Guid?> listProductTypeIndex = replenishProducts.Where(w => !w.Product_Index.HasValue).Select(s => (Guid?)s.ProductType_Index).Distinct().ToList();

                        List<Ms_Product> products = new List<Ms_Product>();
                        if ((listProductTypeIndex?.Count ?? 0) > 0)
                        {
                            products = db.Ms_Product.Where(
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
                    List<Ms_Replenishment_Location> replenishLocations = db.Ms_Replenishment_Location.Where(s => s.Replenishment_Index.Equals(replenishment.Replenishment_Index)).ToList();
                    if ((replenishLocations?.Count ?? 0) > 0)
                    {
                        List<Guid?> listLocationIndex = replenishLocations.Where(w => w.Location_Index.HasValue).Select(s => s.Location_Index).Distinct().ToList();
                        List<Guid?> listZoneIndex = replenishLocations.Where(w => w.Zone_Index.HasValue).Select(s => s.Zone_Index).Distinct().ToList();

                        List<Ms_ZoneLocation> zoneLocations = new List<Ms_ZoneLocation>();
                        if ((listZoneIndex?.Count ?? 0) > 0)
                        {
                            zoneLocations = db.Ms_ZoneLocation.Where(
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
                        Minimum_Qty = e.Qty,
                        Replenish_Qty = 0,
                        Pending_Replenish_Qty = GetPendingReplenishQty(binBalance_API_Model.Owner_Index, documentType.DocumentType_Index, e.Product_Index, e.Location_Index),

                    }
                ));

                binBalance_API_Model.Items.RemoveAll(r => r.Pending_Replenish_Qty >= r.Minimum_Qty);

                if (binBalance_API_Model.Items.Count == 0) { throw new Exception("Already Pending Replenish"); }

                List<ReplenishmentBalanceModel> BinBalances = new List<ReplenishmentBalanceModel>();
                //Send API BinBalance
                if (checkLocation == true)
                {
                    // VC01
                    //  BinBalances = Utils.SendDataApi<List<ReplenishmentBalanceModel>>(new AppSettingConfig().GetUrl("SearchBinBalanceVC"), JsonConvert.SerializeObject(binBalance_API_Model));
                    //  BinBalances = notthin
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

        private List<string> CreateReplenishDocument(Guid ReplenishIndex, Ms_DocumentType GoodsReplenishDocumentType, List<ReplenishmentBalanceModel> BinBalances)
        {
            List<string> GoodsReplenishDocuments = new List<string>();
            try
            {
                dbBa.Database.SetCommandTimeout(120);
                var olog = new logtxt();

                List<ReplenishmentBalanceModel> OwnerBinBalances;

                Guid GoodsReplenishIndex, GoodsReplenishItemIndex;
                string GoodsReplenishNo;

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
                        //GoodsReplenishItem = new Im_GoodsTransferItem()
                        //{
                        //    GoodsTransferItem_Index = GoodsReplenishItemIndex,
                        //    GoodsTransfer_Index = GoodsReplenishIndex,
                        //    GoodsReceiveItem_Index = Item.BinBalance.GoodsReceiveItem_Index,
                        //    GoodsReceive_Index = Item.BinBalance.GoodsReceive_Index,
                        //    GoodsReceiveItemLocation_Index = Item.BinBalance.GoodsReceiveItemLocation_Index,
                        //    LineNum = (OwnerBinBalances.IndexOf(Item) + 1).ToString(),

                        //    TagItem_Index = Item.BinBalance.TagItem_Index,
                        //    Tag_Index = Item.BinBalance.Tag_Index,
                        //    Tag_No = Item.BinBalance.Tag_No,
                        //    Owner_Index = Item.BinBalance.Owner_Index,
                        //    Owner_Id = Item.BinBalance.Owner_Id,
                        //    Owner_Name = Item.BinBalance.Owner_Name,
                        //    GoodsReceive_EXP_Date = Item.BinBalance.GoodsReceive_EXP_Date,
                        //    Product_Index = Item.BinBalance.Product_Index,
                        //    Product_Id = Item.BinBalance.Product_Id,
                        //    Product_Name = Item.BinBalance.Product_Name,
                        //    Product_SecondName = Item.BinBalance.Product_SecondName,
                        //    Product_ThirdName = Item.BinBalance.Product_ThirdName,
                        //    Product_Lot = Item.BinBalance.Product_Lot,
                        //    ItemStatus_Index = Item.BinBalance.ItemStatus_Index,
                        //    ItemStatus_Id = Item.BinBalance.ItemStatus_Id,
                        //    ItemStatus_Name = Item.BinBalance.ItemStatus_Name,
                        //    ItemStatus_Index_To = Item.BinBalance.ItemStatus_Index,
                        //    ItemStatus_Id_To = Item.BinBalance.ItemStatus_Id,
                        //    ItemStatus_Name_To = Item.BinBalance.ItemStatus_Name,
                        //    Location_Index = Item.BinBalance.Location_Index,
                        //    Location_Id = Item.BinBalance.Location_Id,
                        //    Location_Name = Item.BinBalance.Location_Name,
                        //    Location_Index_To = Item.Location_Index,
                        //    Location_Id_To = Item.Location_Id,
                        //    Location_Name_To = Item.Location_Name,
                        //    Qty = decimal.Round(Item.Replenish_Qty / (Item.BinBalance.BinBalance_Ratio ?? 1), 6),
                        //    Ratio = Item.BinBalance.BinBalance_Ratio,
                        //    TotalQty = Item.Replenish_Qty,
                        //    ProductConversion_Index = Item.BinBalance.ProductConversion_Index,
                        //    ProductConversion_Id = Item.BinBalance.ProductConversion_Id,
                        //    ProductConversion_Name = Item.BinBalance.ProductConversion_Name,

                        //    UnitVolume = Item.BinBalance.BinBalance_UnitVolumeBal,
                        //    Volume = decimal.Round((Item.Replenish_Qty / Item.BinBalance.BinBalance_QtyBal ?? 1) * (Item.BinBalance.BinBalance_VolumeBal ?? 0), 6),

                        //    UnitGrsWeight = Item.BinBalance.BinBalance_UnitGrsWeightBal,
                        //    UnitGrsWeight_Index = Item.BinBalance.BinBalance_UnitGrsWeightBal_Index,
                        //    UnitGrsWeight_Id = Item.BinBalance.BinBalance_UnitGrsWeightBal_Id,
                        //    UnitGrsWeight_Name = Item.BinBalance.BinBalance_UnitGrsWeightBal_Name,
                        //    UnitGrsWeightRatio = Item.BinBalance.BinBalance_UnitGrsWeightBalRatio,

                        //    GrsWeight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6),
                        //    GrsWeight_Index = Item.BinBalance.BinBalance_GrsWeightBal_Index,
                        //    GrsWeight_Id = Item.BinBalance.BinBalance_GrsWeightBal_Id,
                        //    GrsWeight_Name = Item.BinBalance.BinBalance_GrsWeightBal_Name,
                        //    GrsWeightRatio = Item.BinBalance.BinBalance_GrsWeightBalRatio,

                        //    UnitNetWeight = Item.BinBalance.BinBalance_UnitNetWeightBal,
                        //    UnitNetWeight_Index = Item.BinBalance.BinBalance_UnitNetWeightBal_Index,
                        //    UnitNetWeight_Id = Item.BinBalance.BinBalance_UnitNetWeightBal_Id,
                        //    UnitNetWeight_Name = Item.BinBalance.BinBalance_UnitNetWeightBal_Name,
                        //    UnitNetWeightRatio = Item.BinBalance.BinBalance_UnitNetWeightBalRatio,

                        //    NetWeight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitNetWeightBal ?? 0), 6),
                        //    NetWeight_Index = Item.BinBalance.BinBalance_NetWeightBal_Index,
                        //    NetWeight_Id = Item.BinBalance.BinBalance_NetWeightBal_Id,
                        //    NetWeight_Name = Item.BinBalance.BinBalance_NetWeightBal_Name,
                        //    NetWeightRatio = Item.BinBalance.BinBalance_NetWeightBalRatio,

                        //    UnitWeight = Item.BinBalance.BinBalance_UnitWeightBal,
                        //    UnitWeight_Index = Item.BinBalance.BinBalance_UnitWeightBal_Index,
                        //    UnitWeight_Id = Item.BinBalance.BinBalance_UnitWeightBal_Id,
                        //    UnitWeight_Name = Item.BinBalance.BinBalance_UnitWeightBal_Name,
                        //    UnitWeightRatio = Item.BinBalance.BinBalance_UnitWeightBalRatio,

                        //    Weight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6),
                        //    Weight_Index = Item.BinBalance.BinBalance_WeightBal_Index,
                        //    Weight_Id = Item.BinBalance.BinBalance_WeightBal_Id,
                        //    Weight_Name = Item.BinBalance.BinBalance_WeightBal_Name,
                        //    WeightRatio = Item.BinBalance.BinBalance_WeightBalRatio,

                        //    UnitWidth = Item.BinBalance.BinBalance_UnitWidthBal,
                        //    UnitWidth_Index = Item.BinBalance.BinBalance_UnitWidthBal_Index,
                        //    UnitWidth_Id = Item.BinBalance.BinBalance_UnitWidthBal_Id,
                        //    UnitWidth_Name = Item.BinBalance.BinBalance_UnitWidthBal_Name,
                        //    UnitWidthRatio = Item.BinBalance.BinBalance_UnitWidthBalRatio,

                        //    Width = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitWidthBal ?? 0), 6),
                        //    Width_Index = Item.BinBalance.BinBalance_WidthBal_Index,
                        //    Width_Id = Item.BinBalance.BinBalance_WidthBal_Id,
                        //    Width_Name = Item.BinBalance.BinBalance_WidthBal_Name,
                        //    WidthRatio = Item.BinBalance.BinBalance_WidthBalRatio,

                        //    UnitLength = Item.BinBalance.BinBalance_UnitLengthBal,
                        //    UnitLength_Index = Item.BinBalance.BinBalance_UnitLengthBal_Index,
                        //    UnitLength_Id = Item.BinBalance.BinBalance_UnitLengthBal_Id,
                        //    UnitLength_Name = Item.BinBalance.BinBalance_UnitLengthBal_Name,
                        //    UnitLengthRatio = Item.BinBalance.BinBalance_UnitLengthBalRatio,

                        //    Length = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitLengthBal ?? 0), 6),
                        //    Length_Index = Item.BinBalance.BinBalance_LengthBal_Index,
                        //    Length_Id = Item.BinBalance.BinBalance_LengthBal_Id,
                        //    Length_Name = Item.BinBalance.BinBalance_LengthBal_Name,
                        //    LengthRatio = Item.BinBalance.BinBalance_LengthBalRatio,

                        //    UnitHeight = Item.BinBalance.BinBalance_UnitHeightBal,
                        //    UnitHeight_Index = Item.BinBalance.BinBalance_UnitHeightBal_Index,
                        //    UnitHeight_Id = Item.BinBalance.BinBalance_UnitHeightBal_Id,
                        //    UnitHeight_Name = Item.BinBalance.BinBalance_UnitHeightBal_Name,
                        //    UnitHeightRatio = Item.BinBalance.BinBalance_UnitHeightBalRatio,

                        //    Height = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitHeightBal ?? 0), 6),
                        //    Height_Index = Item.BinBalance.BinBalance_HeightBal_Index,
                        //    Height_Id = Item.BinBalance.BinBalance_HeightBal_Id,
                        //    Height_Name = Item.BinBalance.BinBalance_HeightBal_Name,
                        //    HeightRatio = Item.BinBalance.BinBalance_HeightBalRatio,

                        //    UnitPrice = Item.BinBalance.UnitPrice,
                        //    UnitPrice_Index = Item.BinBalance.UnitPrice_Index,
                        //    UnitPrice_Id = Item.BinBalance.UnitPrice_Id,
                        //    UnitPrice_Name = Item.BinBalance.UnitPrice_Name,

                        //    Price = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.UnitPrice ?? 0), 6),
                        //    Price_Index = Item.BinBalance.Price_Index,
                        //    Price_Id = Item.BinBalance.Price_Id,
                        //    Price_Name = Item.BinBalance.Price_Name,

                        //    DocumentRef_No5 = Item.BinBalance.BinBalance_QtyBal?.ToString(),

                        //    Document_Status = 0,
                        //    Create_By = ActiveBy,
                        //    Create_Date = ActiveDate


                        //};

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
                        var modelSaleUnit = db.Ms_ProductConversion.Where(c => c.Product_Index == Item.BinBalance.Product_Index && c.SALE_UNIT == 1).FirstOrDefault();
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

                        GoodsReplenishItem.Weight = decimal.Round(Item.Replenish_Qty * (Item.BinBalance.BinBalance_UnitGrsWeightBal ?? 0), 6);
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

                        //  var resultAssignjob = Utils.SendDataApi<string>(new AppSettingConfig().GetUrl("AssignJobTransfer"), JsonConvert.SerializeObject(new View_AssignJobLocViewModel()  { modelassjob }.sJson()));
                        //JsonConvert.SerializeObject(modelassjob)

                        // Comment 20210811 manual confirm assignjob
                        var resultAssignjob = Utils.SendDataApi<string>(new AppSettingConfig().GetUrl("AssignJobTransfer"), JsonConvert.SerializeObject(modelassjob));

                        olog.logging("CreateReplenishDocument", "SendWCSPutAwayVC GoodsReplenishNo : " + GoodsReplenishNo);

                        var modelTransferReple = new { docNo = GoodsReplenishNo };

                        var resultSendWCSPutAwayVC = Utils.SendDataApi<dynamic>(new AppSettingConfig().GetUrl("SendWCSPutAwayVC"), JsonConvert.SerializeObject(modelTransferReple));

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

                var myTransaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    db.SaveChanges();
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

        #region + Trace Transfer +

        public actionResultTrace_TransferViewModel printOutTraceTransferReplenish(TraceTransferModel data)
        {
            var culture = new System.Globalization.CultureInfo("en-US");
            var olog = new logtxt();
            try
            {
                db.Database.SetCommandTimeout(360);

                var PID = new SqlParameter("@ProductID", data.product_Id == null ? "" : data.product_Id.ToString());
                var PLOT = new SqlParameter("@ProductLot", data.product_Lot == null ? "" : data.product_Lot.ToString());
                var TAG = new SqlParameter("@TagNo", data.tag_No == null ? "" : data.tag_No.ToString());
                var LO = new SqlParameter("@LocationID", data.location_Id == null ? "" : data.location_Id.ToString());
                var GT = new SqlParameter("@GoodsTransferNo", data.goodsTransfer_No == null ? "" : data.goodsTransfer_No.ToString());
                var ST = new SqlParameter("@Status", data.processStatus_Id == null ? "" : data.processStatus_Id.ToString());
                var TFD = new SqlParameter("@Transfer_Date", data.transfer_Date == null ? "" : data.transfer_Date.ToString());
                var TFDT = new SqlParameter("@Transfer_Date_To", data.transfer_Date_To == null ? "" : data.transfer_Date_To.ToString());

                var responseData = dbTf.sp_Trace_replenishment.FromSql("sp_Trace_replenishment @ProductID ,@ProductLot ,@TagNo ,@LocationID ,@GoodsTransferNo ,@Status ,@Transfer_Date , @Transfer_Date_To", PID, PLOT, TAG, LO, GT, ST, TFD, TFDT).ToList();

                if (!string.IsNullOrEmpty(data.transfer_Date) && !string.IsNullOrEmpty(data.transfer_Date_To))
                {
                    var dateStart = Convert.ToDateTime(data.transfer_Date);
                    var dateEnd = Convert.ToDateTime(data.transfer_Date_To);
                    responseData = responseData.Where(c => c.GoodsTransfer_Date >= dateStart && c.GoodsTransfer_Date <= dateEnd).ToList();
                }

                var TotalRow = new List<sp_Trace_replenishment>();

                TotalRow = responseData.ToList();
                var Row = 1;

                if (!data.export)
                {
                    if (data.CurrentPage != 0 && data.PerPage != 0)
                    {
                        responseData = responseData.Skip(((data.CurrentPage - 1) * data.PerPage)).OrderBy(c => c.RowIndex).ToList();
                        Row = (data.CurrentPage == 1 ? 1 : ((data.CurrentPage - 1) * data.PerPage) + 1);
                    }

                    if (data.PerPage != 0)
                    {
                        responseData = responseData.Take(data.PerPage).OrderBy(c => c.RowIndex).ToList();

                    }
                    else
                    {
                        responseData = responseData.Take(50).OrderBy(c => c.RowIndex).ToList();
                    }
                }

                var result = new List<TraceTransferModel>();
                foreach (var item in responseData)
                {
                    TraceTransferModel traceTransfer = new TraceTransferModel();
                    traceTransfer.rowIndex = Row;
                    traceTransfer.goodsTransfer_No = item.GoodsTransfer_No;
                    traceTransfer.tag_No = item.Tag_No;
                    traceTransfer.product_Id = item.Product_Id;
                    traceTransfer.product_Name = item.Product_Name;
                    traceTransfer.product_Lot = item.Product_Lot;
                    traceTransfer.qty = item.Qty;
                    traceTransfer.ratio = item.Ratio;
                    traceTransfer.totalQty = item.TotalQty;
                    traceTransfer.productConversion_Name = item.ProductConversion_Name;
                    traceTransfer.location_Id = item.Location_Id;
                    traceTransfer.location_Id_To = item.Location_Id_To;
                    traceTransfer.goodsTransfer_Date = Convert.ToDateTime(item.GoodsTransfer_Date).ToString("dd/MM/yyyy");
                    //traceTransfer.gt_Status = item.GT_Status;
                    //traceTransfer.gti_Status = item.GTI_Status;
                    //traceTransfer.t_Status = item.T_Status;
                    //traceTransfer.ti_Status = item.TI_Status; 

                    traceTransfer.remaining = item.Remaining;
                    traceTransfer.unit_Remaining = item.Unit_Remaining;
                    traceTransfer.total = item.Total;
                    traceTransfer.unit_Total = item.Unit_Total;

                    traceTransfer.create_By = item.Create_By;
                    traceTransfer.create_Date = Convert.ToDateTime(item.Create_Date).ToString("dd/MM/yyyy hh:mm:ss");
                    traceTransfer.update_By = item.Update_By;
                    traceTransfer.update_Date = Convert.ToDateTime(item.Update_Date).ToString("dd/MM/yyyy hh:mm:ss");

                    traceTransfer.documentType_Id = item.DocumentType_Id;
                    traceTransfer.processStatus_Index = item.ProcessStatus_Index;
                    traceTransfer.processStatus_Id = item.ProcessStatus_Id;
                    traceTransfer.processStatus_Name = item.ProcessStatus_Name;

                    Row++;
                    result.Add(traceTransfer);
                }
                var count = TotalRow.Count;
                var actionResult = new actionResultTrace_TransferViewModel();
                actionResult.itemsTrace = result.ToList();
                actionResult.pagination = new Pagination() { TotalRow = count, CurrentPage = data.CurrentPage, PerPage = data.PerPage, };
                return actionResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        public Validated_ImportModel GenerateTaskPiecePick()
        {
            String State = "Start";
            var olog = new logtxt();

            var import_userindex = Guid.NewGuid();
            var import_Validate = Guid.Parse("D0006E01-8576-4505-894D-74ADA251B601");
            try
            {
                olog.logging("GenerateTaskPiecePick", State);

                dbTf.Database.SetCommandTimeout(360);
                Guid ImportIndex = Guid.NewGuid();
                Validated_ImportModel result_model = new Validated_ImportModel() { Import_GuID = ImportIndex };

                var checkdupstep = dbTf._Prepare_Imports_step.FirstOrDefault(c => c.Import_Index == import_Validate);
                if (checkdupstep != null)
                {
                    if (checkdupstep.Import_Status != 0)
                    {
                        result_model.ResultIsUse = false;
                        result_model.ResultMsg = "ไม่สามารถ Generate task ได้ เนื่องจากมีการทำงานอยู่";

                        return result_model;
                    }
                    else
                    {
                        checkdupstep.Import_Status = 1;
                        checkdupstep.import_userindex = import_userindex;

                        var transaction = dbTf.Database.BeginTransaction(IsolationLevel.Serializable);
                        try
                        {
                            dbTf.SaveChanges();
                            transaction.Commit();

                        }
                        catch (Exception exy)
                        {
                            transaction.Rollback();
                            result_model.ResultIsUse = false;
                            result_model.ResultMsg = "ไม่สามารถทำการ Generate task ได้ กรุณาติดต่อ Admin : " + exy.Message;
                            return result_model;
                        }

                        Thread.Sleep(2000);

                        _Prepare_Imports_step checkdupstep_user = dbTf._Prepare_Imports_step.FirstOrDefault(c => c.Import_Index == import_Validate && c.import_userindex == import_userindex);
                        if (checkdupstep_user == null)
                        {
                            result_model.ResultIsUse = false;
                            result_model.ResultMsg = "พบว่ามีการกด Generate task ซ้ำ หรือ พร้อมกัน กรุณารอหรือลองอีกครั้งในภายหลัง";
                            return result_model;
                        }
                    }
                }
                else
                {
                    result_model.ResultIsUse = false;
                    result_model.ResultMsg = "กรุณาติดต่อ Admin";
                    return result_model;
                }

                ReplenishmentService service = new ReplenishmentService();
                var result = service.ActivateReplenishmentPiecePick();

                _Prepare_Imports_step status_validate = dbTf._Prepare_Imports_step.FirstOrDefault(c => c.Import_Index == import_Validate && c.import_userindex == import_userindex);
                if (status_validate != null)
                {
                    checkdupstep.Import_Status = 0;
                    checkdupstep.Import_By = null;
                    checkdupstep.import_userindex = null;
                    checkdupstep.Import_File_Name = null;
                }

                var transactionX = dbTf.Database.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    dbTf.SaveChanges();
                    transactionX.Commit();

                }
                catch (Exception exy)
                {
                    transactionX.Rollback();
                    throw exy;
                }

                result_model.ResultIsUse = true;
                result_model.ResultMsg = "Generate task success";
                return result_model;

            }
            catch (Exception ex)
            {
                #region Update status _Prepare_Imports_step
                _Prepare_Imports_step checkdupstep = dbTf._Prepare_Imports_step.FirstOrDefault(c => c.Import_Index == import_Validate && c.import_userindex == import_userindex);

                if (checkdupstep != null)
                {
                    checkdupstep.Import_Status = 0;
                    checkdupstep.Import_By = null;
                    checkdupstep.import_userindex = null;
                    checkdupstep.Import_File_Name = null;

                    var transaction = dbTf.Database.BeginTransaction(IsolationLevel.Serializable);
                    try
                    {
                        dbTf.SaveChanges();
                        transaction.Commit();

                    }
                    catch (Exception exy)
                    {
                        transaction.Rollback();
                        throw exy;
                    }
                }
                #endregion

                olog.logging("GenerateTaskPiecePick", "Error : " + ex.ToString());

                Validated_ImportModel result_model_ex = new Validated_ImportModel();
                result_model_ex.ResultIsUse = true; //false
                result_model_ex.ResultMsg = "Generate task success"; //ex.Message
                return result_model_ex;
            }
        }

        //#region BypassASRS
        //public List<string> ActivateBypassReplenishmentFromASRS(ReplenishOnDemandViewModel model)
        //{

        //    String State = "Start";
        //    String msglog = "";
        //    var olog = new logtxt();

        //    List<string> GoodsReplenishDocuments = new List<string>();
        //    try
        //    {
        //        olog.logging("ActivateReplenishmentASRS", State);

        //        //Find Task Replenish
        //        DateTime currentDate = DateTime.Today;
        //        TimeSpan currentTime = new TimeSpan(0, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

        //        int dayOfWeek = (int)currentDate.DayOfWeek;
        //        List<Ms_Replenishment> Replenishments = db.Ms_Replenishment.Where(
        //           w => w.IsActive == 1 && (currentTime >= w.Trigger_Time && currentTime <= w.Trigger_Time_End) && (
        //              ((dayOfWeek == 0 ? w.IsSunday :
        //                dayOfWeek == 1 ? w.IsMonday :
        //                dayOfWeek == 2 ? w.IsTuesday :
        //                dayOfWeek == 3 ? w.IsWednesday :
        //                dayOfWeek == 4 ? w.IsThursday :
        //                dayOfWeek == 5 ? w.IsFriday :
        //                dayOfWeek == 6 ? w.IsSaturday : false) == true && w.Trigger_Date == null) ||
        //                (w.Trigger_Date.HasValue ? w.Trigger_Date : currentDate.AddDays(-1)) >= currentDate &&
        //                (w.Trigger_Date_End.HasValue ? w.Trigger_Date_End : w.Trigger_Date) <= currentDate)
        //        ).ToList();

        //        if (Replenishments.Count == 0)
        //        {
        //            //no task found.
        //            olog.logging("ActivateReplenishmentASRS", "Task Replenish not found");
        //            throw new Exception("Task Replenish not found");
        //        }

        //        //TO DO Config Index
        //        //Prepare BinBalance Model
        //        Guid GoodsReplenishDocumentTypeIndex = Guid.Parse("D61AB6E6-FFB7-47B9-A2D3-CD4AF77E98C5"); // Auto Replenishment ASRS
        //        Ms_DocumentType goodsReplenishDocumentType = db.Ms_DocumentType.Find(GoodsReplenishDocumentTypeIndex);
        //        if (goodsReplenishDocumentType is null)
        //        {
        //            olog.logging("ActivateReplenishmentASRS", "Replenish DocumentType not found");
        //            throw new Exception("Replenish DocumentType not found");
        //        }

        //        // Fix TOP 100
        //        Guid StorageLocationTypeIndex = Guid.Parse("02F5CBFC-769A-411B-9146-1D27F92AE82D");   // ASRS
        //        List<Guid> ReplenishLocationIndexs =
        //            JsonConvert.DeserializeObject<List<Guid>>(
        //            JsonConvert.SerializeObject(
        //                db.Ms_Location.Where(s => s.IsActive == 1 && s.LocationType_Index == StorageLocationTypeIndex).Select(s => s.Location_Index)));
        //        if ((ReplenishLocationIndexs?.Count ?? 0) == 0)
        //        {
        //            olog.logging("ActivateReplenishmentASRS", "Replenish Location not found");
        //            throw new Exception("Replenish Location not found");
        //        }

        //        List<Guid> ReplenishItemStatusIndexs = new List<Guid> { Guid.Parse("525BCFF1-2AD9-4ACB-819D-0DEA4E84EA12") };
        //        SearchReplenishmentBalanceModel binBalance_API_Model = new SearchReplenishmentBalanceModel()
        //        {
        //            ReplenishLocationIndexs = ReplenishLocationIndexs,
        //            ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
        //        };

        //        List<string> errorMsg = new List<string>();
        //        List<ReplenishmentBalanceModel> binBalances;
        //        foreach (Ms_Replenishment replenishment in Replenishments)
        //        {
        //            try
        //            {

        //                binBalances = new List<ReplenishmentBalanceModel>();
        //                binBalance_API_Model = new SearchReplenishmentBalanceModel()
        //                {
        //                    ReplenishLocationIndexs = ReplenishLocationIndexs,
        //                    ReplenishItemStatusIndexs = ReplenishItemStatusIndexs
        //                };

        //                olog.logging("ActivateReplenishmentASRS", " GetBinBalanceReplenish Replenishment_Id :  " + replenishment.Replenishment_Id);


        //                binBalances = GetBinBalanceReplenish(goodsReplenishDocumentType, replenishment, binBalance_API_Model);
        //                if ((binBalances?.Count ?? 0) > 0)
        //                {
        //                    olog.logging("ActivateReplenishmentASRS", " CreateReplenishDocument Replenishment_Id :  " + replenishment.Replenishment_Id);


        //                    GoodsReplenishDocuments.AddRange(
        //                        CreateReplenishDocument(replenishment.Replenishment_Index, goodsReplenishDocumentType, binBalances)
        //                    );
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                olog.logging("ActivateReplenishmentASRS", "GetBinBalanceReplenish " + ex.Message);

        //                errorMsg.Add(ex.Message);
        //                continue;
        //            }
        //        }

        //        if (errorMsg.Count > 0)
        //        {
        //            olog.logging("ActivateReplenishmentASRS", "Receieved Error : " + string.Join(",", errorMsg.Distinct()));
        //            throw new Exception("Receieved Error : " + string.Join(",", errorMsg.Distinct()));
        //        }

        //        return GoodsReplenishDocuments;
        //    }
        //    catch (Exception ex)
        //    {
        //        olog.logging("ActivateReplenishmentASRS", ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
        //        throw new Exception(ex.Message + (GoodsReplenishDocuments.Count > 0 ? " Document Created " + JsonConvert.SerializeObject(GoodsReplenishDocuments) : string.Empty));
        //    }
        //}
        //#endregion

        #region Get Data from Binbalance

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
            try
            {
                SearchReplenishmentBalanceModel data = GetSearchReplenishmentBalanceModel(jsonData);
                List<ReplenishmentBalanceModel> ReplenishmentBinBalance = new List<ReplenishmentBalanceModel>();



                List<wm_BinBalance> StorageBinBalances;
                decimal ReplenishQty, SumLocationBinBalanceQty, SumStorageBalanceQty, PendingReplenishQty, StorageQty;

                foreach (SearchReplenishmentBalanceItemModel Item in data.Items)
                {
                    SumLocationBinBalanceQty = dbBa.wm_BinBalance.Where(
                        s => (data.Owner_Index.HasValue ? s.Owner_Index.Equals(data.Owner_Index) : true) &&
                             (s.Product_Index.Equals(Item.Product_Index)) &&
                             (s.Location_Index.Equals(Item.Location_Index)) &&
                             (s.BinBalance_QtyReserve >= 0) &&
                             (s.BinBalance_QtyBal > 0)
                    ).Sum(s => s.BinBalance_QtyBal) ?? 0; // (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;

                    if (SumLocationBinBalanceQty > 0)
                    {
                        ReplenishQty = 0;
                    }
                    else
                    {
                        ReplenishQty = (Item.Replenish_Qty > Item.Minimum_Qty ? Item.Replenish_Qty : Item.Minimum_Qty) - Item.Pending_Replenish_Qty - SumLocationBinBalanceQty;

                    }


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
                             (s.BinBalance_QtyReserve == 0) &&
                             (data.ReplenishItemStatusIndexs.Contains(s.ItemStatus_Index))
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
                    ).Sum(s => s.BinBalance_QtyBal - s.BinBalance_QtyReserve) ?? 0; // (s.BinBalance_QtyBal - s.BinBalance_QtyReserve)) ?? 0;




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
                    foreach (wm_BinBalance StorageBalance in StorageBinBalances.OrderBy(c => c.GoodsReceive_EXP_Date).ThenBy(q => q.BinBalance_QtyBal).ThenBy(d => d.Location_Name))
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
        #endregion

    }

}
