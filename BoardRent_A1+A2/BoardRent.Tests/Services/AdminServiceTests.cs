namespace BoardRent.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRent.Data;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Repositories;
    using BoardRent.Services;
    using BoardRent.Utils;
    using Moq;
    using Xunit;

    public class AdminServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly AdminService systemUnderTest;

        public AdminServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockSessionContext = new Mock<ISessionContext>();

            this.mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new AdminService(
                this.mockUserRepository.Object,
                this.mockFailedLoginRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        private void SetupAdminSession()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Administrator");
        }

        [Fact]
        public async Task GetAllUsersAsync_NotAdministrator_ReturnsFailResult()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Standard User");

            ServiceResult<PaginatedResult<UserProfileDataTransferObject>> serviceResult =
                await this.systemUnderTest.GetAllUsersAsync(1, 10);

            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        [Fact]
        public async Task GetAllUsersAsync_NotLoggedIn_ReturnsFailResult()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(false);

            ServiceResult<PaginatedResult<UserProfileDataTransferObject>> serviceResult =
                await this.systemUnderTest.GetAllUsersAsync(1, 10);

            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        [Fact]
        public async Task GetAllUsersAsync_Administrator_ReturnsPaginatedUserList()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User
            {
                Id = userIdentifier,
                Username = "testuser",
                DisplayName = "Test User",
                Email = "test@boardrent.com",
                IsSuspended = false,
                Roles = new List<Role> { new Role { Id = Guid.NewGuid(), Name = "Standard User" } },
            };

            this.mockUserRepository
                .Setup(repository => repository.GetPageAsync(1, 10))
                .ReturnsAsync(new List<User> { testUser });
            this.mockUserRepository
                .Setup(repository => repository.GetTotalCountAsync())
                .ReturnsAsync(1);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(userIdentifier))
                .ReturnsAsync((FailedLoginAttempt)null);

            ServiceResult<PaginatedResult<UserProfileDataTransferObject>> serviceResult =
                await this.systemUnderTest.GetAllUsersAsync(1, 10);

            Assert.True(serviceResult.Success);
            Assert.Single(serviceResult.Data.Items);
            Assert.Equal(testUser.DisplayName, serviceResult.Data.Items[0].DisplayName);
            Assert.Equal(testUser.Email, serviceResult.Data.Items[0].Email);
            Assert.Equal(testUser.Username, serviceResult.Data.Items[0].Username);
            Assert.Equal(1, serviceResult.Data.TotalItemCount);
        }

        [Fact]
        public async Task GetAllUsersAsync_UserHasActiveLock_ReturnsUserWithIsLockedTrue()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User
            {
                Id = userIdentifier,
                Roles = new List<Role>(),
            };

            this.mockUserRepository
                .Setup(repository => repository.GetPageAsync(1, 10))
                .ReturnsAsync(new List<User> { testUser });
            this.mockUserRepository
                .Setup(repository => repository.GetTotalCountAsync())
                .ReturnsAsync(1);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(userIdentifier))
                .ReturnsAsync(new FailedLoginAttempt
                {
                    UserId = userIdentifier,
                    FailedAttempts = 5,
                    LockedUntil = DateTime.UtcNow.AddMinutes(10),
                });

            ServiceResult<PaginatedResult<UserProfileDataTransferObject>> serviceResult =
                await this.systemUnderTest.GetAllUsersAsync(1, 10);

            Assert.True(serviceResult.Data.Items[0].IsLocked);
        }

        [Fact]
        public async Task GetAllUsersAsync_UserLockHasExpired_ReturnsUserWithIsLockedFalse()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, Roles = new List<Role>() };

            this.mockUserRepository
                .Setup(repository => repository.GetPageAsync(1, 10))
                .ReturnsAsync(new List<User> { testUser });
            this.mockUserRepository
                .Setup(repository => repository.GetTotalCountAsync())
                .ReturnsAsync(1);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(userIdentifier))
                .ReturnsAsync(new FailedLoginAttempt
                {
                    UserId = userIdentifier,
                    LockedUntil = DateTime.UtcNow.AddMinutes(-1),
                });

            ServiceResult<PaginatedResult<UserProfileDataTransferObject>> serviceResult =
                await this.systemUnderTest.GetAllUsersAsync(1, 10);

            Assert.False(serviceResult.Data.Items[0].IsLocked);
        }

        [Fact]
        public async Task SuspendUserAsync_NotAdministrator_ReturnsFailResult()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Standard User");

            ServiceResult<bool> serviceResult = await this.systemUnderTest.SuspendUserAsync(Guid.NewGuid());

            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        [Fact]
        public async Task SuspendUserAsync_UserDoesNotExist_ReturnsFailResult()
        {
            this.SetupAdminSession();
            this.mockUserRepository
                .Setup(repository => repository.GetByIdentifierAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);

            ServiceResult<bool> serviceResult = await this.systemUnderTest.SuspendUserAsync(Guid.NewGuid());

            Assert.False(serviceResult.Success);
            Assert.Equal("User not found.", serviceResult.Error);
        }

        [Fact]
        public async Task SuspendUserAsync_UserExists_UpdatesStatusToSuspended()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, IsSuspended = false };

            this.mockUserRepository
                .Setup(repository => repository.GetByIdentifierAsync(userIdentifier))
                .ReturnsAsync(testUser);

            ServiceResult<bool> serviceResult = await this.systemUnderTest.SuspendUserAsync(userIdentifier);

            Assert.True(serviceResult.Success);
            Assert.True(testUser.IsSuspended);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task UnsuspendUserAsync_NotAdministrator_ReturnsFailResult()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Standard User");

            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnsuspendUserAsync(Guid.NewGuid());

            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        [Fact]
        public async Task UnsuspendUserAsync_UserDoesNotExist_ReturnsFailResult()
        {
            this.SetupAdminSession();
            this.mockUserRepository
                .Setup(repository => repository.GetByIdentifierAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);

            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnsuspendUserAsync(Guid.NewGuid());

            Assert.False(serviceResult.Success);
            Assert.Equal("User not found.", serviceResult.Error);
        }

        [Fact]
        public async Task UnsuspendUserAsync_UserExists_UpdatesStatusToActive()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            User testUser = new User { Id = userIdentifier, IsSuspended = true };

            this.mockUserRepository
                .Setup(repository => repository.GetByIdentifierAsync(userIdentifier))
                .ReturnsAsync(testUser);

            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnsuspendUserAsync(userIdentifier);

            Assert.True(serviceResult.Success);
            Assert.False(testUser.IsSuspended);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_NotAdministrator_ReturnsFailResult()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Standard User");

            ServiceResult<bool> serviceResult =
                await this.systemUnderTest.ResetPasswordAsync(Guid.NewGuid(), "validpassword");

            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_PasswordIsNull_ReturnsFailResult()
        {
            this.SetupAdminSession();

            ServiceResult<bool> serviceResult =
                await this.systemUnderTest.ResetPasswordAsync(Guid.NewGuid(), null);

            Assert.False(serviceResult.Success);
        }

        [Fact]
        public async Task ResetPasswordAsync_PasswordTooShort_ReturnsFailResult()
        {
            this.SetupAdminSession();

            ServiceResult<bool> serviceResult =
                await this.systemUnderTest.ResetPasswordAsync(Guid.NewGuid(), "123");

            Assert.False(serviceResult.Success);
            Assert.Contains("at least 6 characters", serviceResult.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_UserDoesNotExist_ReturnsFailResult()
        {
            this.SetupAdminSession();
            this.mockUserRepository
                .Setup(repository => repository.GetByIdentifierAsync(It.IsAny<Guid>()))
                .ReturnsAsync((User)null);

            ServiceResult<bool> serviceResult =
                await this.systemUnderTest.ResetPasswordAsync(Guid.NewGuid(), "validpassword");

            Assert.False(serviceResult.Success);
            Assert.Equal("User not found.", serviceResult.Error);
        }

        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_UpdatesPasswordHash()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();
            string oldHash = "old_hash";
            User testUser = new User { Id = userIdentifier, PasswordHash = oldHash };

            this.mockUserRepository
                .Setup(repository => repository.GetByIdentifierAsync(userIdentifier))
                .ReturnsAsync(testUser);

            ServiceResult<bool> serviceResult =
                await this.systemUnderTest.ResetPasswordAsync(userIdentifier, "NewSecurePass123!");

            Assert.True(serviceResult.Success);
            Assert.NotEqual(oldHash, testUser.PasswordHash);
            this.mockUserRepository.Verify(repository => repository.UpdateAsync(testUser), Times.Once);
        }

        [Fact]
        public async Task UnlockAccountAsync_NotAdministrator_ReturnsFailResult()
        {
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Standard User");

            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnlockAccountAsync(Guid.NewGuid());

            Assert.False(serviceResult.Success);
            Assert.Equal("Unauthorized access.", serviceResult.Error);
        }

        [Fact]
        public async Task UnlockAccountAsync_Invoked_CallsResetOnFailedLoginRepository()
        {
            this.SetupAdminSession();
            Guid userIdentifier = Guid.NewGuid();

            ServiceResult<bool> serviceResult = await this.systemUnderTest.UnlockAccountAsync(userIdentifier);

            Assert.True(serviceResult.Success);
            this.mockFailedLoginRepository.Verify(
                repository => repository.ResetAsync(userIdentifier),
                Times.Once);
        }
    }
}