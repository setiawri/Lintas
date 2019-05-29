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
        [Display(Name = "Forwarders")]
        public Guid Forwarders_Id { get; set; }
        public string Notes { get; set; }
        [Display(Name = "Status")]
        public ShipmentItemStatusEnum Status_enumid { get; set; }
    }

    public class ShipmentsIndexViewModels
    {
        public Guid Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime Timestamp { get; set; }
        public string Forwarders { get; set; }
        public string Notes { get; set; }
        [Display(Name = "Status")]
        public ShipmentItemStatusEnum Status_enumid { get; set; }
    }
}