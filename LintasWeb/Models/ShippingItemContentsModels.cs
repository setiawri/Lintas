using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("ShippingItemContents")]
    public class ShippingItemContentsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ShippingItems_Id { get; set; }
        public Guid OrderItems_Id { get; set; }
        public int Qty { get; set; }
        public string Notes { get; set; }
    }

    public class ShippingItemContentDetails
    {
        public Guid Id { get; set; }
        public int qty { get; set; }
        public string notes { get; set; }
    }
}