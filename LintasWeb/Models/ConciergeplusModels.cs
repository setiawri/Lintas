using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class ConciergeplusModels
    {
        public OrdersModels Order { get; set; }
        public List<InvoicesModels> ListInvoice { get; set; }
        public List<PaymentsIndexViewModels> ListPayment { get; set; }
        public InvoicesModels Invoice { get; set; }
        public PaymentsModels Payment { get; set; }
        public List<OrderItemsModels> ListOrderItem { get; set; }
    }
}