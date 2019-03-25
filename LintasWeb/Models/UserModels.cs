using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("AspNetUsers")]
    public class UserModels
    {
        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Fullname { get; set; }
        public Guid? Stations_Id { get; set; }
        public string Notes { get; set; }
    }

    [Table("AspNetRoles")]
    public class RoleModels
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [Table("AspNetUserRoles")]
    public class UserRoleModels
    {
        [Key]
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}