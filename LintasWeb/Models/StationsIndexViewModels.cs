﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LintasMVC.Models
{
    public class StationsIndexViewModels
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Countries { get; set; }
        public string Address { get; set; }        
    }
}