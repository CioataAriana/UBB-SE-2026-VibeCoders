using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

using MovieShop.Models;

namespace MovieShop.Repositories
{
    internal class ActiveSalesRepo
    {
        private string connString = "SERVER = S24B\\SQLEXPRESS; Database = MovieShopDB; Trusted_Connection = True; TrustServerCertificate = True";

        public List<ActiveSale> GetCurrentSales()
        {
            List<ActiveSale> sales = new List<ActiveSale>();

            string query = @"SELECT s.ID, s.DiscountPercentage, s.EndTime, m.ID AS MovieID, m.Title, m.Price
                            FROM ActiveSales s
                            JOIN Movies m ON s.MovieID = m.ID
                            WHERE s.StartTime <= GETDATE() AND s.EndTime > GETDATE()
                            ORDER BY s.EndTime ASC";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();

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
                            Title = reader["Title"].ToString(),
                            Price = (decimal)reader["Price"]
                        }
                    });
                }
            }

            return sales;
        }
    }
}
