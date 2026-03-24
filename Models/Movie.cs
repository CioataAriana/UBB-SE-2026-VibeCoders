using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.Models
{
    public class Movie
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public bool IsOnSale { get; set; }

        public decimal GetDiscountedPrice(double discountPercentage)
        {
            return Price * (1 - (decimal)(discountPercentage / 100.0));
        }
    }
}
