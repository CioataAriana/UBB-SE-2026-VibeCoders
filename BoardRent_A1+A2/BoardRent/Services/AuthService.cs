using BoardRent.Data;
using BoardRent.Domain;
using BoardRent.DTOs;
using BoardRent.Repositories;
using BoardRent.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BoardRent.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFailedLoginRepository _failedLoginRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public AuthService(IUserRepository userRepository, IFailedLoginRepository failedLoginRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _userRepository = userRepository;
            _failedLoginRepository = failedLoginRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.DisplayName) || dto.DisplayName.Length < 2 || dto.DisplayName.Length > 50)
                errors.Add("DisplayName|Display name must be between 2 and 50 characters long.");

            if (string.IsNullOrWhiteSpace(dto.Username) || dto.Username.Length < 3 || dto.Username.Length > 30
                || !Regex.IsMatch(dto.Username, @"^[a-zA-Z0-9_]+$"))
                errors.Add("Username|Username must be 3–30 characters and contain only letters, numbers, and underscores.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add("Email|Email is required.");

            var (pwValid, pwError) = PasswordValidator.Validate(dto.Password);
            if (!pwValid)
                errors.Add($"Password|{pwError}");

            if (dto.Password != dto.ConfirmPassword)
                errors.Add("ConfirmPassword|Passwords do not match.");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                if (!Regex.IsMatch(dto.PhoneNumber, @"^\+?\d{7,15}$"))
                    errors.Add("PhoneNumber|Phone number format is invalid.");
            }

            if (errors.Any())
                return ServiceResult<bool>.Fail(string.Join(";", errors));

            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
                if (existingUsername != null)
                    return ServiceResult<bool>.Fail("Username|Username is already taken.");

                var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                    return ServiceResult<bool>.Fail("Email|Email is already registered.");

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    DisplayName = dto.DisplayName,
                    Username = dto.Username,
                    Email = dto.Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber,
                    Country = string.IsNullOrWhiteSpace(dto.Country) ? null : dto.Country,
                    City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City,
                    StreetName = string.IsNullOrWhiteSpace(dto.StreetName) ? null : dto.StreetName,
                    StreetNumber = string.IsNullOrWhiteSpace(dto.StreetNumber) ? null : dto.StreetNumber,
                    PasswordHash = PasswordHasher.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsSuspended = false
                };

                await _userRepository.AddAsync(newUser);
                await _userRepository.AddRoleAsync(newUser.Id, "Standard User");

                SessionContext.GetInstance().Populate(newUser, "Standard User");
            }

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<UserProfileDto>> LoginAsync(LoginDto dto)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);
                ((FailedLoginRepository)_failedLoginRepository).SetUnitOfWork(uow);

                User user = await _userRepository.GetByUsernameAsync(dto.UsernameOrEmail);
                if (user == null)
                {
                    user = await _userRepository.GetByEmailAsync(dto.UsernameOrEmail);
                }

                if (user == null)
                {
                    return ServiceResult<UserProfileDto>.Fail("Invalid username or password.");
                }

                if (user.IsSuspended)
                {
                    return ServiceResult<UserProfileDto>.Fail("This account has been suspended. Please contact support.");
                }

                var failedAttempt = await _failedLoginRepository.GetByUserIdAsync(user.Id);
                if (failedAttempt != null && failedAttempt.LockedUntil.HasValue)
                {
                    if (failedAttempt.LockedUntil.Value > DateTime.UtcNow)
                    {
                        var remaining = failedAttempt.LockedUntil.Value - DateTime.UtcNow;
                        int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
                        return ServiceResult<UserProfileDto>.Fail(
                            $"Account is locked due to too many failed attempts. Try again in {minutes} minute(s).");
                    }
                }

                if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                {
                    await _failedLoginRepository.IncrementAsync(user.Id);
                    return ServiceResult<UserProfileDto>.Fail("Invalid username or password.");
                }

                await _failedLoginRepository.ResetAsync(user.Id);

                var firstRole = user.Roles?.FirstOrDefault();
                string roleName = firstRole?.Name ?? "Standard User";

                SessionContext.GetInstance().Populate(user, roleName);

                var roleDto = new RoleDto
                {
                    Id = firstRole?.Id ?? Guid.Empty,
                    Name = roleName
                };

                var profileDto = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    AvatarUrl = user.AvatarUrl,
                    Role = roleDto,
                    IsSuspended = user.IsSuspended,
                    Country = user.Country,
                    City = user.City,
                    StreetName = user.StreetName,
                    StreetNumber = user.StreetNumber
                };

                return ServiceResult<UserProfileDto>.Ok(profileDto);
            }
        }

        public async Task<ServiceResult<bool>> LogoutAsync()
        {
            SessionContext.GetInstance().Clear();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync()
        {
            return ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com to reset your password.");
        }
    }
}
