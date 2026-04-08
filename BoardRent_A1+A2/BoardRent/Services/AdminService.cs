using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardRent.Data;
using BoardRent.Domain;
using BoardRent.DTOs;
using BoardRent.Repositories;
using BoardRent.Utils;

namespace BoardRent.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFailedLoginRepository _failedLoginRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public AdminService(IUserRepository userRepository, IFailedLoginRepository failedLoginRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _userRepository = userRepository;
            _failedLoginRepository = failedLoginRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        private bool IsAuthorized()
        {
            var session = SessionContext.GetInstance();
            return session.IsLoggedIn && session.Role == "Administrator";
        }

        public async Task<ServiceResult<List<UserProfileDto>>> GetAllUsersAsync(int page, int pageSize)
        {
            if (!IsAuthorized())
                return ServiceResult<List<UserProfileDto>>.Fail("Unauthorized access.");

            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);
                ((FailedLoginRepository)_failedLoginRepository).SetUnitOfWork(uow);

                var users = await _userRepository.GetAllAsync(page, pageSize);

                var dtos = new List<UserProfileDto>();
                foreach (var u in users)
                {
                    var firstRole = u.Roles?.FirstOrDefault();
                    var failedAttempt = await _failedLoginRepository.GetByUserIdAsync(u.Id);
                    bool isLocked = failedAttempt != null
                        && failedAttempt.LockedUntil.HasValue
                        && failedAttempt.LockedUntil.Value > DateTime.UtcNow;

                    dtos.Add(new UserProfileDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        DisplayName = u.DisplayName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        AvatarUrl = u.AvatarUrl,
                        Role = new RoleDto
                        {
                            Id = firstRole?.Id ?? Guid.Empty,
                            Name = firstRole?.Name ?? "Standard User"
                        },
                        IsSuspended = u.IsSuspended,
                        IsLocked = isLocked,
                        Country = u.Country,
                        City = u.City,
                        StreetName = u.StreetName,
                        StreetNumber = u.StreetNumber
                    });
                }

                return ServiceResult<List<UserProfileDto>>.Ok(dtos);
            }
        }

        public async Task<ServiceResult<bool>> SuspendUserAsync(Guid userId)
        {
            if (!IsAuthorized())
                return ServiceResult<bool>.Fail("Unauthorized access.");

            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Fail("User not found.");

                user.IsSuspended = true;
                await _userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnsuspendUserAsync(Guid userId)
        {
            if (!IsAuthorized())
                return ServiceResult<bool>.Fail("Unauthorized access.");

            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Fail("User not found.");

                user.IsSuspended = false;
                await _userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ResetPasswordAsync(Guid userId, string newPassword)
        {
            if (!IsAuthorized())
                return ServiceResult<bool>.Fail("Unauthorized access.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return ServiceResult<bool>.Fail("Password must be at least 6 characters long.");

            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Fail("User not found.");

                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                await _userRepository.UpdateAsync(user);

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> UnlockAccountAsync(Guid userId)
        {
            if (!IsAuthorized())
                return ServiceResult<bool>.Fail("Unauthorized access.");

            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((FailedLoginRepository)_failedLoginRepository).SetUnitOfWork(uow);

                await _failedLoginRepository.ResetAsync(userId);

                return ServiceResult<bool>.Ok(true);
            }
        }
    }
}
