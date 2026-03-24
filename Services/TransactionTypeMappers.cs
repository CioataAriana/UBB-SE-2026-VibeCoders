using System;

namespace MovieShop.Services
{
    public static class TransactionTypeMapper
    {
        // --- Type Mapping ---
        public static string ToDisplayString(string type)
        {
            return type switch
            {
                "MoviePurchase" => "🎬 Movie Purchase",
                "TicketPurchase" => "🎟️ Ticket Purchase",
                "EquipmentPurchase" => "🎥 Equipment Purchase",
                "TopUp" => "💳 Top-Up",
                "EquipmentSale" => "💰 Equipment Sale",
                _ => "Unknown Transaction"
            };
        }

        // --- Status Mapping ---
        public static string StatusToDisplayString(string status)
        {
            return status switch
            {
                "Pending" => "⏳ Pending",
                "Completed" => "✅ Completed",
                "Failed" => "❌ Failed",
                _ => "Unknown Status"
            };
        }

        // --- Amount Formatting ---
        public static string FormatAmount(decimal amount)
        {
            return amount >= 0
                ? $"+${amount:0.00}"
                : $"-${Math.Abs(amount):0.00}";
        }
    }
}