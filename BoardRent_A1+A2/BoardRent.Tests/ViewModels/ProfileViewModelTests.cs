namespace BoardRent.Tests.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Domain;
    using BoardRent.Services;
    using BoardRent.Utils;
    using BoardRent.ViewModels;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for the ProfileViewModel class.
    /// </summary>
    public class ProfileViewModelTests
    {
        private readonly Mock<IUserService> mockUserService;
        private readonly Mock<IAuthService> mockAuthService;
        private readonly Mock<IFilePickerService> mockFilePickerService;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly ProfileViewModel systemUnderTest;
        private readonly Guid testUserId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileViewModelTests"/> class.
        /// </summary>
        public ProfileViewModelTests()
        {
            this.mockUserService = new Mock<IUserService>();
            this.mockAuthService = new Mock<IAuthService>();
            this.mockFilePickerService = new Mock<IFilePickerService>();
            this.mockSessionContext = new Mock<ISessionContext>();

            // Pregătim un identificator de test
            this.testUserId = Guid.NewGuid();

            // Configurăm mock-ul de sesiune să returneze ID-ul utilizatorului de test
            this.mockSessionContext.Setup(session => session.UserId).Returns(this.testUserId);

            this.systemUnderTest = new ProfileViewModel(
                this.mockUserService.Object,
                this.mockAuthService.Object,
                this.mockFilePickerService.Object,
                this.mockSessionContext.Object);
        }

        [Fact]
        public async Task LoadProfileAsync_ValidData_PopulatesProperties()
        {
            // Arrange
            UserProfileDataTransferObject profileData = new UserProfileDataTransferObject
            {
                Id = this.testUserId,
                Username = "loaded_user",
                DisplayName = "Loaded Name",
                Email = "test@test.com"
            };

            this.mockUserService.Setup(service => service.GetProfileAsync(this.testUserId))
                            .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(profileData));

            // Act
            await this.systemUnderTest.LoadProfileAsync();

            // Assert
            Assert.Equal("loaded_user", this.systemUnderTest.Username);
            Assert.Equal("Loaded Name", this.systemUnderTest.DisplayName);
            Assert.Equal("test@test.com", this.systemUnderTest.Email);
        }

        [Fact]
        public async Task SaveProfileCommand_InvalidData_SetsErrorMessage()
        {
            // Arrange
            this.systemUnderTest.DisplayName = "A"; // Lungime invalidă

            ServiceResult<bool> failResult = ServiceResult<bool>.Fail("DisplayName|Display name must be between 2 and 50 characters long.");

            this.mockUserService.Setup(service => service.UpdateProfileAsync(this.testUserId, It.IsAny<UserProfileDataTransferObject>()))
                            .ReturnsAsync(failResult);

            // Act
            this.systemUnderTest.SaveProfileCommand.Execute(null);

            await Task.Delay(150);

            // Assert
            Assert.Equal(failResult.Error, this.systemUnderTest.ErrorMessage);
        }

        [Fact]
        public async Task SelectAvatarCommand_UserPicksFile_SetsAvatarUrlPreview()
        {
            // Arrange
            string fakePath = "C:\\test_image.jpg";
            this.mockFilePickerService.Setup(service => service.PickImageFileAsync()).ReturnsAsync(fakePath);

            // Act
            this.systemUnderTest.SelectAvatarCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal(fakePath, this.systemUnderTest.AvatarUrl);

            this.mockUserService.Verify(service => service.UploadAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmPasswordError()
        {
            // Arrange
            this.systemUnderTest.NewPassword = "SecurePassword123!";
            this.systemUnderTest.ConfirmPassword = "DifferentPassword123!";

            // Act
            this.systemUnderTest.SaveNewPasswordCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            Assert.Equal("Passwords do not match", this.systemUnderTest.ConfirmPasswordError);
            this.mockUserService.Verify(service => service.ChangePasswordAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SignOutCommand_InvokesLogoutAndNavigation()
        {
            // Arrange
            bool navigationCalled = false;
            this.systemUnderTest.OnSignOutSuccess = () => navigationCalled = true;

            this.mockAuthService.Setup(service => service.LogoutAsync())
                             .ReturnsAsync(ServiceResult<bool>.Ok(true));

            // Act
            this.systemUnderTest.SignOutCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            this.mockAuthService.Verify(service => service.LogoutAsync(), Times.Once);
            Assert.True(navigationCalled);
        }
    }
}