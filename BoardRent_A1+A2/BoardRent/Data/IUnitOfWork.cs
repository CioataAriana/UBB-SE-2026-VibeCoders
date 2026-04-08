using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace BoardRent.Data
{
    public interface IUnitOfWork : IDisposable
    {
        SqlConnection Connection { get; }
        Task OpenAsync();
    }
}
