using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class ReviewSummary
    {
        public int Count { get; set; }
        /// <summary>Average of StarRating from Reviews, rounded to one decimal (AwayFromZero).</summary>
        public double AverageRounded { get; set; }
        /// <summary>Counts for stars 5 down to 1; index 0 = 5 stars.</summary>
        public int[] StarCounts { get; set; } = new int[5];
    }

    public class ReviewService
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        /// <summary>If this movie has no reviews yet, inserts two sample rows so the shop UI can show a real average and list.</summary>
        public void EnsureTwoSampleReviewsIfEmpty(int movieId)
        {
            const string countSql = "SELECT COUNT(*) FROM Reviews WHERE MovieID = @mid";
            const string insertSql = @"
INSERT INTO Reviews (MovieID, UserID, StarRating, Comment, CreatedAt)
VALUES (@mid, @uid, @stars, @comment, @dt)";

            _db.OpenConnection();
            try
            {
                int count;
                using (var cmd = new SqlCommand(countSql, _db.Connection))
                {
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    count = (int)cmd.ExecuteScalar()!;
                }

                if (count > 0)
                    return;

                int? firstUser = null;
                int? secondUser = null;
                using (var cmd = new SqlCommand("SELECT TOP 1 ID FROM Users ORDER BY ID", _db.Connection))
                {
                    var o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value)
                        firstUser = Convert.ToInt32(o);
                }

                if (firstUser == null)
                    return;

                using (var cmd = new SqlCommand("SELECT TOP 1 ID FROM Users WHERE ID <> @u ORDER BY ID", _db.Connection))
                {
                    cmd.Parameters.AddWithValue("@u", firstUser.Value);
                    var o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value)
                        secondUser = Convert.ToInt32(o);
                }

                int reviewerB = secondUser ?? firstUser.Value;

                void Insert(int userId, int stars, string comment, DateTime created)
                {
                    using var cmd = new SqlCommand(insertSql, _db.Connection);
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@stars", stars);
                    cmd.Parameters.AddWithValue("@comment", comment);
                    cmd.Parameters.AddWithValue("@dt", created);
                    cmd.ExecuteNonQuery();
                }

                Insert(firstUser.Value, 5, "Really enjoyed this — great pacing and acting.", DateTime.Now.AddDays(-2));
                Insert(reviewerB, 4, "Solid watch. Would recommend.", DateTime.Now.AddDays(-1));
            }
            finally
            {
                _db.CloseConnection();
            }
        }

        public ReviewSummary GetSummaryForMovie(int movieId)
        {
            var summary = new ReviewSummary();
            const string avgSql = @"
SELECT COUNT(*), AVG(CAST(StarRating AS FLOAT))
FROM Reviews WHERE MovieID = @mid";
            const string distSql = @"
SELECT StarRating, COUNT(*)
FROM Reviews WHERE MovieID = @mid
GROUP BY StarRating";

            _db.OpenConnection();
            try
            {
                using (var cmd = new SqlCommand(avgSql, _db.Connection))
                {
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        summary.Count = r.GetInt32(0);
                        if (summary.Count > 0 && !r.IsDBNull(1))
                        {
                            var raw = r.GetDouble(1);
                            summary.AverageRounded = Math.Round(raw, 1, MidpointRounding.AwayFromZero);
                        }
                    }
                }

                using (var cmd = new SqlCommand(distSql, _db.Connection))
                {
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        int stars = r.GetInt32(0);
                        int c = r.GetInt32(1);
                        if (stars >= 1 && stars <= 5)
                            summary.StarCounts[5 - stars] = c;
                    }
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return summary;
        }

        public List<ReviewDisplayItem> GetReviewsForMovie(int movieId)
        {
            var list = new List<ReviewDisplayItem>();
            const string sql = @"
SELECT r.StarRating, r.Comment, r.CreatedAt, u.Username
FROM Reviews r
INNER JOIN Users u ON u.ID = r.UserID
WHERE r.MovieID = @mid
ORDER BY r.CreatedAt DESC";
            using var cmd = new SqlCommand(sql, _db.Connection);
            cmd.Parameters.AddWithValue("@mid", movieId);
            _db.OpenConnection();
            try
            {
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new ReviewDisplayItem
                    {
                        StarRating = r.GetInt32(0),
                        Comment = r.IsDBNull(1) ? null : r.GetString(1),
                        CreatedAt = r.GetDateTime(2),
                        UserDisplayName = r.GetString(3)
                    });
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }

        public static string BuildStarDistributionTooltip(ReviewSummary summary)
        {
            if (summary.Count == 0)
                return "No reviews yet.";
            return $"5★: {summary.StarCounts[0]}\n4★: {summary.StarCounts[1]}\n3★: {summary.StarCounts[2]}\n2★: {summary.StarCounts[3]}\n1★: {summary.StarCounts[4]}";
        }
    }
}
