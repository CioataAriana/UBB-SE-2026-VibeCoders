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
            Assert.Equal("Invalid username or password.", loginResult.Error);
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
            Assert.Equal("This account has been suspended.", loginResult.Error);
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
            this.mockFailedLoginRepository.Verify(repository => repository.IncrementAsync(testUser.Id), Times.Once);
        }

        #endregion

        #region Logout Tests

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