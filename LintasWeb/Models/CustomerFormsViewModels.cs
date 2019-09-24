using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class CustomerFormsViewModels
    {
        public Guid Id { get; set; }
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }
        [Display(Name = "Receiver Name")]
        public string ReceiverName { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        [Display(Name = "Address")]
        public string ShippingAddress { get; set; }
        [Display(Name = "Mobile")]
        public string ShippingMobile { get; set; }
        public CustomerFormsStatusEnum Status { get; set; }
    }
}