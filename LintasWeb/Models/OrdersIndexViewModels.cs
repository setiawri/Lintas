using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class OrdersIndexViewModels
    {
        public Guid Id { get; set; }
        public string Service { get; set; }
        public string No { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp { get; set; }
        public string Customer { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Notes { get; set; }
        public bool Pending { get; set; }
        public OrderStatusEnum Status { get; set; }
    }
}