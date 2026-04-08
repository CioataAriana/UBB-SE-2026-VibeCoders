using BoardRent.Data;
using BoardRent.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoardRent.Repositories
{
    public class UserRepository : IUserRepository
    {
        private IUnitOfWork _unitOfWork;

        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private SqlConnection Connection => _unitOfWork.Connection;

        public async Task<User> GetByIdAsync(Guid id)
        {
            User user = null;

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM [User] WHERE Id = @Id";
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            if (user != null)
            {
                user.Roles = await LoadRolesAsync(user.Id);
            }

            return user;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            User user = null;

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM [User] WHERE Username = @Username";
                command.Parameters.AddWithValue("@Username", username);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            if (user != null)
            {
                user.Roles = await LoadRolesAsync(user.Id);
            }

            return user;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            User user = null;

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM [User] WHERE Email = @Email";
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user = MapUser(reader);
                    }
                }
            }

            if (user != null)
            {
                user.Roles = await LoadRolesAsync(user.Id);
            }

            return user;
        }

        public async Task<List<User>> GetAllAsync(int page, int pageSize)
        {
            var users = new List<User>();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM [User] ORDER BY CreatedAt OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(MapUser(reader));
                    }
                }
            }

            foreach (var user in users)
            {
                user.Roles = await LoadRolesAsync(user.Id);
            }

            return users;
        }

        public async Task AddAsync(User user)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO [User] (Id, Username, DisplayName, Email, PasswordHash, PhoneNumber, AvatarUrl, IsSuspended, CreatedAt, UpdatedAt, StreetName, StreetNumber, Country, City)
                    VALUES (@Id, @Username, @DisplayName, @Email, @PasswordHash, @PhoneNumber, @AvatarUrl, @IsSuspended, @CreatedAt, @UpdatedAt, @StreetName, @StreetNumber, @Country, @City)";

                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AvatarUrl", user.AvatarUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsSuspended", user.IsSuspended);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);
                command.Parameters.AddWithValue("@StreetName", user.StreetName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StreetNumber", user.StreetNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Country", user.Country ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@City", user.City ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(User user)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE [User] SET
                        DisplayName = @DisplayName,
                        Email = @Email,
                        PasswordHash = @PasswordHash,
                        PhoneNumber = @PhoneNumber,
                        AvatarUrl = @AvatarUrl,
                        IsSuspended = @IsSuspended,
                        UpdatedAt = @UpdatedAt,
                        StreetName = @StreetName,
                        StreetNumber = @StreetNumber,
                        Country = @Country,
                        City = @City
                    WHERE Id = @Id";

                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@DisplayName", user.DisplayName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AvatarUrl", user.AvatarUrl ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IsSuspended", user.IsSuspended);
                command.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);
                command.Parameters.AddWithValue("@StreetName", user.StreetName ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StreetNumber", user.StreetNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Country", user.Country ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@City", user.City ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddRoleAsync(Guid userId, string roleName)
        {
            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                    DECLARE @RoleId UNIQUEIDENTIFIER = (SELECT Id FROM Role WHERE Name = @RoleName);
                    IF @RoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
                        INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId);";
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@RoleName", roleName);

                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task<List<Role>> LoadRolesAsync(Guid userId)
        {
            var roles = new List<Role>();

            using (var command = Connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT r.Id, r.Name
                    FROM Role r
                    INNER JOIN UserRoles ur ON ur.RoleId = r.Id
                    WHERE ur.UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roles.Add(new Role
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }
                }
            }

            return roles;
        }

        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl")),
                IsSuspended = reader.GetBoolean(reader.GetOrdinal("IsSuspended")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                StreetName = reader.IsDBNull(reader.GetOrdinal("StreetName")) ? null : reader.GetString(reader.GetOrdinal("StreetName")),
                StreetNumber = reader.IsDBNull(reader.GetOrdinal("StreetNumber")) ? null : reader.GetString(reader.GetOrdinal("StreetNumber")),
                Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                Roles = new List<Role>()
            };
        }
    }
}
