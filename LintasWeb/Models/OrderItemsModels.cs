﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("OrderItems")]
    public class OrderItemsModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Orders_Id { get; set; }
        public int RowNo { get; set; }

        [Required]
        public string Description { get; set; }
        public int Qty { get; set; }

        [Display(Name = "Received")]
        public int ReceivedQty { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? PurchaseTimestamp { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime? ReceiveTimestamp { get; set; }

        [Display(Name = "Status")]
        public OrderItemStatusEnum Status_enumid { get; set; }
        public bool Invoiced { get; set; }
        public string TrackingNo { get; set; }
    }

    public class OrderItemDetails
    {
        public string desc { get; set; }
        public int qty { get; set; }
        public decimal cost { get; set; }
        public string note { get; set; }
    }
}