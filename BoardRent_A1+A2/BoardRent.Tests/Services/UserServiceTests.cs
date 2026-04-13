namespace BoardRent.Tests.Services
{
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

    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly UserService systemUnderTest;

        public UserServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockSessionContext = new Mock<ISessionContext>();

            this.mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new UserService(
                this.mockUserRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        #region GetProfileAsync Tests

        [Fact]
        public async Task GetProfileAsync_UserDoesNotExist_ReturnsFailResult()
        {
            Guid userIdentifier = Guid.NewGuid();
            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync((User)null);

            var serviceResult = await this.systemUnderTest.GetProfileAsync(userIdentifier);

            Assert.False(serviceResult.Success);
            Assert.Equal("User not found.", serviceResult.Error);
        }

        [Fact]
        public async Task GetProfileAsync_UserExists_ReturnsSuccessResultWithProfileData()
        {
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User
            {
                Id = userIdentifier,
                Username = "test_user",
                DisplayName = "Test User Display Name",
                Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Standard User" } }
            };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            var serviceResult = await this.systemUnderTest.GetProfileAsync(userIdentifier);

            Assert.True(serviceResult.Success);
            Assert.NotNull(serviceResult.Data);
            Assert.Equal("test_user", serviceResult.Data.Username);
        }

        #endregion

        #region UpdateProfileAsync Tests

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesUserAndReturnsSuccess()
        {
            Guid userIdentifier = Guid.NewGuid();
            User existingUser = new User { Id = userIdentifier, Email = "original@test.com" };
            UserProfileDataTransferObject updateInformation = new UserProfileDataTransferObject
            {
                DisplayName = "Updated Display Name",
                Email = "updated@test.com"
            };

            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(existingUser);
            this.mockUserRepository.Setup(repository => repository.GetByEmailAsync("updated@test.com")).ReturnsAsync((User)null);

            var serviceResult = await this.systemUnderTest.UpdateProfileAsync(userIdentifier, updateInformation);

            Assert.True(serviceResult.Success);
            Assert.Equal("Updated Display Name", existingUser.DisplayName);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(existingUser), Times.Once);
        }

        #endregion

        #region ChangePasswordAsync Tests

        #endregion

        #region Avatar Tests

        [Fact]
        public async Task UploadAvatarAsync_UserExists_UpdatesAvatarPath()
        {
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier };
            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            string temporaryFilePath = Path.GetTempFileName();

            try
            {
                string resultPath = await this.systemUnderTest.UploadAvatarAsync(userIdentifier, temporaryFilePath);

                Assert.NotNull(resultPath);
                Assert.Equal(resultPath, testUser.AvatarUrl);
                this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
            }
            finally
            {
                if (File.Exists(temporaryFilePath))
                {
                    File.Delete(temporaryFilePath);
                }
            }
        }

        [Fact]
        public async Task RemoveAvatarAsync_UserExists_SetsAvatarToNull()
        {
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, AvatarUrl = "C:/path/to/avatar.jpg" };
            this.mockUserRepository.Setup(repository => repository.GetByIdAsync(userIdentifier)).ReturnsAsync(testUser);

            await this.systemUnderTest.RemoveAvatarAsync(userIdentifier);

            Assert.Null(testUser.AvatarUrl);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        #endregion
    }
}