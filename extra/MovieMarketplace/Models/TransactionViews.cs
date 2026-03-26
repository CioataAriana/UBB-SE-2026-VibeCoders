using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieMarketplace.Models
{
    public class TransactionView
    {
        public int ID { get; set; }
        public string EquipmentTitle { get; set; } = "";
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public string ShippingAddress { get; set; } = "";
        public DateTime TransactionDate { get; set; }

        public Microsoft.UI.Xaml.Visibility IsPendingVisibility
        {
            get
            {
                if (!string.IsNullOrEmpty(Status) && Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    return Microsoft.UI.Xaml.Visibility.Visible;
                }
                return Microsoft.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}
