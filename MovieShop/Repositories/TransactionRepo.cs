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
        DatabaseSingleton _db = DatabaseSingleton.Instance;

        public void LogTransaction(Transaction transaction)
        {
            string query = @"INSERT INTO Transactions(BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress)
                            VALUES (@buyerID, @sellerID, @equipID, @movieID, @eventID, @amount, @type, @status, @timestamp, @address)";

            using (SqlCommand cmd = new SqlCommand(query, _db.Connection))
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

                _db.OpenConnection();
                cmd.ExecuteNonQuery();
                _db.CloseConnection();
            }
        }
    }
}
