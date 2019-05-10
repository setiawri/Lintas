using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class ShippingServicesModels
    {
        public ShippingsModels Shipping { get; set; }
        public List<InvoicesModels> ListInvoice { get; set; }
        public List<PaymentsIndexViewModels> ListPayment { get; set; }
    }
}