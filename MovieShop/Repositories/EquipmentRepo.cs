using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public class EquipmentRepo
    {
        // Folosim LocalDB conform configurării tale
        private readonly string _connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=MovieShopDB;Trusted_Connection=True;TrustServerCertificate=True;";

        /// <summary>
        /// Aduce toate echipamentele disponibile.
        /// </summary>
        public List<Equipment> FetchAvailableEquipment()
        {
            var items = new List<Equipment>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT ID, SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition FROM Equipment WHERE Status = 'Available'";
                SqlCommand cmd = new SqlCommand(query, conn);

                try
                {
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
                                Status = EquipmentStatus.Available, // Setăm enum-ul corect
                                Description = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                ImageUrl = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Category = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                Condition = reader.IsDBNull(8) ? "" : reader.GetString(8)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Eroare la Fetch: " + ex.Message);
                    throw;
                }
            }
            return items;
        }

        /// <summary>
        /// Adaugă un echipament nou (Listare).
        /// </summary>
        public void ListItem(Equipment item)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Equipment (SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition) 
                                VALUES (@seller, @title, @price, 'Available', @desc, @img, @cat, @cond)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@seller", item.SellerID);
                cmd.Parameters.AddWithValue("@title", item.Title);
                cmd.Parameters.AddWithValue("@price", item.Price);
                cmd.Parameters.AddWithValue("@cat", item.Category ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@cond", item.Condition ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(item.Description) ? (object)DBNull.Value : item.Description);
                cmd.Parameters.AddWithValue("@img", string.IsNullOrEmpty(item.ImageUrl) ? (object)DBNull.Value : item.ImageUrl);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Tranzacție complexă: Scade banii, vinde produsul, loghează tranzacția.
        /// </summary>
        public void PurchaseEquipment(int equipmentId, int buyerId, decimal price, string address)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction sqlTrans = conn.BeginTransaction();

                try
                {
                    // 1. Scădem balanța cumpărătorului (Folosim coloana 'Balance' din clasa User)
                    string deductSql = "UPDATE Users SET Balance = Balance - @price WHERE ID = @bid";
                    SqlCommand cmd1 = new SqlCommand(deductSql, conn, sqlTrans);
                    cmd1.Parameters.AddWithValue("@price", price);
                    cmd1.Parameters.AddWithValue("@bid", buyerId);
                    cmd1.ExecuteNonQuery();

                    // 2. Marcăm produsul ca fiind vândut
                    string updateEquip = "UPDATE Equipment SET Status = 'Sold' WHERE ID = @eid";
                    SqlCommand cmd2 = new SqlCommand(updateEquip, conn, sqlTrans);
                    cmd2.Parameters.AddWithValue("@eid", equipmentId);
                    cmd2.ExecuteNonQuery();

                    // 3. Creăm înregistrarea în tabelul Transactions
                    // NOTA: Am adăugat SELECT-ul pentru a prelua automat SellerID-ul din tabelul Equipment
                    // 3. Creăm înregistrarea în tabelul Transactions
                    // Am adăugat coloanele 'Type' și 'Timestamp' (folosind funcția GETDATE())
                    string logTrans = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, Amount, Status, ShippingAddress, Type, Timestamp) 
                                    SELECT @bid, SellerID, ID, @price, 'Completed', @addr, 'Marketplace', GETDATE()
                                     FROM Equipment WHERE ID = @eid";

                    SqlCommand cmd3 = new SqlCommand(logTrans, conn, sqlTrans);
                    cmd3.Parameters.AddWithValue("@bid", buyerId);
                    cmd3.Parameters.AddWithValue("@price", price);
                    cmd3.Parameters.AddWithValue("@addr", address);
                    cmd3.Parameters.AddWithValue("@eid", equipmentId);
                    cmd3.ExecuteNonQuery();

                    sqlTrans.Commit();
                }
                catch (Exception ex)
                {
                    sqlTrans.Rollback();
                    System.Diagnostics.Debug.WriteLine("Tranzacție eșuată: " + ex.Message);
                    throw; // Aruncăm eroarea mai departe pentru a o afișa în UI
                }
            }
        }
    }
}