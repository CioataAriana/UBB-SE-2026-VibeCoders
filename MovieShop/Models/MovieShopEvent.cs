using System;

namespace MovieShop.Models
{
    public class MovieShopEvent
    {
        public int ID { get; set; }
        public int MovieID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public string Location { get; set; } = "";
        public decimal TicketPrice { get; set; }
        public string PosterUrl { get; set; } = "";
    }
}
