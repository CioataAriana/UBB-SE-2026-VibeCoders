using BoardRent.Domain;
using System;
using System.Threading.Tasks;

namespace BoardRent.Repositories
{
    public interface IFailedLoginRepository
    {
        Task<FailedLoginAttempt?> GetByUserIdAsync(Guid userId);
        Task IncrementAsync(Guid userId);
        Task ResetAsync(Guid userId);
    }
}
