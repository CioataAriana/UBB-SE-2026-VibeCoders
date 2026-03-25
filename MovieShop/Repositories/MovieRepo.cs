using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public class MovieRepo
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        public List<Movie> GetAllMovies()
        {
            var list = new List<Movie>();
            const string query = "SELECT ID, Title, Description, Rating, Price, ImageUrl FROM Movies ORDER BY Title";
            using var cmd = new SqlCommand(query, _db.Connection);
            try
            {
                _db.OpenConnection();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Movie
                    {
                        ID = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        Rating = reader.GetDouble(3),
                        Price = reader.GetDecimal(4),
                        ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MovieRepo.GetAllMovies: " + ex.Message);
                throw;
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }

        public List<Movie> GetMoviesWithActiveSale()
        {
            var list = new List<Movie>();
            const string query = @"
SELECT DISTINCT m.ID, m.Title, m.Description, m.Rating, m.Price, m.ImageUrl
FROM Movies m
INNER JOIN ActiveSales s ON s.MovieID = m.ID
WHERE s.StartTime <= GETDATE() AND s.EndTime > GETDATE()
ORDER BY m.Title";
            using var cmd = new SqlCommand(query, _db.Connection);
            try
            {
                _db.OpenConnection();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Movie
                    {
                        ID = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Description = reader.GetString(2),
                        Rating = reader.GetDouble(3),
                        Price = reader.GetDecimal(4),
                        ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MovieRepo.GetMoviesWithActiveSale: " + ex.Message);
                throw;
            }
            finally
            {
                _db.CloseConnection();
            }

            return list;
        }

        public Movie? GetById(int id)
        {
            const string query = "SELECT ID, Title, Description, Rating, Price, ImageUrl FROM Movies WHERE ID = @id";
            using var cmd = new SqlCommand(query, _db.Connection);
            cmd.Parameters.AddWithValue("@id", id);
            try
            {
                _db.OpenConnection();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;
                return new Movie
                {
                    ID = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    Rating = reader.GetDouble(3),
                    Price = reader.GetDecimal(4),
                    ImageUrl = reader.IsDBNull(5) ? null : reader.GetString(5)
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MovieRepo.GetById: " + ex.Message);
                throw;
            }
            finally
            {
                _db.CloseConnection();
            }
        }
    }
}
