using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public enum OrderStatusEnum : byte
    {
        Ordered,
        [Display(Name = "Waiting Payment")]
        WaitingPayment,
        [Display(Name = "Payment Completed")]
        PaymentCompleted,
        Shipping,
        Completed
    }
}