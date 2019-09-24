using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("CustomerForms")]
    public class CustomerFormsModels
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string Customer_FirstName { get; set; }
        public string Customer_MiddleName { get; set; }
        [Required]
        public string Customer_LastName { get; set; }
        public string Customer_Company { get; set; }
        [Required]
        public string Customer_Address { get; set; }
        public string Customer_Address2 { get; set; }
        [Required]
        public string Customer_City { get; set; }
        [Required]
        public string Customer_State { get; set; }
        public Guid Customer_Countries_Id { get; set; }
        public string Customer_PostalCode { get; set; }
        [Required]
        public string Customer_Phone1 { get; set; }
        public string Customer_Phone2 { get; set; }
        public string Customer_Fax { get; set; }
        public string Customer_Email { get; set; }
        public string Customer_Notes { get; set; }
        public Guid Shipping_Origin_Stations_Id { get; set; }
        public Guid Shipping_Destination_Stations_Id { get; set; }
        [Required]
        public string Shipping_ReceiverName { get; set; }
        public string Shipping_Company { get; set; }
        [Required]
        public string Shipping_Address { get; set; }
        public string Shipping_Address2 { get; set; }
        [Required]
        public string Shipping_City { get; set; }
        [Required]
        public string Shipping_State { get; set; }
        [Required]
        public string Shipping_Country { get; set; }
        [Required]
        public string Shipping_PostalCode { get; set; }
        [Required]
        public string Shipping_Phone1 { get; set; }
        public string Shipping_Phone2 { get; set; }
        public string Shipping_Fax { get; set; }
        public string Shipping_Email { get; set; }
        public string Shipping_TaxNumber { get; set; }
        public string Shipping_Notes { get; set; }
        public Guid? Shippings_Id { get; set; }
        public CustomerFormsStatusEnum Status { get; set; }
    }
}