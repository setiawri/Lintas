using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Shippings")]
    public class ShippingsModels
    {
        [Key]
        public Guid Id { get; set; }

        [Display(Name = "Customer")]
        public Guid Customers_Id { get; set; }

        [Required]
        public string No { get; set; }
        public DateTime Timestamp { get; set; }

        [Display(Name = "Origin")]
        public Guid Origin_Stations_Id { get; set; }

        [Display(Name = "Destination")]
        public Guid Destination_Stations_Id { get; set; }

        public string Company { get; set; }

        [Required]
        [Display(Name = "Address 1")]
        public string Address { get; set; }

        [Display(Name = "Address 2")]
        public string Address2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        public string Country { get; set; }

        [Display(Name = "Country Code")]
        public string CountryCode { get; set; }

        [Display(Name = "Mobile")]
        public string Phone1 { get; set; }

        [Display(Name = "Phone")]
        public string Phone2 { get; set; }

        public string Fax { get; set; }
        public string Email { get; set; }

        [Display(Name = "Tax Number")]
        public string TaxNumber { get; set; }

        public string Notes { get; set; }
        public ShippingStatusEnum Status_enumid { get; set; }
    }

    public class ShippingsViewModels
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string No { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Notes { get; set; }

        [Display(Name = "Status")]
        public ShippingStatusEnum Status_enumid { get; set; }
    }

    public class ShippingItemDetails
    {
        public string id { get; set; }
        public string no { get; set; }
        public string desc { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int weight { get; set; }
        public string notes { get; set; }
    }
}