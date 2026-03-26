using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public class EquipmentRepo
    {
        DatabaseSingleton _db = DatabaseSingleton.Instance;

        public List<Equipment> FetchAvailableEquipment()
        {
            var items = new List<Equipment>();

            string query = "SELECT ID, SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition FROM Equipment WHERE Status = 'Available'";
            SqlCommand cmd = new SqlCommand(query, _db.Connection);

            try
            {
                _db.OpenConnection();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new Equipment
                        {
                            ID = reader.GetInt32(0),
                            SellerID = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            Status = EquipmentStatus.Available,
                            Description = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ImageUrl = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            Category = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            Condition = reader.IsDBNull(8) ? "" : reader.GetString(8)
                        });
                    }
                }
                _db.CloseConnection();
            }
            catch (Exception ex)
            {
                _db.CloseConnection();
                Debug.WriteLine("Fetch error: " + ex.Message);
                throw;
            }

            return items;
        }

        public void ListItem(Equipment item)
        {
            string query = @"INSERT INTO Equipment (SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition) 
                            VALUES (@seller, @title, @price, 'Available', @desc, @img, @cat, @cond)";

            SqlCommand cmd = new SqlCommand(query, _db.Connection);
            cmd.Parameters.AddWithValue("@seller", item.SellerID);
            cmd.Parameters.AddWithValue("@title", item.Title);
            cmd.Parameters.AddWithValue("@price", item.Price);
            cmd.Parameters.AddWithValue("@cat", item.Category ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@cond", item.Condition ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(item.Description) ? (object)DBNull.Value : item.Description);
            cmd.Parameters.AddWithValue("@img", string.IsNullOrEmpty(item.ImageUrl) ? (object)DBNull.Value : item.ImageUrl);

            _db.OpenConnection();
            cmd.ExecuteNonQuery();
            _db.CloseConnection();
        }

        public void PurchaseEquipment(int equipmentId, int buyerId, decimal price, string address)
        {
            _db.OpenConnection();
            SqlTransaction sqlTrans = _db.Connection.BeginTransaction();

            try
            {
                // 1. Deduct buyer balance
                string deductSql = "UPDATE Users SET Balance = Balance - @price WHERE ID = @bid";
                SqlCommand cmd1 = new SqlCommand(deductSql, _db.Connection, sqlTrans);
                cmd1.Parameters.AddWithValue("@price", price);
                cmd1.Parameters.AddWithValue("@bid", buyerId);
                cmd1.ExecuteNonQuery();

                // 2. Mark equipment as sold
                string updateEquip = "UPDATE Equipment SET Status = 'Sold' WHERE ID = @eid";
                SqlCommand cmd2 = new SqlCommand(updateEquip, _db.Connection, sqlTrans);
                cmd2.Parameters.AddWithValue("@eid", equipmentId);
                cmd2.ExecuteNonQuery();

                // 3. Log transaction — store amount as negative so the wallet shows it as a debit (-)
                string logTrans = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, Amount, Status, ShippingAddress, Type, Timestamp) 
                                    SELECT @bid, SellerID, ID, @amount, 'Completed', @addr, 'EquipmentPurchase', GETDATE()
                                    FROM Equipment WHERE ID = @eid";

                SqlCommand cmd3 = new SqlCommand(logTrans, _db.Connection, sqlTrans);
                cmd3.Parameters.AddWithValue("@bid", buyerId);
                cmd3.Parameters.AddWithValue("@amount", -price);
                cmd3.Parameters.AddWithValue("@addr", address);
                cmd3.Parameters.AddWithValue("@eid", equipmentId);
                cmd3.ExecuteNonQuery();

                sqlTrans.Commit();
                _db.CloseConnection();
            }
            catch (Exception ex)
            {
                sqlTrans.Rollback();
                _db.CloseConnection();
                Debug.WriteLine("Failed transaction: " + ex.Message);
                throw;
            }
        }
    }
}