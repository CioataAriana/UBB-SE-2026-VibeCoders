using Microsoft.Data.SqlClient;
using System;

namespace MovieShop.Repositories
{
    public class UserRepo
    {
        private readonly DatabaseSingleton _db = DatabaseSingleton.Instance;

        public decimal GetBalance(int userId)
        {
            if (userId <= 0)
                return 0m;

            const string query = "SELECT Balance FROM Users WHERE ID = @id";

            _db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, _db.Connection);
                cmd.Parameters.AddWithValue("@id", userId);
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
            }
            finally
            {
                _db.CloseConnection();
            }
        }
    }
}
