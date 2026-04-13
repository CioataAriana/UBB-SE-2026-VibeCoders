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

    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IFailedLoginRepository> mockFailedLoginRepository;
        private readonly Mock<IUnitOfWorkFactory> mockUnitOfWorkFactory;
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly AuthService systemUnderTest;

        public AuthServiceTests()
        {
            this.mockUserRepository = new Mock<IUserRepository>();
            this.mockFailedLoginRepository = new Mock<IFailedLoginRepository>();
            this.mockUnitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            this.mockUnitOfWork = new Mock<IUnitOfWork>();
            this.mockSessionContext = new Mock<ISessionContext>();

            this.mockUnitOfWork.Setup(uow => uow.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new AuthService(
                this.mockUserRepository.Object,
                this.mockFailedLoginRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        #region Register Tests

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsFailResult()
        {
            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                Username = "existing_user",
                Password = "Password123!"
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("existing_user"))
                .ReturnsAsync(new User { Username = "existing_user" });

            ServiceResult<bool> registrationResult = await this.systemUnderTest.RegisterAsync(registrationRequest);

            Assert.False(registrationResult.Success);
            Assert.Contains("Username is already taken", registrationResult.Error);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_AddsUserAndPopulatesSession()
        {
            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                Username = "new_user",
                DisplayName = "New User",
                Email = "new@test.com",
                Password = "Password123!"
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("new_user"))
                .ReturnsAsync((User)null);

            ServiceResult<bool> registrationResult = await this.systemUnderTest.RegisterAsync(registrationRequest);

            Assert.True(registrationResult.Success);
            this.mockUserRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Once);
            this.mockUserRepository.Verify(repository => repository.AddRoleAsync(It.IsAny<Guid>(), "Standard User"), Times.Once);
            this.mockSessionContext.Verify(session => session.Populate(It.IsAny<User>(), "Standard User"), Times.Once);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsFailResult()
        {
            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "ghost_user@test.com",
                Password = "SomePassword123!"
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync(loginRequest.UsernameOrEmail))
                .ReturnsAsync((User)null);
            this.mockUserRepository
                .Setup(repository => repository.GetByEmailAsync(loginRequest.UsernameOrEmail))
                .ReturnsAsync((User)null);

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            Assert.Equal("Sign-in was unsuccessful.", loginResult.Error);
        }

        [Fact]
        public async Task LoginAsync_UserIsSuspended_ReturnsFailResult()
        {
            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "suspended_user",
                Password = "AnyPassword123!"
            };

            User suspendedUser = new User { Username = "suspended_user", IsSuspended = true };
            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("suspended_user")).ReturnsAsync(suspendedUser);

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            Assert.Equal("Sign-in was unsuccessful.", loginResult.Error);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_IncrementsFailedAttempts()
        {
            string correctPassword = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";

            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user",
                PasswordHash = PasswordHasher.HashPassword(correctPassword),
                IsSuspended = false
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "test_user",
                Password = wrongPassword
            };

            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("test_user")).ReturnsAsync(testUser);

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            Assert.Equal("Sign-in was unsuccessful.", loginResult.Error);
            this.mockFailedLoginRepository.Verify(repository => repository.IncrementAsync(testUser.Id), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ValidEmailAndPassword_ReturnsProfileAndPopulatesSession()
        {
            string password = "ValidPassword123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "valid_user",
                Email = "valid@boardrent.com",
                DisplayName = "Valid User",
                PasswordHash = PasswordHasher.HashPassword(password),
                IsSuspended = false,
                Roles = new List<Role> { new Role { Name = "Standard User" } }
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "valid@boardrent.com",
                Password = password
            };

            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync(loginRequest.UsernameOrEmail)).ReturnsAsync((User)null);
            this.mockUserRepository.Setup(repository => repository.GetByEmailAsync(loginRequest.UsernameOrEmail)).ReturnsAsync(testUser);

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.True(loginResult.Success);
            Assert.Equal("valid_user", loginResult.Data.Username);
            this.mockSessionContext.Verify(session => session.Populate(testUser, "Standard User"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ResetsFailedAttemptsAndReturnsProfile()
        {
            string password = "ValidPassword123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "valid_user",
                PasswordHash = PasswordHasher.HashPassword(password),
                IsSuspended = false,
                Roles = new List<Role> { new Role { Name = "Administrator" } }
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject { UsernameOrEmail = "valid_user", Password = password };
            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("valid_user")).ReturnsAsync(testUser);

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.True(loginResult.Success);
            Assert.Equal("Administrator", loginResult.Data.Role.Name);
            this.mockFailedLoginRepository.Verify(repository => repository.ResetAsync(testUser.Id), Times.Once);
            this.mockSessionContext.Verify(session => session.Populate(testUser, "Administrator"), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_AccountLocked_ReturnsTimeRemainingMessage()
        {
            string password = "ValidPassword123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "locked_user",
                PasswordHash = PasswordHasher.HashPassword(password),
                IsSuspended = false
            };
            FailedLoginAttempt failedAttempt = new FailedLoginAttempt
            {
                UserId = testUser.Id,
                FailedAttempts = 5,
                LockedUntil = DateTime.UtcNow.AddMinutes(10)
            };

            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("locked_user")).ReturnsAsync(testUser);
            this.mockFailedLoginRepository.Setup(repository => repository.GetByUserIdAsync(testUser.Id)).ReturnsAsync(failedAttempt);

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(new LoginDataTransferObject
            {
                UsernameOrEmail = "locked_user",
                Password = password
            });

            Assert.False(loginResult.Success);
            Assert.Contains("Account locked due to 5 failed sign-in attempts.", loginResult.Error);
            Assert.Contains("Try again in", loginResult.Error);
        }

        [Fact]
        public async Task LoginAsync_FifthFailedAttempt_ReturnsLockMessage()
        {
            string correctPassword = "CorrectPassword123!";
            string wrongPassword = "WrongPassword123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "almost_locked_user",
                PasswordHash = PasswordHasher.HashPassword(correctPassword),
                IsSuspended = false
            };

            this.mockUserRepository.Setup(repository => repository.GetByUsernameAsync("almost_locked_user")).ReturnsAsync(testUser);
            this.mockFailedLoginRepository
                .SetupSequence(repository => repository.GetByUserIdAsync(testUser.Id))
                .ReturnsAsync(new FailedLoginAttempt { UserId = testUser.Id, FailedAttempts = 4, LockedUntil = null })
                .ReturnsAsync(new FailedLoginAttempt { UserId = testUser.Id, FailedAttempts = 5, LockedUntil = DateTime.UtcNow.AddMinutes(15) });

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.systemUnderTest.LoginAsync(new LoginDataTransferObject
            {
                UsernameOrEmail = "almost_locked_user",
                Password = wrongPassword
            });

            Assert.False(loginResult.Success);
            Assert.Contains("Account locked due to 5 failed sign-in attempts.", loginResult.Error);
            this.mockFailedLoginRepository.Verify(repository => repository.IncrementAsync(testUser.Id), Times.Once);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task LogoutAsync_Invoked_ClearsSessionContext()
        {
            ServiceResult<bool> logoutResult = await this.systemUnderTest.LogoutAsync();

            Assert.True(logoutResult.Success);
            this.mockSessionContext.Verify(session => session.Clear(), Times.Once);
        }

        #endregion

        #region Forgot Password Tests

        [Fact]
        public async Task ForgotPasswordAsync_Invoked_ReturnsAdminContactMessage()
        {
            ServiceResult<string> result = await this.systemUnderTest.ForgotPasswordAsync();

            Assert.True(result.Success);
            Assert.Contains("admin@boardrent.com", result.Data);
        }

        #endregion
    }
}