using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("Shipments")]
    public class ShipmentsModels
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid Forwarders_Id { get; set; }
        public string Notes { get; set; }
    }

    public class ShipmentsIndexViewModels
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Forwarders { get; set; }
        public string Notes { get; set; }
    }
}