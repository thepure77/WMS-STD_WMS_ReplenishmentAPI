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
using MasterDataBusiness.ViewModels;
using MasterDataBusiness.BusinessUnit;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MasterDataBusiness
{
    public class ConfigPiecepickItemService
    {
        private MasterDbContext db;

        public ConfigPiecepickItemService()
        {
            db = new MasterDbContext();
        }

        public ConfigPiecepickItemService(MasterDbContext db)
        {
            this.db = db;
        }

        #region filterConfigPiecepickItem
        public actionResultConfigPiecepickItemViewModel filter(SearchConfigPiecepickItemViewModel data)
        {
            try
            {

                var query = db.View_Replenishment_Config.AsQueryable();
                //query = query.Where(c => c.IsActive == 1 || c.IsActive == 0 && c.IsDelete == 0);


                if (!string.IsNullOrEmpty(data.replenishment_Id))
                {
                    query = query.Where(c => c.Product_Id.Contains(data.replenishment_Id)
                                         || c.Location_Name.Contains(data.replenishment_Id));
                }
                
                if(data.location_Type == "01")
                {
                    query = query.Where(c => c.LocType == "PA" || c.LocType == "PB");
                }else if(data.location_Type == "02")
                {
                    query = query.Where(c => c.LocType == "VC");
                }
                else
                { 
                
                }
                
               /* if (!string.IsNullOrEmpty(data.location_Type))
                {
                    query = query.Where(c => c.Product_Id.Contains(data.replenishment_Id)
                                         || c.Location_Name.Contains(data.replenishment_Id));
                }*/

                var Item = new List<View_Replenishment_Config>();
                var TotalRow = new List<View_Replenishment_Config>();

                TotalRow = query.ToList();


                /*if (data.CurrentPage != 0 && data.PerPage != 0)
                {
                    query = query.Skip(((data.CurrentPage - 1) * data.PerPage));
                }

                if (data.PerPage != 0)
                {
                    query = query.Take(data.PerPage);
                
                }*/

                Item = query.OrderBy(o => o.Product_Id).ToList();

                var result = new List<SearchConfigPiecepickItemViewModel>();

                foreach (var item in Item)
                {
                    var resultItem = new SearchConfigPiecepickItemViewModel();

                    resultItem.replenishment_Index = item.Replenishment_Index;
                    resultItem.replenishment_Id = item.Replenishment_Id;
                    resultItem.replenishment_Remark = item.Replenishment_Remark;
                    resultItem.isActive = item.IsActive;
                    resultItem.product_Id = item.Product_Id;
                    resultItem.product_Name = item.Product_Name;
                    resultItem.location_Name = item.Location_Name;
                    resultItem.qty = item.Qty;
                    resultItem.replenish_Qty = item.Replenish_Qty;
                    resultItem.min_Qty = item.Min_Qty;
                    resultItem.locType = item.LocType;
                    resultItem.replenishment_Product_Index = item.Replenishment_Product_Index;
                    resultItem.replenishment_Location_Index = item.Replenishment_Location_Index;
                    resultItem.productLocation_Index = item.ProductLocation_Index;
                    resultItem.product_Index = item.Product_Index;
                    resultItem.location_Index = item.Location_Index;
                    resultItem.sale_Unit = item.Sale_Unit == null ? "" : item.Sale_Unit;
                    result.Add(resultItem);
                }

                var count = TotalRow.Count;

                var actionResultConfigPiecepickItemViewModel = new actionResultConfigPiecepickItemViewModel();
                actionResultConfigPiecepickItemViewModel.itemsConfigPiecepickItem = result.ToList();
                actionResultConfigPiecepickItemViewModel.pagination = new Pagination() { TotalRow = count, CurrentPage = data.CurrentPage, PerPage = data.PerPage, Key = data.key };

                return actionResultConfigPiecepickItemViewModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region autoConfigPiecepickItem
        public List<ItemListViewModel> autoSearchConfigPiecepickItemFilter(ItemListViewModel data)
        {
            try
            {
                using (var context = new MasterDbContext())

                {
                    var query = context.View_Replenishment_Config.Where(c => c.IsActive == 1 || c.IsActive == 0);

                    if (data.key == "-")
                    {

                    }
                    else if (!string.IsNullOrEmpty(data.key))
                    {
                        query = query.Where(c => c.Product_Id.Contains(data.key));
                    }

                    var items = new List<ItemListViewModel>();

                    var result = query.Select(c => new { c.Product_Index, c.Product_Id, c.Product_Name }).Distinct().Take(10).ToList();

                    foreach (var item in result)
                    {
                        var resultItem = new ItemListViewModel
                        {
                            index = item.Product_Index,
                            id = item.Product_Id,
                            name = item.Product_Name
                        };

                        items.Add(resultItem);
                    }
                    return items;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region SaveChanges
        public String SaveChanges(ConfigPiecepickItemViewModel data)
        {
            try
            {
                //var query = db.View_Replenishment.AsQueryable();
                /*var c = checkLocation(data);
                if (c == "Fail")
                {
                    throw new Exception("Fail");
                }*/
                var productID = new SqlParameter("@Product_Id", data.product_Id);
                var locationName = new SqlParameter("@Location_Name", data.location_Name);
                var maxQty = new SqlParameter("@MaxQty", data.qty);
                var minQty = new SqlParameter("@MinQty", data.min_Qty);
                var create_By = new SqlParameter("@Create_By", string.IsNullOrEmpty(data.create_By) ? "system" : data.create_By);

                var roweffect = db.Database.ExecuteSqlCommand("EXEC sp_Insert_Config_Replenish @Product_Id , @Location_Name, @MaxQty, @MinQty, @Create_By", productID, locationName, maxQty, minQty, create_By);

                return "Done";

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region find
        public ConfigPiecepickItemViewModel find(Guid id)
        {
            try
            {

                var queryResult = db.View_Replenishment_Config.Where(c => c.Replenishment_Index == id).FirstOrDefault();

                var result = new ConfigPiecepickItemViewModel();

                result.show_location_Name = queryResult.Location_Name;
                checkLocation(result);
                result.replenishment_Index = queryResult.Replenishment_Index;
                result.replenishment_Id = queryResult.Replenishment_Id;
                result.replenishment_Remark = queryResult.Replenishment_Remark;
                result.isActive = queryResult.IsActive;
                result.product_Id = queryResult.Product_Id;
                result.product_Name = queryResult.Product_Name;
                result.location_Name = queryResult.Location_Name;
                result.qty = queryResult.Qty;
                result.replenish_Qty = queryResult.Replenish_Qty;
                result.show_replenish_Qty = queryResult.Qty;
                result.min_Qty = queryResult.Min_Qty;
                result.show_min_Qty = queryResult.Min_Qty;
                result.locType = queryResult.LocType;
                result.replenishment_Product_Index = queryResult.Replenishment_Product_Index;
                result.replenishment_Location_Index = queryResult.Replenishment_Location_Index;
                result.productLocation_Index = queryResult.ProductLocation_Index;
                result.product_Index = queryResult.Product_Index;
                result.location_Index = queryResult.Location_Index;
                result.sale_Unit = queryResult.Sale_Unit == null ? "" : queryResult.Sale_Unit;

                return result;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region checkLocation
        public string checkLocation(ConfigPiecepickItemViewModel data)
        {
            try
            {
                var query = db.Ms_Location.AsQueryable();
                var queryView = db.View_Replenishment_Config.AsQueryable();
                var checkLocation = query.Where(c => c.Location_Name == data.location_Name).FirstOrDefault();
                var checkLocationView = queryView.Where(c => c.Location_Name == data.location_Name).FirstOrDefault();
                if (data.show_location_Name == data.location_Name || checkLocation != null && checkLocationView == null)
                {
                    return "Done";
                }else
                {
                    return "Fail";
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region SaveImportList
        public String SaveImportList(List<ConfigPiecepickItemViewModel> model)
        {
            // GET CONFIG CONNECTIONSTRING
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false);
            var configuration = builder.Build();
            var connectionString = configuration.GetConnectionString("Master_ConnectionString").ToString();
            SqlConnection conn = new SqlConnection(connectionString);

            try
            {                
                var Result = "";
                List<string> list_return = new List<string>();
                List<int> list_Codereturn = new List<int>();

                foreach (var data in model)
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_Insert_Config_Replenish", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Product_Id", SqlDbType.NVarChar).Value = data.product_Id;
                    cmd.Parameters.Add("@Location_Name", SqlDbType.NVarChar).Value = data.location_Name;
                    cmd.Parameters.Add("@MaxQty", SqlDbType.Int).Value = data.qty;
                    cmd.Parameters.Add("@MinQty", SqlDbType.Int).Value = data.min_Qty;
                    cmd.Parameters.Add("@Create_By", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(data.create_By) ? "system" : data.create_By;

                    var strReturn = cmd.ExecuteScalar().ToString();

                   if (conn.State == ConnectionState.Open)
                       conn.Close();

                    if (!strReturn.Contains("Insert Complete "))
                    {
                        list_return.Add(strReturn);
                        list_Codereturn.Add(400);
                    }
                    else
                    {
                        list_Codereturn.Add(200);
                    }                   
                }

                if (list_Codereturn.Where(a => a != 200).Count() > 0)
                {
                    Result = string.Join(" , ", list_return);
                }
                else
                {
                    Result = "Done";
                }

                return Result;
            }
            catch (Exception ex)
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                throw ex;
            }
        }
        #endregion
    }
}
