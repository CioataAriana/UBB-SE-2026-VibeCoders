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

    public class RegisterViewModelTests
    {
        private readonly Mock<IAuthService> mockAuthService;
        private readonly RegisterViewModel systemUnderTest;

        public RegisterViewModelTests()
        {
            this.mockAuthService = new Mock<IAuthService>();
            this.systemUnderTest = new RegisterViewModel(this.mockAuthService.Object);
        }

        [Fact]
        public async Task RegisterAsync_SuccessfulRegistration_InvokesSuccessCallback()
        {
            bool wasNavigationCalled = false;
            this.systemUnderTest.OnRegistrationSuccess = () => wasNavigationCalled = true;

            this.systemUnderTest.Username = "newuser";
            this.systemUnderTest.Password = "Password123!";
            this.systemUnderTest.ConfirmPassword = "Password123!";

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.True(wasNavigationCalled);
            this.mockAuthService.Verify(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_FieldValidationError_MapsErrorsToCorrectProperties()
        {

            string validationErrorMessage = "Username|Username already exists;Password|Password is too short";
            ServiceResult<bool> failResult = ServiceResult<bool>.Fail(validationErrorMessage);

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(failResult);

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.Equal("Username already exists", this.systemUnderTest.UsernameError);
            Assert.Equal("Password is too short", this.systemUnderTest.PasswordError);
            Assert.False(this.systemUnderTest.IsLoading);
        }

        [Fact]
        public async Task RegisterAsync_GeneralError_SetsGeneralErrorMessage()
        {
            string generalError = "Server connection lost";
            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Fail(generalError));

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.Equal(generalError, this.systemUnderTest.ErrorMessage);
            Assert.Equal(string.Empty, this.systemUnderTest.EmailError);
        }

        [Fact]
        public void GoToLogin_CommandExecuted_InvokesNavigateBackRequest()
        {
            bool wasBackNavigationCalled = false;
            this.systemUnderTest.OnNavigateBackRequest = () => wasBackNavigationCalled = true;

            this.systemUnderTest.GoToLoginCommand.Execute(null);

            Assert.True(wasBackNavigationCalled);
        }

        [Fact]
        public async Task RegisterAsync_ClearErrors_RemovesOldErrorsBeforeNewAttempt()
        {
            this.systemUnderTest.UsernameError = "Old error";

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            Assert.Equal(string.Empty, this.systemUnderTest.UsernameError);
        }

        [Fact]
        public async Task RegisterAsync_ValidData_MapsPropertiesToDataTransferObjectCorrectly()
        {
            this.systemUnderTest.DisplayName = "John Doe";
            this.systemUnderTest.Username = "johndoe99";
            this.systemUnderTest.Email = "john@example.com";
            this.systemUnderTest.Password = "SecurePass123!";
            this.systemUnderTest.ConfirmPassword = "SecurePass123!";
            this.systemUnderTest.PhoneNumber = "1234567890";

            this.mockAuthService
                .Setup(service => service.RegisterAsync(It.IsAny<RegisterDataTransferObject>()))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.RegisterCommand.ExecuteAsync(null);

            this.mockAuthService.Verify(service => service.RegisterAsync(It.Is<RegisterDataTransferObject>(dto =>
                dto.DisplayName == "John Doe" &&
                dto.Username == "johndoe99" &&
                dto.Email == "john@example.com" &&
                dto.Password == "SecurePass123!" &&
                dto.ConfirmPassword == "SecurePass123!" &&
                dto.PhoneNumber == "1234567890")), Times.Once);
        }
    }
}