using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class LabelViewModels
    {
        public string NoShipping { get; set; }
        public ShippingsModels Shippings { get; set; }
        public ShippingItemsModels ShippingItems { get; set; }
        public StationsModels OriginStation { get; set; }
        public StationsModels DestinationStation { get; set; }
    }
}