using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class UserViewModels
    {
        public string Id { get; set; }

        [Display(Name = "Full Name")]
        public string Fullname { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }
        public string Role { get; set; }
        public string RoleId { get; set; }
        public string Notes { get; set; }
        public string Station { get; set; }

        [Display(Name = "Station")]
        public Guid? Stations_Id { get; set; }
    }
}