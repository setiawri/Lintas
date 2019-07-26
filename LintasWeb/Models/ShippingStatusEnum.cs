using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public enum ShippingStatusEnum : byte
    {
        Cancelled,
        Shipping,
        WaitingPayment,
        Documents,
        OnShipments,
        OnDelivery,
        ShipmentComplete,
        Completed
    }
}