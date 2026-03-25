using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MovieShop.Models;

namespace MovieShop.Repositories
{
    internal class TransactionRepo
    {
        private string connString = @"Server=(localdb)\MSSQLLocalDB;Database=MovieShopDB;Trusted_Connection=True;TrustServerCertificate=True;";
        public void LogTransaction(Transaction transaction)
        {
            string query = @"INSERT INTO Transactions(BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress)
                            VALUES (@buyerID, @sellerID, @equipID, @movieID, @eventID, @amount, @type, @status, @timestamp, @address)";

            // 'using' automatically opens and closes the data conn

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@buyerID", transaction.BuyerID.ID);
                    cmd.Parameters.AddWithValue("@sellerID", (object)transaction.SellerID?.ID ?? DBNull.Value); // Seller might be null for store buys
                    cmd.Parameters.AddWithValue("@equipID", (object)transaction.EquipmentID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@movieID", (object)transaction.MovieID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@eventID", (object)transaction.EventID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@amount", transaction.Amount);
                    cmd.Parameters.AddWithValue("@type", transaction.Type);
                    cmd.Parameters.AddWithValue("@status", transaction.Status);
                    cmd.Parameters.AddWithValue("@timestamp", transaction.Timestamp);
                    cmd.Parameters.AddWithValue("@address", (object)transaction.ShippingAddress ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

            }
        }
        public List<Transaction> GetTransactionsByUserId(int userId)
        {
            var list = new List<Transaction>();
            string query = "SELECT Timestamp, Type, Status, Amount FROM Transactions WHERE BuyerID = @uid OR SellerID = @uid ORDER BY Timestamp DESC";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Transaction
                            {
                                Timestamp = reader.GetDateTime(0),
                                Type = reader.GetString(1),
                                Status = reader.GetString(2),
                                Amount = reader.GetDecimal(3)
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}
