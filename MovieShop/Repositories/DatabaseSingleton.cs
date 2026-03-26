using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.Repositories
{
    public sealed class DatabaseSingleton
    {
        private static DatabaseSingleton? _instance;
        private readonly SqlConnection _connection;

        public static DatabaseSingleton Instance => _instance ??= new DatabaseSingleton();

        public SqlConnection Connection => _connection;

        private DatabaseSingleton() => _connection = new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={Helpers.GetProjectDirectory()}\\MovieShopDB.mdf;Integrated Security=True");

        public void OpenConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open) return;
            _connection.Open();
        }

        public void CloseConnection() => _connection.Close();
    }
}
