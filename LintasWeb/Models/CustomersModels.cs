using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Customers")]
    public class CustomersModels
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Phone 1")]
        public string Phone1 { get; set; }

        [Display(Name = "Phone 2")]
        public string Phone2 { get; set; }

        [Display(Name = "Zip Code")]
        public string Zipcode { get; set; }

        [Required]
        [Display(Name = "Countries")]
        public Guid Countries_Id { get; set; }
        public string Notes { get; set; }
    }
}