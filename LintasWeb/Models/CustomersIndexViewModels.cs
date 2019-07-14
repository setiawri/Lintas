using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class CustomersIndexViewModels
    {
        public Guid Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        public string Address { get; set; }
        
        public string City { get; set; }

        public string Countries { get; set; }
    }
}