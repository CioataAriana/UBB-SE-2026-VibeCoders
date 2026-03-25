using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

using MovieShop.Models;
using System.Diagnostics;

namespace MovieShop.Repositories
{
    internal class ActiveSalesRepo
    {
        DatabaseSingleton _db = DatabaseSingleton.Instance;

        public List<ActiveSale> GetCurrentSales()
        {
            List<ActiveSale> sales = new List<ActiveSale>();

            string query = @"SELECT s.ID, s.DiscountPercentage, s.EndTime, m.ID AS MovieID, m.Title, m.Price
                            FROM ActiveSales s
                            JOIN Movies m ON s.MovieID = m.ID
                            WHERE s.StartTime <= GETDATE() AND s.EndTime > GETDATE()
                            ORDER BY s.EndTime ASC";


            using (SqlCommand cmd = new SqlCommand(query, _db.Connection))
            {
                _db.OpenConnection();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    sales.Add(new ActiveSale
                    {
                        ID = (int)reader["ID"],
                        DiscountPercentage = (decimal)reader["DiscountPercentage"],
                        EndTime = (DateTime)reader["EndTime"],
                        Movie = new Movie
                        {
                            ID = (int)reader["MovieID"],
                            Title = reader["Title"].ToString(),
                            Price = (decimal)reader["Price"]
                        }
                    });
                }

                _db.CloseConnection();
            }

            return sales;
        }
    }
}
