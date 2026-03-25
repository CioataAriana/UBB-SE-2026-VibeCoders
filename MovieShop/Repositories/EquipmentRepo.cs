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
        DatabaseSingleton _db = DatabaseSingleton.Instance;

        public void ListItem(Equipment item)
        {
            string query = @"INSERT INTO Equipment (SellerID, Title, Category, Description, Condition, Price, ImageUrl, Status)
                           VALUES (@sellerID, @title, @category, @description, @condition, @price, @img, 'Available')";

            using (SqlCommand cmd = new SqlCommand(query, _db.Connection))
            {
                cmd.Parameters.AddWithValue("@sellerID", item.Seller.ID);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@category", item.Category);
                cmd.Parameters.AddWithValue("@description", item.Description);
                cmd.Parameters.AddWithValue("@condition", item.Condition);
                cmd.Parameters.AddWithValue("@price", item.Price);
                // Use DBNull if no image URL is provided
                cmd.Parameters.AddWithValue("@img", (object)item.ImageUrl ?? DBNull.Value);

                _db.OpenConnection();
                cmd.ExecuteNonQuery();
                _db.CloseConnection();
            }
        }
    }
}
