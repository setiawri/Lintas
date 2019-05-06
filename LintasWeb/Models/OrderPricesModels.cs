using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("OrderPrices")]
    public class OrderPricesModels
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
    }
}