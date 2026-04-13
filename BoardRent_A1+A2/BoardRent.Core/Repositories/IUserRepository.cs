namespace BoardRent.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.Domain;

    public interface IUserRepository
    {
        void SetUnitOfWork(IUnitOfWork unitOfWork);

        Task<User> GetByIdentifierAsync(Guid identifier);

        Task<User> GetByUsernameAsync(string username);

        Task<User> GetByEmailAsync(string emailAddress);

        Task<List<User>> GetPageAsync(int pageNumber, int pageSize);

        Task<int> GetTotalCountAsync();

        Task AddAsync(User userEntity);

        Task UpdateAsync(User userEntity);

        Task AddRoleAsync(Guid userIdentifier, string roleName);
    }
}