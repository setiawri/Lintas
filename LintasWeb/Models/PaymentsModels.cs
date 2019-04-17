using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Payments")]
    public class PaymentsModels
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Invoice")]
        public Guid Invoices_Id { get; set; }

        [Display(Name = "Date")]
        //[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Info")]
        public string PaymentInfo { get; set; }
        public string Notes { get; set; }
    }
}