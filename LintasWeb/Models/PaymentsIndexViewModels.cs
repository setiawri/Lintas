using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class PaymentsIndexViewModels
    {
        public Guid Id { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime Timestamp { get; set; }

        [Display(Name = "Invoice No.")]
        public string InvoiceNo { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Amount { get; set; }
        public string Info { get; set; }
        public string Notes { get; set; }
    }
}