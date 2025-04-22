﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyecto.Models
{
    public class PagedResult <T>
    {
        public List<T> Items { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalRecords { get; set; }
        
    }
}
