using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Stations")]
    public class StationsModels
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(2)]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Countries")]
        public Guid Countries_Id { get; set; }

        [Required]
        [Display(Name = "Time Zone")]
        public string TimeZone { get; set; }

        public string Address { get; set; }

        [Display(Name = "Phone 1")]
        public string Phone1 { get; set; }

        [Display(Name = "Phone 2")]
        public string Phone2 { get; set; }

        public string Notes { get; set; }
    }
}