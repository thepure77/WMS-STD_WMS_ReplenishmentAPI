using DataAccess;
using DataAccess.Models.Master.Table;

using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;

namespace Business.Commons
{
    public static class GenAutoNumber
    {
        #region GenAutonumber
        public static string GenAutonumber(this string Sys_Key)
        {
            var db = new MasterDbContext();
            string result = "";

            try
            {
                Sy_AutoNumber model = new Sy_AutoNumber();

                var transactionx = db.Database.BeginTransaction(IsolationLevel.Serializable);
                var query = db.Sy_AutoNumber.Where(c => c.Sys_Key == Sys_Key).FirstOrDefault();
                if (query != null)
                {
                    var item = db.Sy_AutoNumber.Find(query.Sys_Key);
                    item.Sys_Value = query.Sys_Value + 1;
                    item.Update_Date = DateTime.Now;
                    result = item.Sys_Value.ToString();

                }
                else
                {
                    model.Sys_Key = Sys_Key;
                    model.Sys_Text = "";
                    model.Sys_Value = 1;
                    model.IsActive = 1;
                    model.IsDelete = 0;
                    model.IsSystem = 1;
                    model.Status_Id = 0;
                    model.Create_By = "System";
                    model.Create_Date = DateTime.Now;
                    result = model.Sys_Value.ToString();
                    db.Sy_AutoNumber.Add(model);
                }

                try
                {
                    db.SaveChanges();
                    transactionx.Commit();
                }

                catch (Exception exy)
                {
                    transactionx.Rollback();
                    throw exy;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion  

    }
}