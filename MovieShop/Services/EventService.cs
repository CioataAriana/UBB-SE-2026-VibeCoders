using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class EventService
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        public int CountUpcomingForMovie(int movieId)
        {
            const string sql = @"
SELECT COUNT(*) FROM Events
WHERE MovieID = @mid AND [Date] >= @from";
            using var cmd = new SqlCommand(sql, _db.Connection);
            cmd.Parameters.AddWithValue("@mid", movieId);
            cmd.Parameters.AddWithValue("@from", DateTime.Today);
            _db.OpenConnection();
            try
            {
                var n = (int)cmd.ExecuteScalar()!;
                return n;
            }
            finally
            {
                _db.CloseConnection();
            }
        }

        public List<MovieShopEvent> GetUpcomingForMovie(int movieId)
        {
            var list = new List<MovieShopEvent>();
            const string sql = @"
SELECT ID, MovieID, Title, Description, [Date], Location, TicketPrice, PosterUrl
FROM Events
WHERE MovieID = @mid AND [Date] >= @from
ORDER BY [Date]";
            using var cmd = new SqlCommand(sql, _db.Connection);
            cmd.Parameters.AddWithValue("@mid", movieId);
            cmd.Parameters.AddWithValue("@from", DateTime.Today);
            _db.OpenConnection();
            try
            {
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new MovieShopEvent
                    {
                        ID = r.GetInt32(0),
                        MovieID = r.GetInt32(1),
                        Title = r.GetString(2),
                        Description = r.GetString(3),
                        Date = r.GetDateTime(4),
                        Location = r.GetString(5),
                        TicketPrice = r.GetDecimal(6),
                        PosterUrl = r.GetString(7)
                    });
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }
    }
}
