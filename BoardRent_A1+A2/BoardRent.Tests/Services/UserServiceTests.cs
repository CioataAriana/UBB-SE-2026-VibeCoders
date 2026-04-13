using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BoardRent.Data;
using BoardRent.DataTransferObjects;
using BoardRent.Domain;
using BoardRent.Repositories;
using BoardRent.Services;
using BoardRent.Utils;
using Xunit;
using Moq;


namespace BoardRent.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly UserService systemUnderTest;

        public UserServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();

            this.mockUnitOfWork.Setup(u => u.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(f => f.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new UserService(this.mockUserRepository.Object, this.mockUnitOfWorkFactory.Object);
        }

        #region GetProfileAsync Tests

        [Fact]
        public async Task GetProfileAsync_UserDoesNotExist_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await this.systemUnderTest.GetProfileAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task GetProfileAsync_UserExists_ReturnsSuccessResultWithProfileData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Username = "testuser",
                DisplayName = "Test User",
                Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Standard User" } }
            };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.GetProfileAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("testuser", result.Data.Username);
            Assert.Equal("Standard User", result.Data.Role.Name);
        }

        #endregion

        #region UpdateProfileAsync Tests

        [Fact]
        public async Task UpdateProfileAsync_UserDoesNotExist_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateData = new UserProfileDataTransferObject { DisplayName = "Valid Name" };
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await this.systemUnderTest.UpdateProfileAsync(userId, updateData);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task UpdateProfileAsync_InvalidDisplayName_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var updateData = new UserProfileDataTransferObject { DisplayName = "A" }; // Too short

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.UpdateProfileAsync(userId, updateData);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("DisplayName", result.Error);
        }

        [Fact]
        public async Task UpdateProfileAsync_InvalidPhoneNumber_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var updateData = new UserProfileDataTransferObject
            {
                DisplayName = "Valid Name",
                PhoneNumber = "invalid_phone_number"
            };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.UpdateProfileAsync(userId, updateData);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("PhoneNumber", result.Error);
        }

        [Fact]
        public async Task UpdateProfileAsync_InvalidStreetNumber_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var updateData = new UserProfileDataTransferObject
            {
                DisplayName = "Valid Name",
                StreetNumber = "12345678901" // > 10 chars
            };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.UpdateProfileAsync(userId, updateData);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("StreetNumber", result.Error);
        }

        [Fact]
        public async Task UpdateProfileAsync_EmailAlreadyTaken_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId, Email = "old@test.com" };
            var updateData = new UserProfileDataTransferObject
            {
                DisplayName = "Valid Name",
                Email = "new@test.com"
            };

            var otherUser = new User { Id = Guid.NewGuid(), Email = "new@test.com" };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            this.mockUserRepository.Setup(repo => repo.GetByEmailAsync("new@test.com")).ReturnsAsync(otherUser);

            // Act
            var result = await this.systemUnderTest.UpdateProfileAsync(userId, updateData);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("This email address is already taken", result.Error);
        }

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesUserAndReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId, Email = "old@test.com" };
            var updateData = new UserProfileDataTransferObject
            {
                DisplayName = "Updated Name",
                Email = "new@test.com"
            };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            this.mockUserRepository.Setup(repo => repo.GetByEmailAsync("new@test.com")).ReturnsAsync((User)null);

            // Act
            var result = await this.systemUnderTest.UpdateProfileAsync(userId, updateData);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated Name", existingUser.DisplayName);
            Assert.Equal("new@test.com", existingUser.Email);
            this.mockUserRepository.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_UserDoesNotExist_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await this.systemUnderTest.ChangePasswordAsync(userId, "oldpass", "newpass");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Error);
        }

        [Fact]
        public async Task ChangePasswordAsync_IncorrectCurrentPassword_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingHash = PasswordHasher.HashPassword("CorrectOldPassword123!");
            var user = new User { Id = userId, PasswordHash = existingHash };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.ChangePasswordAsync(userId, "WrongPassword!", "NewValidPass123!");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Current password is incorrect.", result.Error);
        }

        [Fact]
        public async Task ChangePasswordAsync_InvalidNewPassword_ReturnsFailResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingHash = PasswordHasher.HashPassword("CorrectOldPassword123!");
            var user = new User { Id = userId, PasswordHash = existingHash };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.ChangePasswordAsync(userId, "CorrectOldPassword123!", "weak");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Password must be at least", result.Error);
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidPasswords_UpdatesPasswordAndReturnsSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingHash = PasswordHasher.HashPassword("CorrectOldPassword123!");
            var user = new User { Id = userId, PasswordHash = existingHash };

            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await this.systemUnderTest.ChangePasswordAsync(userId, "CorrectOldPassword123!", "NewValidPass123!");

            // Assert
            Assert.True(result.Success);
            Assert.NotEqual(existingHash, user.PasswordHash);
            this.mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
        }

        #endregion

        #region UploadAvatarAsync Tests

        [Fact]
        public async Task UploadAvatarAsync_UserDoesNotExist_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => this.systemUnderTest.UploadAvatarAsync(userId, "dummyPath.jpg"));
        }

        [Fact]
        public async Task UploadAvatarAsync_UserExists_UpdatesAvatarAndReturnsPath()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Create a dummy file to avoid FileNotFoundException during File.Copy
            var tempFilePath = Path.GetTempFileName();

            try
            {
                // Act
                var resultPath = await this.systemUnderTest.UploadAvatarAsync(userId, tempFilePath);

                // Assert
                Assert.NotNull(resultPath);
                Assert.Equal(resultPath, user.AvatarUrl);
                this.mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
            }
            finally
            {
                // Cleanup temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }

        #endregion

        #region RemoveAvatarAsync Tests

        [Fact]
        public async Task RemoveAvatarAsync_UserDoesNotExist_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => this.systemUnderTest.RemoveAvatarAsync(userId));
        }

        [Fact]
        public async Task RemoveAvatarAsync_UserExists_SetsAvatarToNullAndUpdates()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, AvatarUrl = "C:/existing/avatar.png" };
            this.mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            await this.systemUnderTest.RemoveAvatarAsync(userId);

            // Assert
            Assert.Null(user.AvatarUrl);
            this.mockUserRepository.Verify(repo => repo.UpdateAsync(user), Times.Once);
        }

        #endregion
    }
}