using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proyecto.Models
{
    public class PagedFoodResult
    {
        public IEnumerable<Food> Foods { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
