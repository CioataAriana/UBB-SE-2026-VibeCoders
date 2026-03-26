using Microsoft.Data.SqlClient;
using MovieShop.Models;
using System;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public sealed class EventRepo
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        public List<MovieEvent> GetEventsForMovie(int movieId)
        {
            var list = new List<MovieEvent>();
            const string query = @"
SELECT ID, MovieID, Title, Description, Date, Location, TicketPrice, PosterUrl
FROM Events
WHERE MovieID = @mid
ORDER BY Date ASC, ID ASC";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, _db.Connection);
                cmd.Parameters.AddWithValue("@mid", movieId);
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

        public List<MovieEvent> GetAllEvents()
        {
            var list = new List<MovieEvent>();
            const string query = @"
SELECT ID, MovieID, Title, Description, Date, Location, TicketPrice, PosterUrl
FROM Events
ORDER BY Date ASC, ID ASC";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, _db.Connection);
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

        public MovieEvent? GetEventById(int eventId)
        {
            const string query = @"SELECT ID, MovieID, Title, Description, Date, Location, TicketPrice, PosterUrl FROM Events WHERE ID = @id";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, _db.Connection);
                cmd.Parameters.AddWithValue("@id", eventId);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;

                return new MovieEvent
                {
                    ID = reader.GetInt32(0),
                    MovieID = reader.GetInt32(1),
                    Title = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Date = reader.GetDateTime(4),
                    Location = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    TicketPrice = reader.GetDecimal(6),
                    PosterUrl = reader.IsDBNull(7) ? "" : reader.GetString(7)
                };
            }
            finally
            {
                _db.CloseConnection();
            }
        }

        public void PurchaseTicket(int userId, int eventId)
        {
            if (userId <= 0)
                throw new InvalidOperationException("You must be logged in to purchase.");

            _db.OpenConnection();
            using var sqlTrans = _db.Connection.BeginTransaction();
            try
            {
                const string checkOwned = @"SELECT COUNT(*) FROM OwnedTickets WHERE UserID = @uid AND EventID = @eid";
                using (var cmd = new SqlCommand(checkOwned, _db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@eid", eventId);
                    var ownedCount = (int)cmd.ExecuteScalar()!;
                    if (ownedCount > 0)
                        throw new InvalidOperationException("You already own a ticket for this event.");
                }

                decimal price;
                const string getPrice = @"SELECT TicketPrice FROM Events WHERE ID = @eid";
                using (var cmd = new SqlCommand(getPrice, _db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@eid", eventId);
                    var pr = cmd.ExecuteScalar();
                    if (pr == null || pr == DBNull.Value) throw new InvalidOperationException("Event not found.");
                    price = Convert.ToDecimal(pr);
                }
                
                const string deductSql = @"UPDATE Users SET Balance = Balance - @price WHERE ID = @uid AND Balance >= @price";
                int updated;
                using (var cmd = new SqlCommand(deductSql, _db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    updated = cmd.ExecuteNonQuery();
                }

                if (updated == 0)
                    throw new InvalidOperationException("Insufficient balance.");

                const string insertOwned = @"INSERT INTO OwnedTickets (UserID, EventID) VALUES (@uid, @eid)";
                using (var cmd = new SqlCommand(insertOwned, _db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@eid", eventId);
                    cmd.ExecuteNonQuery();
                }

                const string insertTx = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress) VALUES (@buyerID, NULL, NULL, NULL, @eventID, @amount, @type, @status, @timestamp, NULL)";
                using (var cmd = new SqlCommand(insertTx, _db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@buyerID", userId);
                    cmd.Parameters.AddWithValue("@eventID", eventId);
                    cmd.Parameters.AddWithValue("@amount", -price);
                    cmd.Parameters.AddWithValue("@type", "EventTicket");
                    cmd.Parameters.AddWithValue("@status", "Completed");
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                sqlTrans.Commit();
            }
            catch
            {
                sqlTrans.Rollback();
                throw;
            }
            finally
            {
                _db.CloseConnection();
            }
        }
    }
}

