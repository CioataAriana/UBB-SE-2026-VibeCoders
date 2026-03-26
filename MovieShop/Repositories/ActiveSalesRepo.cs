using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

using MovieShop.Models;

namespace MovieShop.Repositories
{
    public class ActiveSalesRepo
    {
        DatabaseSingleton _db = DatabaseSingleton.Instance;

        /// <summary>
        /// For each movie, the largest discount among all currently active sales (best price for the customer).
        /// </summary>
        public Dictionary<int, decimal> GetBestDiscountPercentByMovieId()
        {
            var map = new Dictionary<int, decimal>();
            foreach (var sale in GetCurrentSales())
            {
                var id = sale.Movie.ID;
                var pct = sale.DiscountPercentage;
                if (!map.TryGetValue(id, out var existing) || pct > existing)
                    map[id] = pct;
            }

            return map;
        }

        public static void ApplyBestDiscountsToMovies(IReadOnlyList<Movie> movies, Dictionary<int, decimal> bestDiscountByMovieId)
        {
            foreach (var m in movies)
            {
                if (bestDiscountByMovieId.TryGetValue(m.ID, out var pct))
                    m.ActiveSaleDiscountPercent = pct;
                else
                    m.ActiveSaleDiscountPercent = null;
            }
        }

        public List<ActiveSale> GetCurrentSales()
        {
            List<ActiveSale> sales = new List<ActiveSale>();

            string query = @"SELECT s.ID, s.DiscountPercentage, s.EndTime, m.ID AS MovieID, m.Title, m.Price
                            FROM ActiveSales s
                            JOIN Movies m ON s.MovieID = m.ID
                            WHERE s.StartTime <= GETDATE() AND s.EndTime > GETDATE()
                            ORDER BY s.EndTime ASC";

            
            SqlCommand cmd = new SqlCommand(query, _db.Connection);
            _db.OpenConnection();

            SqlDataReader reader = cmd.ExecuteReader();

            while(reader.Read())
            {
                sales.Add(new ActiveSale
                {
                    ID = (int)reader["ID"],
                    DiscountPercentage = (decimal)reader["DiscountPercentage"],
                    EndTime = (DateTime)reader["EndTime"],
                    Movie = new Movie
                    {
                        ID = (int)reader["MovieID"],
                        Title = reader["Title"].ToString() ?? "<no title>",
                        Price = (decimal)reader["Price"]
                    }
                });
            }

            _db.CloseConnection();
            return sales;
        }
    }
}
