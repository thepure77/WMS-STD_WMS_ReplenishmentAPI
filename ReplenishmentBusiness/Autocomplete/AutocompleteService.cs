using DataAccess;
using MasterDataBusiness.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterDataBusiness
{
    public class AutocompleteService
    {

        private MasterDbContext db;

        public AutocompleteService()
        {
            db = new MasterDbContext();
        }

        public AutocompleteService(MasterDbContext db)
        {
            this.db = db;
        }

       

        #region autoProduct
        public List<ItemListViewModel> autoProduct(ItemListViewModel data)
        {
            try
            {

                using (var context = new MasterDbContext())
                {

                    var owP = context.MS_ProductOwner.AsQueryable();

                    if (!string.IsNullOrEmpty(data.key2))
                    {
                        owP = owP.Where(c => c.Owner_Index == new Guid(data.key2));
                    }

                    var queryO = owP.ToList();

                    var query = context.Ms_Product.Where(c => c.IsActive == 1 && c.IsDelete == 0);
                    if (data.key == "-")
                    {


                    }
                    else if (!string.IsNullOrEmpty(data.key))
                    {
                        query = query.Where(c => c.Product_Name.Contains(data.key));

                    }
                    else if (!string.IsNullOrEmpty(data.key3))
                    {
                        query = query.Where(c => c.Product_Name.Contains(data.key3));
                    }

                    query = query.Where(c => queryO.Select(s => s.Product_Index).Contains(c.Product_Index));



                    var result = query.Select(c => new {c.Product_Id}).Distinct().Take(10).ToList();

                    var items = new List<ItemListViewModel>();

                    foreach (var item in result)
                    {
                        var resultItem = new ItemListViewModel
                        {
                            id = item.Product_Id
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

        #region autoLocation
        public List<ItemListViewModel> autoLocation(ItemListViewModel data)
        {
            try
            {
                using (var context = new MasterDbContext())
                {
                    var query = context.Ms_Location.Where(c => c.IsActive == 1 || c.IsActive == 0 && c.IsDelete == 0);
                    query = query.Where(a => a.LocationType_Index.ToString() == "E77778D2-7A8E-448D-BA31-CD35FD938FC3" || a.LocationType_Index.ToString() == "7F3E1BC2-F18B-4B16-80A9-2394EB8BBE63");
                    if (data.key == "-")
                    {

                    }

                    else if (!string.IsNullOrEmpty(data.key))
                    {
                        query = query.Where(c => c.Location_Id.Contains(data.key)
                                                || c.Location_Name.Contains(data.key));
                    }

                    var items = new List<ItemListViewModel>();
                    var result = query.Select(c => new { c.Location_Name, c.Location_Index, c.Location_Id }).Distinct().Take(10).ToList();
                    //result.Where(c => c.Location_Name && data.key.Any(p => p == c.Location_Name));
                    
                    foreach (var item in result)
                    {
                        var resultItem = new ItemListViewModel
                        {
                            //index = new Guid(item.User_Name),
                            index = item.Location_Index,
                            id = item.Location_Id,
                            name = item.Location_Name,
                            key = item.Location_Id + " - " + item.Location_Name,
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

        #region autoSku
        public List<ItemListViewModel> autoSku(ItemListViewModel data)
        {
            try
            {

                using (var context = new MasterDbContext())
                {

                    var owP = context.MS_ProductOwner.AsQueryable();

                    if (!string.IsNullOrEmpty(data.key2))
                    {
                        owP = owP.Where(c => c.Owner_Index == new Guid(data.key2));
                    }

                    var queryO = owP.ToList();

                    var query = context.Ms_Product.Where(c => c.IsActive == 1 && c.IsDelete == 0);
                    if (data.key == "-")
                    {


                    }
                    else if (!string.IsNullOrEmpty(data.key))
                    {
                        query = query.Where(c => c.Product_Id.Contains(data.key));
                    }
                    if (data.key3 == "-")
                    {


                    }
                    else if (!string.IsNullOrEmpty(data.key3))
                    {
                        query = query.Where(c => c.Product_Id.Contains(data.key3));
                    }

                    query = query.Where(c => queryO.Select(s => s.Product_Index).Contains(c.Product_Index));

                    var result = query.Select(c => new { c.Product_Index, c.Product_Id, c.Product_Name, c.Ref_No1, c.Ref_No2, c.IsLot, c.UDF_1, c.IsMfgDate, c.IsExpDate }).Distinct().Take(10).ToList();

                    var items = new List<ItemListViewModel>();

                    foreach (var item in result)
                    {
                        var resultItem = new ItemListViewModel
                        {
                            index = item.Product_Index,
                            id = item.Product_Id,
                            name = item.Product_Name,
                            value1 = item.Ref_No1,
                            value2 = item.Ref_No2,
                            value3 = item.IsLot.ToString(),
                            value4 = item.IsMfgDate.ToString(),
                            value7 = item.IsExpDate.ToString(),
                            value6 = item.UDF_1
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

        #region autoProductId
        public List<ItemListViewModel> autoProductId(ItemListViewModel data)
        {
            try
            {

                using (var context = new MasterDbContext())
                {
                    //var query = context.Ms_Product.AsQueryable();
                    var query = context.View_AutoProduct.AsQueryable();
                    //query = query.Where(c => c.ProductType_Name != null);
                    //query = query.Where(c => c.Ref_No1 == "carton flow rack");
                    query = query.Where(c => c.Ref_No1 == "carton flow rack" && c.SALE_UNIT == 1);
                     
                    if (!string.IsNullOrEmpty(data.key.ToString()) && data.key != "-")
                    {
                        query = query.Where(c => c.Product_Id.Contains(data.key));
                    }
                    var result = query.Select(c => new { c.Product_Index, c.Product_Id, c.Product_Name, c.ProductConversion_Name }).Distinct().Take(10).ToList();
                    //var result = query.Select(c => new {c.Product_Id, c.Product_Name, c.ProductConversion_Name }).Distinct().Take(10).ToList();

                    var items = new List<ItemListViewModel>();

                    foreach (var item in result)
                    {
                        var resultItem = new ItemListViewModel
                        {
                            index = item.Product_Index,
                            id = item.Product_Name,
                            name = item.Product_Id,
                            value1 = item.ProductConversion_Name == null ? "" : item.ProductConversion_Name
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

    }
}
