using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Tracking")]
    public class TrackingModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Ref_Id { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Description { get; set; }
    }
}