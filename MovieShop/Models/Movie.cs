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

        /// <summary>When set, an active sale applies this discount percent off <see cref="Price"/>.</summary>
        public decimal? ActiveSaleDiscountPercent { get; set; }

        public bool HasActiveSale =>
            ActiveSaleDiscountPercent is decimal d && d > 0;

        public decimal GetEffectivePrice()
        {
            if (!HasActiveSale)
                return Price;
            return decimal.Round(
                Price * (1 - ActiveSaleDiscountPercent!.Value / 100m),
                2,
                MidpointRounding.AwayFromZero);
        }

        public decimal GetDiscountedPrice(double discountPercentage)
        {
            return Price * (1 - (decimal)(discountPercentage / 100.0));
        }

        /// <summary>Plain numeric string for catalog price row (currency symbol added in XAML).</summary>
        public string CatalogStrikePriceText => Price.ToString("0.00");

        /// <summary>Numeric string for the price charged when a sale applies.</summary>
        public string CatalogEffectivePriceText => GetEffectivePrice().ToString("0.00");
    }
}
