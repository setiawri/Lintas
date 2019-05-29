using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public enum ShipmentItemStatusEnum : byte
    {
        Pending,
        [Display(Name = "In Transit")]
        InTransit,
        Conflict,
        Completed
    }
}