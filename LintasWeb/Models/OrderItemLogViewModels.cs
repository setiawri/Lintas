using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class OrderItemLogViewModels
    {
        public OrderItemsModels OrderItem { get; set; }
        public List<OrderItemLogModels> ListOrderItemLog { get; set; }
    }
}