using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("OrderItems")]
    public class OrderItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Orders_Id { get; set; }

        [Required]
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }

    public class OrderItemDetails
    {
        public string desc { get; set; }
        public decimal cost { get; set; }
        public string note { get; set; }
    }
}