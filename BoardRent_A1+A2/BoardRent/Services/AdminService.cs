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

        public AdminService(IUserRepository userRepository, IFailedLoginRepository failedLoginRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.userRepository = userRepository;
            this.failedLoginRepository = failedLoginRepository;
            this.unitOfWorkFactory = unitOfWorkFactory;
        }

        private bool IsAuthorized()
        {
            var session = SessionContext.GetInstance();
            return session.IsLoggedIn && session.Role == "Administrator";
        }

        public async Task<ServiceResult<List<UserProfileDataTransferObject>>> GetAllUsersAsync(int page, int pageSize)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<List<UserProfileDataTransferObject>>.Fail("Unauthorized access.");
            }

            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                var users = await this.userRepository.GetAllAsync(page, pageSize);

                var dtos = new List<UserProfileDataTransferObject>();
                foreach (var userEntity in users)
                {
                    var firstRole = userEntity.Roles?.FirstOrDefault();
                    var failedAttempt = await this.failedLoginRepository.GetByUserIdAsync(userEntity.Id);
                    bool isLocked = failedAttempt != null
                        && failedAttempt.LockedUntil.HasValue
                        && failedAttempt.LockedUntil.Value > DateTime.UtcNow;

                    dtos.Add(new UserProfileDataTransferObject
                    {
                        Id = userEntity.Id,
                        Username = userEntity.Username,
                        DisplayName = userEntity.DisplayName,
                        Email = userEntity.Email,
                        PhoneNumber = userEntity.PhoneNumber,
                        AvatarUrl = userEntity.AvatarUrl,
                        Role = new RoleDataTransferObject
                        {
                            Id = firstRole?.Id ?? Guid.Empty,
                            Name = firstRole?.Name ?? "Standard User"
                        },
                        IsSuspended = userEntity.IsSuspended,
                        IsLocked = isLocked,
                        Country = userEntity.Country,
                        City = userEntity.City,
                        StreetName = userEntity.StreetName,
                        StreetNumber = userEntity.StreetNumber
                    });
                }

                return ServiceResult<List<UserProfileDataTransferObject>>.Ok(dtos);
            }
        }

        public async Task<ServiceResult<bool>> SuspendUserAsync(Guid userId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                var user = await this.userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                user.IsSuspended = true;
                await this.userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnsuspendUserAsync(Guid userId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                var user = await this.userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                user.IsSuspended = false;
                await this.userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid userId, string newPassword)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return ServiceResult<bool>.Fail("Password must be at least 6 characters long.");
            }

            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.userRepository.SetUnitOfWork(unitOfWork);

                var user = await this.userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<bool>.Fail("User not found.");
                }

                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                await this.userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid userId)
        {
            if (!this.IsAuthorized())
            {
                return ServiceResult<bool>.Fail("Unauthorized access.");
            }

            using (var unitOfWork = this.unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                this.failedLoginRepository.SetUnitOfWork(unitOfWork);

                await this.failedLoginRepository.ResetAsync(userId);

                return ServiceResult<bool>.Ok(true);
            }
        }
    }
}