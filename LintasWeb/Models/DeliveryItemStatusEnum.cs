using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public enum DeliveryItemStatusEnum : byte
    {
        Pending,
        [Display(Name = "Waiting for Pickup")]
        PickupWaiting,
        [Display(Name = "Local Delivery")]
        LocalDelivery,
        Conflict,
        Completed
    }
}