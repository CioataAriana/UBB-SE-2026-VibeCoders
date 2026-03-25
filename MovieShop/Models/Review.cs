using System;

namespace MovieShop.Models
{
    public class Review
    {
        public int ID { get; set; }
        public int MovieID { get; set; }
        public int UserID { get; set; }
        public int StarRating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ReviewDisplayItem
    {
        public string UserDisplayName { get; set; } = "";
        public int StarRating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>UI row for review list (ItemsControl binding).</summary>
    public class ReviewListRow
    {
        public string UserDisplayName { get; set; } = "";
        public int StarRating { get; set; }
        public string CommentLine { get; set; } = "";
        public string CreatedAtDisplay { get; set; } = "";
    }
}
