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

            this.mockUnitOfWork.Setup(unitOfWork => unitOfWork.OpenAsync()).Returns(Task.CompletedTask);
            this.mockUnitOfWorkFactory.Setup(factory => factory.Create()).Returns(this.mockUnitOfWork.Object);

            this.systemUnderTest = new AuthService(
                this.mockUserRepository.Object,
                this.mockFailedLoginRepository.Object,
                this.mockUnitOfWorkFactory.Object,
                this.mockSessionContext.Object);
        }

        [Fact]
        public async Task RegisterAsync_UsernameAlreadyExists_ReturnsFailResult()
        {
            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                Username = "existing_user",
                Password = "Password123!",
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("existing_user"))
                .ReturnsAsync(new User { Username = "existing_user" });

            ServiceResult<bool> registrationResult =
                await this.systemUnderTest.RegisterAsync(registrationRequest);

            Assert.False(registrationResult.Success);
            Assert.Contains("Username is already taken", registrationResult.Error);
        }

        [Fact]
        public async Task RegisterAsync_EmailAlreadyExists_ReturnsFailResult()
        {
            RegisterDataTransferObject registrationRequest = new RegisterDataTransferObject
            {
                Username = "new_user",
                Email = "taken@boardrent.com",
                Password = "Password123!",
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("new_user"))
                .ReturnsAsync((User)null);
            this.mockUserRepository
                .Setup(repository => repository.GetByEmailAsync("taken@boardrent.com"))
                .ReturnsAsync(new User { Email = "taken@boardrent.com" });

            ServiceResult<bool> registrationResult =
                await this.systemUnderTest.RegisterAsync(registrationRequest);

            Assert.False(registrationResult.Success);
            Assert.Contains("Email", registrationResult.Error);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ReturnsFailResult()
        {
            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "ghost_user@test.com",
                Password = "SomePassword123!",
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync(loginRequest.UsernameOrEmail))
                .ReturnsAsync((User)null);
            this.mockUserRepository
                .Setup(repository => repository.GetByEmailAsync(loginRequest.UsernameOrEmail))
                .ReturnsAsync((User)null);

            ServiceResult<UserProfileDataTransferObject> loginResult =
                await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            Assert.Equal("Invalid username or password.", loginResult.Error);
        }

        [Fact]
        public async Task LoginAsync_UsernameAndPassword_LogsInSuccessfully()
        {
            string plainPassword = "Password123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "existing_user",
                DisplayName = "Existing User",
                Email = "existing_user@test.com",
                PasswordHash = PasswordHasher.HashPassword(plainPassword),
                Roles = new List<Role> { new Role { Name = "Administrator" } },
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "existing_user",
                Password = plainPassword,
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("existing_user"))
                .ReturnsAsync(testUser);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(testUser.Id))
                .ReturnsAsync((FailedLoginAttempt)null);

            ServiceResult<UserProfileDataTransferObject> loginResult =
                await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.True(loginResult.Success);
            Assert.Equal("existing_user", loginResult.Data.Username);
            Assert.Equal("Existing User", loginResult.Data.DisplayName);
            Assert.Equal("Administrator", loginResult.Data.Role.Name);
            this.mockSessionContext.Verify(
                context => context.Populate(testUser, "Administrator"),
                Times.Once);
            this.mockFailedLoginRepository.Verify(
                repository => repository.ResetAsync(testUser.Id),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_EmailAndPassword_LogsInSuccessfully()
        {
            string plainPassword = "Password123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "user_from_email",
                DisplayName = "Email User",
                Email = "email_user@test.com",
                PasswordHash = PasswordHasher.HashPassword(plainPassword),
                Roles = new List<Role> { new Role { Name = "Standard User" } },
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "email_user@test.com",
                Password = plainPassword,
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("email_user@test.com"))
                .ReturnsAsync((User)null);
            this.mockUserRepository
                .Setup(repository => repository.GetByEmailAsync("email_user@test.com"))
                .ReturnsAsync(testUser);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(testUser.Id))
                .ReturnsAsync((FailedLoginAttempt)null);

            ServiceResult<UserProfileDataTransferObject> loginResult =
                await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.True(loginResult.Success);
            this.mockUserRepository.Verify(
                repository => repository.GetByEmailAsync("email_user@test.com"),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UserIsSuspended_ReturnsFailResult()
        {
            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "suspended_user",
                Password = "AnyPassword123!",
            };

            User suspendedUser = new User { Username = "suspended_user", IsSuspended = true };
            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("suspended_user"))
                .ReturnsAsync(suspendedUser);

            ServiceResult<UserProfileDataTransferObject> loginResult =
                await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            Assert.Equal("This account has been suspended.", loginResult.Error);
        }

        [Fact]
        public async Task LoginAsync_AccountLocked_ReturnsLockMessageWithRemainingTime()
        {
            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "locked_user",
                Password = "AnyPassword123!",
            };

            User lockedUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "locked_user",
                PasswordHash = PasswordHasher.HashPassword("AnyPassword123!"),
                IsSuspended = false,
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("locked_user"))
                .ReturnsAsync(lockedUser);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(lockedUser.Id))
                .ReturnsAsync(new FailedLoginAttempt
                {
                    UserId = lockedUser.Id,
                    FailedAttempts = 5,
                    LockedUntil = DateTime.UtcNow.AddMinutes(10),
                });

            ServiceResult<UserProfileDataTransferObject> loginResult =
                await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            Assert.Contains("This account is locked.", loginResult.Error);
            Assert.Matches(@".*\d{2}:\d{2}\.$", loginResult.Error);
            this.mockFailedLoginRepository.Verify(
                repository => repository.IncrementAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_IncrementsFailedAttempts()
        {
            string correctPassword = "CorrectPassword123!";
            User testUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "test_user",
                PasswordHash = PasswordHasher.HashPassword(correctPassword),
                IsSuspended = false,
            };

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = "test_user",
                Password = "WrongPassword123!",
            };

            this.mockUserRepository
                .Setup(repository => repository.GetByUsernameAsync("test_user"))
                .ReturnsAsync(testUser);
            this.mockFailedLoginRepository
                .Setup(repository => repository.GetByUserIdAsync(testUser.Id))
                .ReturnsAsync((FailedLoginAttempt)null);

            ServiceResult<UserProfileDataTransferObject> loginResult =
                await this.systemUnderTest.LoginAsync(loginRequest);

            Assert.False(loginResult.Success);
            this.mockFailedLoginRepository.Verify(
                repository => repository.IncrementAsync(testUser.Id),
                Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_Invoked_ClearsSessionAndReturnsSuccess()
        {
            ServiceResult<bool> logoutResult = await this.systemUnderTest.LogoutAsync();

            Assert.True(logoutResult.Success);
            this.mockSessionContext.Verify(context => context.Clear(), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordAsync_Invoked_ReturnsAdminContactMessage()
        {
            ServiceResult<string> result = await this.systemUnderTest.ForgotPasswordAsync();

            Assert.True(result.Success);
            Assert.Contains("admin@boardrent.com", result.Data);
        }
    }
}