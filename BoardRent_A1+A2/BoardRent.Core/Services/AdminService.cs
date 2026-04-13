namespace BoardRent.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Repositories;
    using BoardRent.Utils;

    public class AdminService : IAdminService
    {
        private readonly IUserRepository userRepository;
        private readonly IFailedLoginRepository failedLoginRepository;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly ISessionContext sessionContext;

        public AdminService(
            IUserRepository userRepository,
            IFailedLoginRepository failedLoginRepository,
            IUnitOfWorkFactory unitOfWorkFactory,
            ISessionContext sessionContext)
        {
            this.userRepository = userRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.sessionContext = sessionContext;
        }

        public async Task<ServiceResult<PaginatedResult<UserProfileDataTransferObject>>> GetAllUsersAsync(
            int pageNumber,
            int pageSize)
        {
            if (!this.IsCurrentUserAdministrator())
            {
                return ServiceResult<PaginatedResult<UserProfileDataTransferObject>>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                List<User> userEntities = await this.userRepository.GetPageAsync(pageNumber, pageSize);
                int totalUserCount = await this.userRepository.GetTotalCountAsync();

                var userProfileDtos = new List<UserProfileDataTransferObject>();

                foreach (User userEntity in userEntities)
                {
                    Role primaryRole = userEntity.Roles?.FirstOrDefault();
                    FailedLoginAttempt failedLoginRecord =
                        await this.failedLoginRepository.GetByUserIdAsync(userEntity.Id);

                    bool accountIsLocked = failedLoginRecord != null
                        && failedLoginRecord.LockedUntil.HasValue
                        && failedLoginRecord.LockedUntil.Value > DateTime.UtcNow;

                    userProfileDtos.Add(new UserProfileDataTransferObject
                    {
                        Id = userEntity.Id,
                        Username = userEntity.Username,
                        DisplayName = userEntity.DisplayName,
                        Email = userEntity.Email,
                        PhoneNumber = userEntity.PhoneNumber,
                        AvatarUrl = userEntity.AvatarUrl,
                        Role = new RoleDataTransferObject
                        {
                            Id = primaryRole?.Id ?? Guid.Empty,
                            Name = primaryRole?.Name ?? "Standard User"
                        },
                        IsSuspended = userEntity.IsSuspended,
                        IsLocked = accountIsLocked,
                        Country = userEntity.Country,
                        City = userEntity.City,
                        StreetName = userEntity.StreetName,
                        StreetNumber = userEntity.StreetNumber
                    });
                }

                var paginatedResult = new PaginatedResult<UserProfileDataTransferObject>
                {
                    Items = userProfileDtos,
                    TotalItemCount = totalUserCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return ServiceResult<PaginatedResult<UserProfileDataTransferObject>>.Ok(paginatedResult);
            }
        }

        public async Task<ServiceResult<bool>> SuspendUserAsync(Guid userIdentifier)
        {
            if (!this.IsCurrentUserAdministrator())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByIdentifierAsync(userIdentifier);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                userEntity.IsSuspended = true;
                userEntity.UpdatedAt = DateTime.UtcNow;
                await this.userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnsuspendUserAsync(Guid userIdentifier)
        {
            if (!this.IsCurrentUserAdministrator())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByIdentifierAsync(userIdentifier);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                userEntity.IsSuspended = false;
                userEntity.UpdatedAt = DateTime.UtcNow;
                await this.userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid userIdentifier, string newPassword)
        {
            if (!this.IsCurrentUserAdministrator())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            const int MinimumPasswordLength = 6;
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < MinimumPasswordLength)
            {
                return ServiceResult<bool>.Fail($"Password must be at least {MinimumPasswordLength} characters long.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                User userEntity = await this.userRepository.GetByIdentifierAsync(userIdentifier);
                if (userEntity == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                userEntity.PasswordHash = PasswordHasher.HashPassword(newPassword);
                userEntity.UpdatedAt = DateTime.UtcNow;
                await this.userRepository.UpdateAsync(userEntity);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid userIdentifier)
        {
            if (!this.IsCurrentUserAdministrator())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (IUnitOfWork unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                await this.failedLoginRepository.ResetAsync(userIdentifier);

                return ServiceResult<bool>.Ok(true);
            }
        }

        // ── Private helpers ──────────────────────────────────────────────────────
        private bool IsCurrentUserAdministrator()
        {
            return this.sessionContext.IsLoggedIn
                && this.sessionContext.Role == "Administrator";
        }
    }
}