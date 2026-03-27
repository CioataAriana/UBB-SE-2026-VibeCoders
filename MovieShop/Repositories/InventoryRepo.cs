using Microsoft.Data.SqlClient;
using MovieShop.Models;
using System;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public class InventoryRepo
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        public List<Movie> GetOwnedMovies(int userId)
        {
            var list = new List<Movie>();
            const string sql = @"SELECT m.ID, m.Title, m.Description, m.Price, m.ImageUrl FROM OwnedMovies om JOIN Movies m ON m.ID = om.MovieID WHERE om.UserID = @uid";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(sql, _db.Connection);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Movie
                    {
                        ID = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Price = reader.GetDecimal(3),
                        ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }

        public void RemoveOwnedMovie(int userId, int movieId)
        {
            if (userId <= 0) return;

            _db.OpenConnection();
            using var tx = _db.Connection.BeginTransaction();
            try
            {
                const string del = "DELETE FROM OwnedMovies WHERE UserID = @uid AND MovieID = @mid";
                using (var cmd = new SqlCommand(del, _db.Connection, tx))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    cmd.ExecuteNonQuery();
                }

                const string insertTx = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress)
                                          VALUES (@buyerID, NULL, NULL, @movieID, NULL, @amount, @type, @status, @timestamp, NULL)";
                using (var cmd = new SqlCommand(insertTx, _db.Connection, tx))
                {
                    cmd.Parameters.AddWithValue("@buyerID", userId);
                    cmd.Parameters.AddWithValue("@movieID", movieId);
                    cmd.Parameters.AddWithValue("@amount", 0); 
                    cmd.Parameters.AddWithValue("@type", "RemoveOwnedMovie");
                    cmd.Parameters.AddWithValue("@status", "Completed");
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            finally
            {
                _db.CloseConnection();
            }
        }

        public void RemoveOwnedTicket(int userId, int eventId)
        {
            if (userId <= 0) return;

            _db.OpenConnection();
            using var tx = _db.Connection.BeginTransaction();
            try
            {
                const string del = "DELETE FROM OwnedTickets WHERE UserID = @uid AND EventID = @eid";
                using (var cmd = new SqlCommand(del, _db.Connection, tx))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@eid", eventId);
                    cmd.ExecuteNonQuery();
                }

                const string insertTx = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress)
                                          VALUES (@buyerID, NULL, NULL, NULL, @eventID, @amount, @type, @status, @timestamp, NULL)";
                using (var cmd = new SqlCommand(insertTx, _db.Connection, tx))
                {
                    cmd.Parameters.AddWithValue("@buyerID", userId);
                    cmd.Parameters.AddWithValue("@eventID", eventId);
                    cmd.Parameters.AddWithValue("@amount", 0);
                    cmd.Parameters.AddWithValue("@type", "RemoveOwnedTicket");
                    cmd.Parameters.AddWithValue("@status", "Completed");
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            finally
            {
                _db.CloseConnection();
            }
        }

        public List<MovieEvent> GetOwnedTickets(int userId)
        {
            var list = new List<MovieEvent>();
            const string sql = @"SELECT e.ID, e.MovieID, e.Title, e.Description, e.Date, e.Location, e.TicketPrice, e.PosterUrl FROM OwnedTickets ot JOIN Events e ON e.ID = ot.EventID WHERE ot.UserID = @uid";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(sql, _db.Connection);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MovieEvent
                    {
                        ID = reader.GetInt32(0),
                        MovieID = reader.GetInt32(1),
                        Title = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Date = reader.GetDateTime(4),
                        Location = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        TicketPrice = reader.GetDecimal(6),
                        PosterUrl = reader.IsDBNull(7) ? "" : reader.GetString(7)
                    });
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }

        public List<Equipment> GetOwnedEquipment(int userId)
        {
            var list = new List<Equipment>();
            const string sql = @"SELECT eq.ID, eq.SellerID, eq.Title, eq.Category, eq.Description, eq.Condition, eq.Price, eq.ImageUrl, eq.Status FROM Transactions t JOIN Equipment eq ON t.EquipmentID = eq.ID WHERE t.BuyerID = @uid";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(sql, _db.Connection);
                cmd.Parameters.AddWithValue("@uid", userId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Equipment
                    {
                        ID = reader.GetInt32(0),
                        SellerID = reader.GetInt32(1),
                        Title = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Condition = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Price = reader.GetDecimal(6),
                        ImageUrl = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Status = reader.IsDBNull(8) ? EquipmentStatus.Available : Enum.TryParse<EquipmentStatus>(reader.GetString(8), out var st) ? st : EquipmentStatus.Available
                    });
                }
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }
    }
}
