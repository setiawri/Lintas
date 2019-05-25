using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("ShipmentLog")]
    public class ShipmentLogModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Shipments_Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public string UserAccounts_Id { get; set; }
    }
}