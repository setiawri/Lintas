using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class ShippingDocumentsViewModels
    {
        public Guid Id { get; set; }
        public string No { get; set; }
        public List<FileUploadsModels> ListFileUploads { get; set; }
    }
}