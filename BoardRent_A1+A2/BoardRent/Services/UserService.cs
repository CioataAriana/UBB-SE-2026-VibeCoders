using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BoardRent.Data;
using BoardRent.DTOs;
using BoardRent.Repositories;
using BoardRent.Utils;

namespace BoardRent.Services
{
    class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public UserService(IUserRepository userRepository, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _userRepository = userRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public async Task<ServiceResult<UserProfileDto>> GetProfileAsync(Guid userId)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User not found");

                Debug.WriteLine($"Avatar path: {user.AvatarUrl}");
                Debug.WriteLine($"File exists: {File.Exists(user.AvatarUrl)}");

                var firstRole = user.Roles?.FirstOrDefault();

                return new ServiceResult<UserProfileDto>
                {
                    Data = new UserProfileDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        DisplayName = user.DisplayName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        AvatarUrl = user.AvatarUrl,
                        Role = new RoleDto
                        {
                            Id = firstRole?.Id ?? Guid.Empty,
                            Name = firstRole?.Name ?? "Standard User"
                        },
                        IsSuspended = user.IsSuspended,
                        Country = user.Country,
                        City = user.City,
                        StreetName = user.StreetName,
                        StreetNumber = user.StreetNumber
                    }
                };
            }
        }

        public async Task<ServiceResult<bool>> UpdateProfileAsync(Guid userId, UserProfileDto dto)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);

                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(dto.DisplayName) || dto.DisplayName.Length < 2 || dto.DisplayName.Length > 50)
                    errors.Add("DisplayName|Display name must be between 2 and 50 characters long");

                if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(dto.PhoneNumber, @"^\+?\d{7,15}$"))
                        errors.Add("PhoneNumber|Phone number format is invalid");
                }

                if (!string.IsNullOrWhiteSpace(dto.StreetNumber) && dto.StreetNumber.Length > 10)
                    errors.Add("StreetNumber|Street number must be a valid value");

                if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
                {
                    var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
                    if (existingEmail != null && existingEmail.Id != userId)
                        errors.Add("Email|This email address is already taken by another account");
                }

                if (errors.Any())
                    return ServiceResult<bool>.Fail(string.Join(";", errors));

                user.DisplayName = dto.DisplayName;
                user.Email = dto.Email;
                user.PhoneNumber = dto.PhoneNumber;
                user.Country = dto.Country;
                user.City = dto.City;
                user.StreetName = dto.StreetName;
                user.StreetNumber = dto.StreetNumber;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult<bool>.Fail("User not found");

                if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                    return ServiceResult<bool>.Fail("Current password is incorrect");

                var (pwValid, pwError) = PasswordValidator.Validate(newPassword);
                if (!pwValid)
                    return ServiceResult<bool>.Fail(pwError);

                user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                SessionContext.GetInstance().Clear();

                return ServiceResult<bool>.Ok(true);
            }
        }

        public async Task<string> UploadAvatarAsync(Guid userId, string filePath)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User not found");

                var fileName = $"{userId}_{Path.GetFileName(filePath)}";
                var saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BoardRent", "Avatars");
                Directory.CreateDirectory(saveFolder);
                var savePath = Path.Combine(saveFolder, fileName);
                File.Copy(filePath, savePath, true);

                user.AvatarUrl = savePath;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                return savePath;
            }
        }

        public async Task RemoveAvatarAsync(Guid userId)
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                await uow.OpenAsync();
                ((UserRepository)_userRepository).SetUnitOfWork(uow);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User not found");

                user.AvatarUrl = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }
    }
}
