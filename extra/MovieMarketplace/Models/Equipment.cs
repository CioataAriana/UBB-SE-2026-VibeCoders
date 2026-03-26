namespace MovieMarketplace.Models
{
    public enum EquipmentStatus { Available, Sold, Pending }

    public class Equipment
    {
        public int ID { get; set; }
        public int SellerID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    }
}