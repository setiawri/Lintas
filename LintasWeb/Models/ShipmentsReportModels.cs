using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("ShipmentsReport")]
    public class ShipmentsReportModels
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ShippingItems_Id { get; set; }
        public string WaybillNumber { get; set; }
        public string ServiceNumber { get; set; }
        public string ConversionNumber { get; set; }
        public string OriginCountry { get; set; }
        public decimal ParcelWeight { get; set; }
        public decimal ParcelLong { get; set; }
        public decimal ParcelWide { get; set; }
        public decimal ParcelHigh { get; set; }
        public decimal ParcelVolume { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = true)]
        public DateTime ConsignmentDate { get; set; }
        public string TaxConsigneeNumber { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsigneeCompany { get; set; }
        public string ConsigneePhone { get; set; }
        public string ConsigneeMobile { get; set; }
        public string ConsigneeFax { get; set; }
        public string ConsigneeEmail { get; set; }
        public string ConsigneePostalCode { get; set; }
        public string ConsigneeCountry { get; set; }
        public string ConsigneeCountryCode { get; set; }
        public string ConsigneeState { get; set; }
        public string ConsigneeCity { get; set; }
        public string ConsigneeAddress1 { get; set; }
        public string ConsigneeAddress2 { get; set; }
        public string ShipperName { get; set; }
        public string ShipperCompany { get; set; }
        public string ShipperPhone { get; set; }
        public string ShipperMobile { get; set; }
        public string ShipperFax { get; set; }
        public string ShipperEmail { get; set; }
        public string ShipperPostalCode { get; set; }
        public string ShipperCountry { get; set; }
        public string ShipperCountryCode { get; set; }
        public string ShipperState { get; set; }
        public string ShipperCity { get; set; }
        public string ShipperAddress1 { get; set; }
        public string ShipperAddress2 { get; set; }
        public int ParcelQty { get; set; }
        public int ProductQty { get; set; }
        public string ProductDescription { get; set; }
        public decimal DeclarationPrice { get; set; }
        public string Currency { get; set; }
        public string BillingCode { get; set; }
        public string BillingAccount { get; set; }
        public string BrokerName { get; set; }
        public string BrokerPhone { get; set; }
        public string HsCode { get; set; }
        public decimal FreightCost { get; set; }
        public decimal Insurance { get; set; }
        public string BagNo { get; set; }
        public string PaymentType { get; set; }
    }
}