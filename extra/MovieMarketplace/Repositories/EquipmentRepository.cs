using Microsoft.Data.SqlClient;
using MovieMarketplace.Models;
using System;
using System.Collections.Generic;

namespace MovieMarketplace.Repositories
{
    public class EquipmentRepository
    {
        private readonly string _connectionString = @"Server=.\SQLEXPRESS;Database=MovieMarketplaceDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";

        public List<Equipment> FetchAvailableEquipment()
        {
            var items = new List<Equipment>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ID, SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition FROM Equipment WHERE Status = 'Available'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

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
            }
            return items;
        }

        public void AddEquipment(Equipment item)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Equipment (SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition) 
                                VALUES (@seller, @title, @price, @status, @desc, @img, @cat, @cond)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@seller", item.SellerID);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@price", item.Price);
                cmd.Parameters.AddWithValue("@status", item.Status.ToString());
                cmd.Parameters.AddWithValue("@cat", item.Category);
                cmd.Parameters.AddWithValue("@cond", item.Condition);
                cmd.Parameters.AddWithValue("@desc", item.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@img", item.ImageUrl ?? (object)DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<TransactionView> FetchUserOrders(int buyerId)
        {
            var orders = new List<TransactionView>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT t.ID, e.Title, t.Amount, t.Status, t.ShippingAddress, t.TransactionDate 
                         FROM Transactions t 
                         JOIN Equipment e ON t.EquipmentID = e.ID 
                         WHERE t.BuyerID = @bid 
                         ORDER BY t.TransactionDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@bid", buyerId);
                conn.Open();

                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        orders.Add(new TransactionView
                        {
                            ID = r.GetInt32(0),
                            EquipmentTitle = r.GetString(1),
                            Amount = r.GetDecimal(2),
                            Status = r.GetString(3),
                            ShippingAddress = r.GetString(4),
                            TransactionDate = r.GetDateTime(5)
                        });
                    }
                }
            }
            return orders;
        }

        public List<TransactionView> FetchSellerSales(int sellerId)
        {
            var sales = new List<TransactionView>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT t.ID, e.Title, t.Amount, t.Status, t.ShippingAddress, t.TransactionDate 
                         FROM Transactions t 
                         JOIN Equipment e ON t.EquipmentID = e.ID 
                         WHERE t.SellerID = @sid 
                         ORDER BY t.TransactionDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@sid", sellerId);
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        sales.Add(new TransactionView
                        {
                            ID = r.GetInt32(0),
                            EquipmentTitle = r.GetString(1),
                            Amount = r.GetDecimal(2),
                            Status = r.GetString(3),
                            ShippingAddress = r.GetString(4),
                            TransactionDate = r.GetDateTime(5)
                        });
                    }
                }
            }
            return sales;
        }

        public void PurchaseEquipment(int equipmentId, int buyerId, decimal price, string address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction sqlTrans = conn.BeginTransaction();

                try
                {
                    string deductSql = "UPDATE Users SET Balance = Balance - @price WHERE UserID = @bid";
                    SqlCommand cmd1 = new SqlCommand(deductSql, conn, sqlTrans);
                    cmd1.Parameters.AddWithValue("@price", price);
                    cmd1.Parameters.AddWithValue("@bid", buyerId);
                    cmd1.ExecuteNonQuery();

                    string updateEquip = "UPDATE Equipment SET Status = 'Sold' WHERE ID = @eid";
                    SqlCommand cmd2 = new SqlCommand(updateEquip, conn, sqlTrans);
                    cmd2.Parameters.AddWithValue("@eid", equipmentId);
                    cmd2.ExecuteNonQuery();

                    string logTrans = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, Amount, Status, ShippingAddress) 
                               SELECT @bid, SellerID, ID, @price, 'Pending', @addr 
                               FROM Equipment WHERE ID = @eid";
                    SqlCommand cmd3 = new SqlCommand(logTrans, conn, sqlTrans);
                    cmd3.Parameters.AddWithValue("@bid", buyerId);
                    cmd3.Parameters.AddWithValue("@price", price);
                    cmd3.Parameters.AddWithValue("@addr", address);
                    cmd3.Parameters.AddWithValue("@eid", equipmentId);
                    cmd3.ExecuteNonQuery();

                    sqlTrans.Commit();
                }
                catch (Exception)
                {
                    sqlTrans.Rollback();
                    throw;
                }
            }
        }

        public void ConfirmDelivery(int transactionId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string payoutSql = @"
                    UPDATE Users 
                    SET Balance = Balance + (SELECT Amount FROM Transactions WHERE ID = @tid)
                    WHERE UserID = (SELECT SellerID FROM Transactions WHERE ID = @tid)
                    AND EXISTS (SELECT 1 FROM Transactions WHERE ID = @tid AND Status = 'Pending')";

                        using (SqlCommand cmd1 = new SqlCommand(payoutSql, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@tid", transactionId);
                            cmd1.ExecuteNonQuery();
                        }

                        string completeSql = "UPDATE Transactions SET Status = 'Completed' WHERE ID = @tid AND Status = 'Pending'";
                        using (SqlCommand cmd2 = new SqlCommand(completeSql, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@tid", transactionId);
                            int rowsAffected = cmd2.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                throw new Exception("Transaction is already completed or does not exist.");
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}