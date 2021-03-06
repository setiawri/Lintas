﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("ShippingItems")]
    public class ShippingItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? Shippings_Id { get; set; }

        [Required]
        public string No { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Notes { get; set; }

        [Display(Name = "Status")]
        public ShippingItemStatusEnum Status_enumid { get; set; }
        public bool Invoiced { get; set; }
        public Guid? Shipments_Id { get; set; }

        [Display(Name = "Delivery Status")]
        public DeliveryItemStatusEnum? Delivery_Status { get; set; }
        public string TrackingNo { get; set; }
        public string Description { get; set; }
        public decimal? DeclaredPrice { get; set; }
        public string CourierInfo { get; set; }
    }
}