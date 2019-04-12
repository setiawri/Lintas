using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Invoices")]
    public class InvoicesModels
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Order")]
        public Guid Orders_Id { get; set; }
        public string No { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }

        [Required]
        public string Notes { get; set; }
    }
}
