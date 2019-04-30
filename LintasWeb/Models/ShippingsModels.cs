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

        [Required]
        public string No { get; set; }
        public DateTime Timestamp { get; set; }

        [Display(Name = "Origin")]
        public Guid Origin_Stations_Id { get; set; }

        [Display(Name = "Destination")]
        public Guid Destination_Stations_Id { get; set; }
        public string Notes { get; set; }
    }

    public class ShippingsViewModels
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string No { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime Timestamp { get; set; }
        
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Notes { get; set; }
    }
}