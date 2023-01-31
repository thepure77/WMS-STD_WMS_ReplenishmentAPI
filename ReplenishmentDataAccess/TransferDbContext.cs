using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

using DataAccess.Models.Transfer.Table;
using DataAccess.Models.Transfer.StoredProcedure;

namespace DataAccess
{
    public class TransferDbContext : DbContext
    {
        public virtual DbSet<Im_GoodsTransfer> Im_GoodsTransfer { get; set; }

        public virtual DbSet<Im_GoodsTransferItem> Im_GoodsTransferItem { get; set; }

        public virtual DbSet<Ms_DocumentTypeNumber> Ms_DocumentTypeNumber { get; set; }

        public virtual DbSet<sp_Trace_replenishment> sp_Trace_replenishment { get; set; }

        public virtual DbSet<_Prepare_Imports_step> _Prepare_Imports_step { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false);

                var configuration = builder.Build();

                var connectionString = configuration.GetConnectionString("Transfer_ConnectionString").ToString();

                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
