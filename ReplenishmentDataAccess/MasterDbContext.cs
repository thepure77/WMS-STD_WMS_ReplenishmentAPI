using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

using DataAccess.Models.Master.Table;
using DataAccess.Models.Master.View;
using DataAccess.Models.Utils;
using MasterDataDataAccess.Models;

namespace DataAccess
{
    public class MasterDbContext : DbContext
    {
        public virtual DbSet<GetValueByColumn> GetValueByColumn { get; set; }

        public virtual DbSet<Sy_AutoNumber> Sy_AutoNumber { get; set; }

        public virtual DbSet<Ms_DocumentType> Ms_DocumentType { get; set; }

        public virtual DbSet<Ms_Product> Ms_Product { get; set; }

        public virtual DbSet<Ms_Location> Ms_Location { get; set; }

        public virtual DbSet<Ms_ProductLocation> Ms_ProductLocation { get; set; }

        public virtual DbSet<Ms_ZoneLocation> Ms_ZoneLocation { get; set; }

        public virtual DbSet<Ms_Replenishment> Ms_Replenishment { get; set; }

        public virtual DbSet<Ms_Replenishment_Product> Ms_Replenishment_Product { get; set; }

        public virtual DbSet<Ms_Replenishment_Location> Ms_Replenishment_Location { get; set; }

        public virtual DbSet<View_ProductLocation> View_ProductLocation { get; set; }

        public virtual DbSet<View_Replenishment_Product> View_Replenishment_Product { get; set; }

        public virtual DbSet<View_Replenishment_Location> View_Replenishment_Location { get; set; }

        public virtual DbSet<Ms_ProductConversion> Ms_ProductConversion { get; set; }

        public virtual DbSet<View_ReplenishmentOnDemand> View_ReplenishmentOnDemand { get; set; }

        public virtual DbSet<sp_ReplenishmentOnDemand> sp_ReplenishmentOnDemand { get; set; }

        public virtual DbSet<View_Replenishment> View_Replenishment { get; set; }

        public virtual DbSet<View_Replenishment_Config> View_Replenishment_Config { get; set; }
        
        public virtual DbSet<MS_ProductOwner> MS_ProductOwner { get; set; }
        public virtual DbSet<View_AutoProduct> View_AutoProduct { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false);

                var configuration = builder.Build();

                var connectionString = configuration.GetConnectionString("Master_ConnectionString").ToString();

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
