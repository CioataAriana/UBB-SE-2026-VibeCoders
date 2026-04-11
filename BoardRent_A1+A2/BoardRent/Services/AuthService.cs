using BoardRent.Data;
using BoardRent.Domain;
using BoardRent.DataTransferObjects;
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

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDataTransferObject registrationRequest)
        {
            var validationErrors = new List<string>();

            const int MinimumDisplayNameLength = 2;
            const int MaximumDisplayNameLength = 50;
            const int MinimumUsernameLength = 3;
            const int MaximumUsernameLength = 30;

            if (string.IsNullOrWhiteSpace(registrationRequest.DisplayName) || registrationRequest.DisplayName.Length < MinimumDisplayNameLength || registrationRequest.DisplayName.Length > MaximumDisplayNameLength)
                validationErrors.Add("DisplayName|Display name must be between 2 and 50 characters long.");

            if (string.IsNullOrWhiteSpace(registrationRequest.Username) || registrationRequest.Username.Length < MinimumUsernameLength || registrationRequest.Username.Length > MaximumUsernameLength
                || !Regex.IsMatch(registrationRequest.Username, @"^[a-zA-Z0-9_]+$"))
                validationErrors.Add("Username|Username must be 3–30 characters and contain only letters, numbers, and underscores.");

            if (string.IsNullOrWhiteSpace(registrationRequest.Email))
                validationErrors.Add("Email|Email is required.");

            var (isPasswordValid, passwordErrorMessage) = PasswordValidator.Validate(registrationRequest.Password);
            if (!isPasswordValid)
                validationErrors.Add($"Password|{passwordErrorMessage}");

            if (registrationRequest.Password != registrationRequest.ConfirmPassword)
                validationErrors.Add("ConfirmPassword|Passwords do not match.");

            if (!string.IsNullOrWhiteSpace(registrationRequest.PhoneNumber))
            {
                if (!Regex.IsMatch(registrationRequest.PhoneNumber, @"^\+?\d{7,15}$"))
                    validationErrors.Add("PhoneNumber|Phone number format is invalid.");
            }

            if (validationErrors.Any())
                return ServiceResult<bool>.Fail(string.Join(";", validationErrors));

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(unitOfWork);

                var existingUsername = await _userRepository.GetByUsernameAsync(registrationRequest.Username);
                if (existingUsername != null)
                    return ServiceResult<bool>.Fail("Username|Username is already taken.");

                var existingEmail = await _userRepository.GetByEmailAsync(registrationRequest.Email);
                if (existingEmail != null)
                    return ServiceResult<bool>.Fail("Email|Email is already registered.");

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    DisplayName = registrationRequest.DisplayName,
                    Username = registrationRequest.Username,
                    Email = registrationRequest.Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(registrationRequest.PhoneNumber) ? null : registrationRequest.PhoneNumber,
                    Country = string.IsNullOrWhiteSpace(registrationRequest.Country) ? null : registrationRequest.Country,
                    City = string.IsNullOrWhiteSpace(registrationRequest.City) ? null : registrationRequest.City,
                    StreetName = string.IsNullOrWhiteSpace(registrationRequest.StreetName) ? null : registrationRequest.StreetName,
                    StreetNumber = string.IsNullOrWhiteSpace(registrationRequest.StreetNumber) ? null : registrationRequest.StreetNumber,
                    PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
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

        public async Task<ServiceResult<UserProfileDataTransferObject>> LoginAsync(LoginDataTransferObject dto)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                await unitOfWork.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(unitOfWork);
                ((FailedLoginRepository)_failedLoginRepository).SetUnitOfWork(unitOfWork);

                User user = await _userRepository.GetByUsernameAsync(dto.UsernameOrEmail);
                if (user == null)
                {
                    user = await _userRepository.GetByEmailAsync(dto.UsernameOrEmail);
                }

                if (user == null)
                {
                    return ServiceResult<UserProfileDataTransferObject>.Fail("Invalid username or password.");
                }

                if (user.IsSuspended)
                {
                    return ServiceResult<UserProfileDataTransferObject>.Fail("This account has been suspended. Please contact support.");
                }

                var failedAttempt = await _failedLoginRepository.GetByUserIdAsync(user.Id);
                if (failedAttempt != null && failedAttempt.LockedUntil.HasValue)
                {
                    if (failedAttempt.LockedUntil.Value > DateTime.UtcNow)
                    {
                        var remaining = failedAttempt.LockedUntil.Value - DateTime.UtcNow;
                        int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
                        return ServiceResult<UserProfileDataTransferObject>.Fail(
                            $"Account is locked due to too many failed attempts. Try again in {minutes} minute(s).");
                    }
                }

                if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                {
                    await _failedLoginRepository.IncrementAsync(user.Id);
                    return ServiceResult<UserProfileDataTransferObject>.Fail("Invalid username or password.");
                }

                await _failedLoginRepository.ResetAsync(user.Id);

                var firstRole = user.Roles?.FirstOrDefault();
                string roleName = firstRole?.Name ?? "Standard User";

                SessionContext.GetInstance().Populate(user, roleName);

                var roleDto = new RoleDataTransferObject
                {
                    Id = firstRole?.Id ?? Guid.Empty,
                    Name = roleName
                };

                var profileDto = new UserProfileDataTransferObject
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

                return ServiceResult<UserProfileDataTransferObject>.Ok(profileDto);
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
