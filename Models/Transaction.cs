using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.Models
{
    public class Transaction
    {
        public int ID { get; set; }

        public User BuyerID { get; set; }

        public User? SellerID { get; set; }

        public int? EquipmentID { get; set; }
        public int? MovieID { get; set; }
        public int? EventID { get; set; }

        public decimal Amount { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ShippingAddress { get; set; }

    }
}