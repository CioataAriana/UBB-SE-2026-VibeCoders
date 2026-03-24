using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.Models
{
    internal class Equipment
    {
        public int ID { get; set; }
        public User? Seller { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; }
    }
}
