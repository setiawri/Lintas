using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Orders")]
    public class OrdersModels
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string No { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp { get; set; }

        [Display(Name = "Customer")]
        public Guid Customers_Id { get; set; }

        [Display(Name = "Origin")]
        public Guid Origin_Stations_Id { get; set; }

        [Display(Name = "Destination")]
        public Guid Destination_Stations_Id { get; set; }

        [Required]
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Company { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }

        [Display(Name = "Tax Number")]
        public string TaxNumber { get; set; }

        public string Notes { get; set; }
        public OrderStatusEnum Status_enumid { get; set; }
    }
}