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

            // Setup a fake logged-in user in the Singleton SessionContext
            this.testUserId = Guid.NewGuid();
            var fakeUser = new User { Id = this.testUserId, Username = "testuser", DisplayName = "Test User" };
            SessionContext.GetInstance().Populate(fakeUser, "Standard User");

            this.systemUnderTest = new ProfileViewModel(
                this.mockUserService.Object,
                this.mockAuthService.Object,
                this.mockFilePickerService.Object);
        }

        [Fact]
        public async Task LoadProfile_ValidData_PopulatesProperties()
        {
            // Arrange
            var profileData = new UserProfileDataTransferObject
            {
                Id = this.testUserId,
                Username = "loaded_user",
                DisplayName = "Loaded Name",
                Email = "test@test.com"
            };

            this.mockUserService.Setup(s => s.GetProfileAsync(this.testUserId))
                            .ReturnsAsync(ServiceResult<UserProfileDataTransferObject>.Ok(profileData));

            // Act
            await this.systemUnderTest.LoadProfile();

            // Assert
            Assert.Equal("loaded_user", this.systemUnderTest.Username);
            Assert.Equal("Loaded Name", this.systemUnderTest.DisplayName);
            Assert.Equal("test@test.com", this.systemUnderTest.Email);
        }

        [Fact]
        public async Task SaveProfileCommand_InvalidData_SetsErrorProperties()
        {
            // Arrange
            this.systemUnderTest.DisplayName = "A"; // Invalid length

            var failResult = ServiceResult<bool>.Fail("DisplayName|Display name must be between 2 and 50 characters long.");
            this.mockUserService.Setup(s => s.UpdateProfileAsync(this.testUserId, It.IsAny<UserProfileDataTransferObject>()))
                            .ReturnsAsync(failResult);

            // Act
            this.systemUnderTest.SaveProfileCommand.Execute(null);

            // Wait for the async command to complete (simple workaround for ICommand)
            await Task.Delay(100);

            // Assert
            Assert.Equal("Display name must be between 2 and 50 characters long.", this.systemUnderTest.DisplayNameError);
        }

        [Fact]
        public async Task SelectAvatarCommand_UserPicksFile_SetsAvatarUrlPreview()
        {
            // Arrange
            var fakePath = "C:\\poza_test.jpg";
            this.mockFilePickerService.Setup(s => s.PickImageFileAsync()).ReturnsAsync(fakePath);

            // Act
            this.systemUnderTest.SelectAvatarCommand.Execute(null);
            await Task.Delay(100); // Allow async relay command to finish

            // Assert
            Assert.Equal(fakePath, this.systemUnderTest.AvatarUrl);

            // It should NOT call UploadAvatarAsync yet (only on save)
            this.mockUserService.Verify(s => s.UploadAvatarAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmError()
        {
            // Arrange
            this.systemUnderTest.NewPassword = "Password123!";
            this.systemUnderTest.ConfirmPassword = "DifferentPassword123!";

            // Act
            this.systemUnderTest.SaveNewPasswordCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            Assert.Equal("Passwords don't match", this.systemUnderTest.ConfirmPasswordError);
            this.mockUserService.Verify(s => s.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}