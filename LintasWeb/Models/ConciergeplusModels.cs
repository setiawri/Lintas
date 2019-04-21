using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class ConciergeplusModels
    {
        public OrdersModels Order { get; set; }
        public InvoicesModels Invoice { get; set; }
        public PaymentsModels Payment { get; set; }
    }
}