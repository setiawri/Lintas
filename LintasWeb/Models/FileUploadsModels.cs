using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    [Table("FileUploads")]
    public class FileUploadsModels  
    {
        [Key]
        public Guid Id { get; set; }
        public Guid Ref_Id { get; set; }
        public string OriginalFilename { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }
}