using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class LintasContext : DbContext
    {
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
    }
}