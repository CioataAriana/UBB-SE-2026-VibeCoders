using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    internal class EquipmentRepo
    {
        private string connString = "SERVER = S24B\\SQLEXPRESS; Database = MovieShopDB; Trusted_Connection = True; TrustServerCertificate = True";

        public void ListItem(Equipment item)
        {
            string query = @"INSERT INTO Equipment (SellerID, Title, Category, Description, Condition, Price, ImageUrl, Status)
                           VALUES (@sellerID, @title, @category, @description, @condition, @price, @img, 'Available')";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@sellerID", item.Seller.ID);
                    cmd.Parameters.AddWithValue("@title", item.Title);
                    cmd.Parameters.AddWithValue("@category", item.Category);
                    cmd.Parameters.AddWithValue("@description", item.Description);
                    cmd.Parameters.AddWithValue("@condition", item.Condition);
                    cmd.Parameters.AddWithValue("@price", item.Price);
                    // Use DBNull if no image URL is provided
                    cmd.Parameters.AddWithValue("@img", (object)item.ImageUrl ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
