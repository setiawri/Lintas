using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Forwarders")]
    public class ForwardersModels
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Notes { get; set; }
        [Display(Name = "Billing Code")]
        public string BillingCode { get; set; }
        [Display(Name = "Billing Account")]
        public string BillingAccount { get; set; }
        public bool Active { get; set; }
    }
}