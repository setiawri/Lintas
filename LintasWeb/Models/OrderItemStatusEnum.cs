using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public enum OrderItemStatusEnum : byte
    {
        Pending,
        Purchased,
        Received,
        Conflict
    }
}