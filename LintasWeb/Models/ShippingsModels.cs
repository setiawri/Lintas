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

        [Required]
        public string Address { get; set; }
        public string Notes { get; set; }
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
    }

    public class ShippingItemDetails
    {
        public string id { get; set; }
        public string no { get; set; }
        public int length { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int weight { get; set; }
        public string notes { get; set; }
    }
}