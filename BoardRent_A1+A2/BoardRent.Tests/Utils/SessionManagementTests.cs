namespace BoardRent.Tests.Utils
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

    public class SessionManagementTests
    {
        [Fact]
        public async Task UmS01_LoginAsync_StoresIdentityInSessionContext()
        {
            SessionContext sessionContext = new SessionContext();
            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<IFailedLoginRepository> mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(mockUnitOfWork.Object);

            string password = "ValidPassword123!";
            User authenticatedUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "session_user",
                DisplayName = "Session User",
                PasswordHash = PasswordHasher.HashPassword(password),
                Roles = new List<Role> { new Role { Name = "Administrator" } }
            };

            mockUserRepository.Setup(repository => repository.GetByUsernameAsync("session_user")).ReturnsAsync(authenticatedUser);

            AuthService authService = new AuthService(
                mockUserRepository.Object,
                mockFailedLoginRepository.Object,
                mockUnitOfWorkFactory.Object,
                sessionContext);

            ServiceResult<UserProfileDataTransferObject> loginResult = await authService.LoginAsync(
                new LoginDataTransferObject
                {
                    UsernameOrEmail = "session_user",
                    Password = password
                });

            Assert.True(loginResult.Success);
            Assert.True(sessionContext.IsLoggedIn);
            Assert.Equal(authenticatedUser.Id, sessionContext.UserId);
            Assert.Equal("session_user", sessionContext.Username);
            Assert.Equal("Session User", sessionContext.DisplayName);
            Assert.Equal("Administrator", sessionContext.Role);
        }

        [Fact]
        public void UmS02_SessionContext_RemainsAvailableDuringApplicationLifetime()
        {
            SessionContext sessionContext = new SessionContext();
            User signedInUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "lifetime_user",
                DisplayName = "Lifetime User"
            };

            sessionContext.Populate(signedInUser, "Standard User");

            Assert.True(sessionContext.IsLoggedIn);
            Assert.Equal("lifetime_user", sessionContext.Username);
            Assert.Equal("Lifetime User", sessionContext.DisplayName);
            Assert.Equal("Standard User", sessionContext.Role);
        }

        [Fact]
        public async Task UmS03_LogoutAsync_ClearsSessionImmediately()
        {
            SessionContext sessionContext = new SessionContext();
            sessionContext.Populate(
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "logout_user",
                    DisplayName = "Logout User"
                },
                "Standard User");

            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<IFailedLoginRepository> mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();

            AuthService authService = new AuthService(
                mockUserRepository.Object,
                mockFailedLoginRepository.Object,
                mockUnitOfWorkFactory.Object,
                sessionContext);

            ServiceResult<bool> logoutResult = await authService.LogoutAsync();

            Assert.True(logoutResult.Success);
            Assert.False(sessionContext.IsLoggedIn);
            Assert.Equal(Guid.Empty, sessionContext.UserId);
            Assert.Equal(string.Empty, sessionContext.Username);
            Assert.Equal(string.Empty, sessionContext.DisplayName);
            Assert.Equal(string.Empty, sessionContext.Role);
        }

        [Fact]
        public async Task UmS04_ChangePasswordAsync_ClearsSessionImmediately()
        {
            SessionContext sessionContext = new SessionContext();
            Guid userId = Guid.NewGuid();
            string oldPassword = "OldPassword123!";
            User persistedUser = new User
            {
                Id = userId,
                Username = "password_user",
                DisplayName = "Password User",
                PasswordHash = PasswordHasher.HashPassword(oldPassword)
            };

            sessionContext.Populate(persistedUser, "Standard User");

            Mock<IUserRepository> mockUserRepository = new Mock<IUserRepository>();
            Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            Mock<IUnitOfWork> mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(mockUnitOfWork.Object);
            mockUserRepository.Setup(repository => repository.GetByIdentifierAsync(userId)).ReturnsAsync(persistedUser);

            UserService userService = new UserService(
                mockUserRepository.Object,
                mockUnitOfWorkFactory.Object,
                sessionContext);

            ServiceResult<bool> result = await userService.ChangePasswordAsync(userId, oldPassword, "NewPassword123!");

            Assert.True(result.Success);
            Assert.False(sessionContext.IsLoggedIn);
            Assert.Equal(Guid.Empty, sessionContext.UserId);
            Assert.Equal(string.Empty, sessionContext.Username);
            Assert.Equal(string.Empty, sessionContext.DisplayName);
            Assert.Equal(string.Empty, sessionContext.Role);
        }

        [Fact]
        public void UmS05_SessionData_IsNotPersistedAcrossSessionContextInstances()
        {
            SessionContext firstApplicationSession = new SessionContext();
            firstApplicationSession.Populate(
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "ephemeral_user",
                    DisplayName = "Ephemeral User"
                },
                "Standard User");

            SessionContext nextApplicationSession = new SessionContext();

            Assert.False(nextApplicationSession.IsLoggedIn);
            Assert.Equal(Guid.Empty, nextApplicationSession.UserId);
            Assert.Null(nextApplicationSession.Username);
            Assert.Null(nextApplicationSession.DisplayName);
            Assert.Null(nextApplicationSession.Role);
        }
    }
}
