namespace BoardRent.Repositories
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.Domain;
    using Microsoft.Data.SqlClient;
    public class FailedLoginRepository : IFailedLoginRepository
    {
        private const int MaximumFailedAttempts = 5;
        private const int LockoutDurationMinutes = 15;
        private IUnitOfWork unitOfWork;
        private SqlConnection Connection => this.unitOfWork.Connection;
        public void SetUnitOfWork(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<FailedLoginAttempt?> GetByUserIdAsync(Guid userId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM FailedLoginAttempt WHERE UserId = @UserId";
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return this.MapFailedLoginAttempt(reader);
                    }
                }
            }

            return null;
        }
        public async Task IncrementAsync(Guid userId)
        {
            FailedLoginAttempt currentAttempt = await this.GetByUserIdAsync(userId);
            int nextFailedAttempts = (currentAttempt?.FailedAttempts ?? 0) + 1;
            DateTime? nextLockedUntil = nextFailedAttempts >= MaximumFailedAttempts
                ? DateTime.UtcNow.AddMinutes(LockoutDurationMinutes)
                : null;

            if (currentAttempt == null)
            {
                using (var insertCommand = this.Connection.CreateCommand())
                {
                    insertCommand.CommandText = @"
                        INSERT INTO FailedLoginAttempt (UserId, FailedAttempts, LockedUntil)
                        VALUES (@UserId, @FailedAttempts, @LockedUntil)";
                    insertCommand.Parameters.AddWithValue("@UserId", userId);
                    insertCommand.Parameters.AddWithValue("@FailedAttempts", nextFailedAttempts);
                    insertCommand.Parameters.AddWithValue("@LockedUntil", nextLockedUntil ?? (object)DBNull.Value);
                    await insertCommand.ExecuteNonQueryAsync();
                }
                return;
            }

            using (var updateCommand = this.Connection.CreateCommand())
            {
                updateCommand.CommandText = @"
                    UPDATE FailedLoginAttempt
                    SET FailedAttempts = @FailedAttempts, LockedUntil = @LockedUntil
                    WHERE UserId = @UserId";
                updateCommand.Parameters.AddWithValue("@UserId", userId);
                updateCommand.Parameters.AddWithValue("@FailedAttempts", nextFailedAttempts);
                updateCommand.Parameters.AddWithValue("@LockedUntil", nextLockedUntil ?? (object)DBNull.Value);
                await updateCommand.ExecuteNonQueryAsync();
            }
        }
        public async Task ResetAsync(Guid userId)
        {
            using (var command = this.Connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE FailedLoginAttempt
                    SET FailedAttempts = @FailedAttempts, LockedUntil = @LockedUntil
                    WHERE UserId = @UserId";

                command.Parameters.AddWithValue("@FailedAttempts", 0);
                command.Parameters.AddWithValue("@LockedUntil", DBNull.Value);
                command.Parameters.AddWithValue("@UserId", userId);

                await command.ExecuteNonQueryAsync();
            }
        }
        private FailedLoginAttempt MapFailedLoginAttempt(SqlDataReader reader)
        {
            return new FailedLoginAttempt
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                FailedAttempts = reader.GetInt32(reader.GetOrdinal("FailedAttempts")),
                LockedUntil = reader.IsDBNull(reader.GetOrdinal("LockedUntil")) ? null : reader.GetDateTime(reader.GetOrdinal("LockedUntil"))
            };
        }
    }
}