namespace BoardRent.Tests.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using BoardRent.ViewModels;
    using Moq;
    using Xunit;

    public class LoginViewModelTests
    {
        private readonly Mock<IAuthService> mockAuthService;
        private readonly Mock<ILoginPreferenceStore> mockLoginPreferenceStore;
        private readonly LoginViewModel systemUnderTest;

        public LoginViewModelTests()
        {
            this.mockAuthService = new Mock<IAuthService>();
            this.mockLoginPreferenceStore = new Mock<ILoginPreferenceStore>();
            this.mockLoginPreferenceStore.Setup(store => store.GetRememberedUsername()).Returns(string.Empty);
            this.systemUnderTest = new LoginViewModel(this.mockAuthService.Object, this.mockLoginPreferenceStore.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_InvokesSuccessCallbackWithRole()
        {
            // Arrange
            string capturedRole = string.Empty;
            this.systemUnderTest.OnLoginSuccess = (role) => capturedRole = role;

            this.systemUnderTest.UsernameOrEmail = "admin";
            this.systemUnderTest.Password = "Password123!";

            UserProfileDataTransferObject fakeProfile = new UserProfileDataTransferObject
            {
                Username = "admin",
                Role = new RoleDataTransferObject { Name = "Administrator" }
            };

            this.mockAuthService
                .Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(fakeProfile));

            // Act
            this.systemUnderTest.LoginCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal("Administrator", capturedRole);
            this.mockAuthService.Verify(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_EmptyFields_SetsLocalErrorMessageWithoutCallingService()
        {
            // Arrange
            this.systemUnderTest.UsernameOrEmail = string.Empty;
            this.systemUnderTest.Password = string.Empty;

            // Act
            this.systemUnderTest.LoginCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal("Please enter both username/email and password.", this.systemUnderTest.ErrorMessage);
            this.mockAuthService.Verify(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ServiceReturnsError_SetsErrorMessage()
        {
            // Arrange
            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "wrongpass";

            string serviceError = "Invalid username or password.";
            this.mockAuthService
                .Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Fail(serviceError));

            // Act
            this.systemUnderTest.LoginCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal(serviceError, this.systemUnderTest.ErrorMessage);
            Assert.False(this.systemUnderTest.IsLoading);
        }

        [Fact]
        public void NavigateToRegister_CommandExecuted_InvokesCallback()
        {
            // Arrange
            bool wasNavigationCalled = false;
            this.systemUnderTest.OnNavigateToRegister = () => wasNavigationCalled = true;

            // Act
            this.systemUnderTest.NavigateToRegisterCommand.Execute(null);

            // Assert
            Assert.True(wasNavigationCalled);
        }

        [Fact]
        public async Task LoginAsync_NullRole_DefaultsToStandardUser()
        {
            // Arrange
            string capturedRole = string.Empty;
            this.systemUnderTest.OnLoginSuccess = (role) => capturedRole = role;

            this.systemUnderTest.UsernameOrEmail = "user";
            this.systemUnderTest.Password = "pass";

            // UserProfile fără rol setat
            UserProfileDataTransferObject profileWithNoRole = new UserProfileDataTransferObject
            {
                Username = "user",
                Role = null
            };

            this.mockAuthService
                .Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(profileWithNoRole));

            // Act
            this.systemUnderTest.LoginCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal("Standard User", capturedRole);
        }

        [Fact]
        public void Constructor_RememberedUsernameExists_PrefillsUsernameAndChecksRememberMe()
        {
            this.mockLoginPreferenceStore.Setup(store => store.GetRememberedUsername()).Returns("saved_user");

            LoginViewModel viewModel = new LoginViewModel(this.mockAuthService.Object, this.mockLoginPreferenceStore.Object);

            Assert.Equal("saved_user", viewModel.UsernameOrEmail);
            Assert.True(viewModel.RememberMe);
        }

        [Fact]
        public async Task LoginAsync_RememberMeSelected_SavesReturnedUsername()
        {
            this.systemUnderTest.UsernameOrEmail = "valid@boardrent.com";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.RememberMe = true;

            this.mockAuthService.Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(new UserProfileDataTransferObject
                {
                    Username = "valid_user",
                    Role = new RoleDataTransferObject { Name = "Standard User" }
                }));

            this.systemUnderTest.LoginCommand.Execute(null);
            await Task.Delay(150);

            this.mockLoginPreferenceStore.Verify(store => store.SaveRememberedUsername("valid_user"), Times.Once);
            this.mockLoginPreferenceStore.Verify(store => store.ClearRememberedUsername(), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_RememberMeNotSelected_ClearsRememberedUsername()
        {
            this.systemUnderTest.UsernameOrEmail = "valid_user";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.RememberMe = false;

            this.mockAuthService.Setup(service => service.LoginAsync(It.IsAny<LoginDataTransferObject>()))
                .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(new UserProfileDataTransferObject
                {
                    Username = "valid_user",
                    Role = new RoleDataTransferObject { Name = "Standard User" }
                }));

            this.systemUnderTest.LoginCommand.Execute(null);
            await Task.Delay(150);

            this.mockLoginPreferenceStore.Verify(store => store.ClearRememberedUsername(), Times.Once);
            this.mockLoginPreferenceStore.Verify(store => store.SaveRememberedUsername(It.IsAny<string>()), Times.Never);
        }
    }
}