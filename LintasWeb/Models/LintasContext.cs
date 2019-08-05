using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LintasMVC.Models
{
    public class LintasContext : DbContext
    {
        public LintasContext()
            : base("LintasContext")
        {
        }

        public DbSet<ActivityLogsModels> ActivityLogs { get; set; }
        public DbSet<UserModels> User { get; set; }
        public DbSet<RoleModels> Role { get; set; }
        public DbSet<UserRoleModels> UserRole { get; set; }
        public DbSet<CountriesModels> Countries { get; set; }
        public DbSet<CustomersModels> Customers { get; set; }
        public DbSet<StationsModels> Stations { get; set; }
        public DbSet<OrdersModels> Orders { get; set; }
        public DbSet<OrderItemsModels> OrderItems { get; set; }
        public DbSet<InvoicesModels> Invoices { get; set; }
        public DbSet<InvoiceItemsModels> InvoiceItems { get; set; }
        public DbSet<PaymentsModels> Payments { get; set; }
        public DbSet<OrderItemLogModels> OrderItemLog { get; set; }
        public DbSet<ShippingItemsModels> ShippingItems { get; set; }
        public DbSet<ShippingItemContentsModels> ShippingItemContents { get; set; }
        public DbSet<ShippingsModels> Shippings { get; set; }
        public DbSet<OrderPricesModels> OrderPrices { get; set; }
        public DbSet<ShippingPricesModels> ShippingPrices { get; set; }
        public DbSet<FileUploadsModels> FileUploads { get; set; }
        public DbSet<ForwardersModels> Forwarders { get; set; }
        public DbSet<ShipmentsModels> Shipments { get; set; }
        public DbSet<ShipmentLogModels> ShipmentLog { get; set; }
        public DbSet<DeliveryLogModels> DeliveryLog { get; set; }
        public DbSet<TrackingModels> Tracking { get; set; }
        public DbSet<ShipmentsReportModels> ShipmentsReport { get; set; }

        #region Activity Log
        public override int SaveChanges()
        {
            LintasContext db = new LintasContext();
            string userId = db.User.Where(x => x.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().Id;
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (ActivityLogsModels x in GetAuditRecordsForChange(ent, userId))
                {
                    this.ActivityLogs.Add(x);
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync()
        {
            LintasContext db = new LintasContext();
            string userId = db.User.Where(x => x.UserName == HttpContext.Current.User.Identity.Name).FirstOrDefault().Id;
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            foreach (var ent in this.ChangeTracker.Entries().Where(p => p.State == EntityState.Added || p.State == EntityState.Deleted || p.State == EntityState.Modified))
            {
                // For each changed record, get the audit record entries and add them
                foreach (ActivityLogsModels x in GetAuditRecordsForChange(ent, userId))
                {
                    this.ActivityLogs.Add(x);
                }
            }

            return await base.SaveChangesAsync();
        }

        private List<ActivityLogsModels> GetAuditRecordsForChange(DbEntityEntry dbEntry, string userId)
        {
            List<ActivityLogsModels> result = new List<ActivityLogsModels>();

            DateTime changeTime = DateTime.Now;

            // Get the Table() attribute, if one exists
            TableAttribute tableAttr = dbEntry.Entity.GetType().GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;

            // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
            string tableName = tableAttr != null ? tableAttr.Name : dbEntry.Entity.GetType().Name;

            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            string keyName = dbEntry.Entity.GetType().GetProperties().Single(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0).Name;

            if (dbEntry.State == EntityState.Added)
            {
                result.Add(new ActivityLogsModels()
                {
                    Id = Guid.NewGuid(),
                    Description = "Added", // Added
                    TableName = tableName,
                    RefId = new Guid(dbEntry.CurrentValues.GetValue<object>(keyName).ToString()),
                    UserAccounts_Id = userId,
                    Timestamp = changeTime
                });
            }
            else if (dbEntry.State == EntityState.Deleted)
            {
                result.Add(new ActivityLogsModels()
                {
                    Id = Guid.NewGuid(),
                    Description = "Deleted", // Deleted
                    TableName = tableName,
                    RefId = new Guid(dbEntry.CurrentValues.GetValue<object>(keyName).ToString()),
                    UserAccounts_Id = userId,
                    Timestamp = changeTime
                });
            }
            else if (dbEntry.State == EntityState.Modified)
            {
                result.Add(new ActivityLogsModels()
                {
                    Id = Guid.NewGuid(),
                    Description = "Modified", // Modified
                    TableName = tableName,
                    RefId = new Guid(dbEntry.OriginalValues.GetValue<object>(keyName).ToString()),
                    UserAccounts_Id = userId,
                    Timestamp = changeTime
                });
            }
            // Otherwise, don't do anything, we don't care about Unchanged or Detached entities

            return result;
        }
        #endregion
    }
}