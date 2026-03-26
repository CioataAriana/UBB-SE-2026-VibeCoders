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

        public decimal? ActiveSaleDiscountPercent { get; set; }

        public bool HasActiveSale => ActiveSaleDiscountPercent is decimal d && d > 0;

        public decimal GetEffectivePrice() => HasActiveSale ? decimal.Round(Price * (1 - ActiveSaleDiscountPercent!.Value / 100m), 2, MidpointRounding.AwayFromZero)
                                                            : Price;

        public decimal GetDiscountedPrice(double discountPercentage) => Price * (1 - (decimal)(discountPercentage / 100.0));

        public string OriginalPriceText => Price.ToString("0.00");

        public string DiscountedPriceText => GetEffectivePrice().ToString("0.00");
    }
}
