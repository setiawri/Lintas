using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class ShipmentsViewModels
    {
        public ShipmentsModels Shipments { get; set; }
        public ForwardersModels Forwarders { get; set; }
        public List<ShippingItemsModels> ListItems { get; set; }
    }
}