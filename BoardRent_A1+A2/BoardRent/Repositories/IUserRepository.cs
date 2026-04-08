using BoardRent.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoardRent.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync(int page, int pageSize);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task AddRoleAsync(Guid userId, string roleName);
    }
}
