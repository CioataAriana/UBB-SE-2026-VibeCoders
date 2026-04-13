namespace BoardRent.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using BoardRent.ViewModels;
    using Moq;
    using Xunit;

    public class AdminViewModelTests
    {
        private readonly Mock<IAdminService> mockAdminService;
        private readonly Mock<IAuthService> mockAuthService;
        private readonly Mock<ISessionContext> mockSessionContext;
        private readonly AdminViewModel systemUnderTest;

        public AdminViewModelTests()
        {
            this.mockAdminService = new Mock<IAdminService>();
            this.mockAuthService = new Mock<IAuthService>();
            this.mockSessionContext = new Mock<ISessionContext>();
            this.mockSessionContext.Setup(session => session.IsLoggedIn).Returns(true);
            this.mockSessionContext.Setup(session => session.Role).Returns("Administrator");

            // Default setup prevents the constructor's fire-and-forget load from throwing.
            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ServiceResult<PaginatedResult<UserProfileDataTransferObject>>.Ok(
                    new PaginatedResult<UserProfileDataTransferObject>
                    {
                        Items = new List<UserProfileDataTransferObject>(),
                        TotalItemCount = 0,
                        PageNumber = 1,
                        PageSize = 10,
                    }));

            this.systemUnderTest = new AdminViewModel(
                this.mockAdminService.Object,
                this.mockAuthService.Object,
                this.mockSessionContext.Object);
        }

        [Fact]
        public async Task LoadUsersAsync_ServiceReturnsData_PopulatesUsersCollection()
        {
            List<UserProfileDataTransferObject> fakeUsers = new List<UserProfileDataTransferObject>
            {
                new UserProfileDataTransferObject { Username = "user1", DisplayName = "User One" },
                new UserProfileDataTransferObject { Username = "user2", DisplayName = "User Two" },
            };

            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(1, 10))
                .ReturnsAsync(ServiceResult<PaginatedResult<UserProfileDataTransferObject>>.Ok(
                    new PaginatedResult<UserProfileDataTransferObject>
                    {
                        Items = fakeUsers,
                        TotalItemCount = 2,
                        PageNumber = 1,
                        PageSize = 10,
                    }));

            await this.systemUnderTest.LoadUsersAsync();

            Assert.Equal(2, this.systemUnderTest.Users.Count);
            Assert.Equal("user1", this.systemUnderTest.Users[0].Username);
            Assert.False(this.systemUnderTest.IsLoading);
        }

        [Fact]
        public async Task LoadUsersAsync_ServiceFails_SetsErrorMessage()
        {
            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(ServiceResult<PaginatedResult<UserProfileDataTransferObject>>.Fail("Unauthorized access."));

            await this.systemUnderTest.LoadUsersAsync();

            Assert.Equal("Unauthorized access.", this.systemUnderTest.ErrorMessage);
        }

        [Fact]
        public void SelectedUser_WhenChanged_NotifiesCommandState()
        {
            UserProfileDataTransferObject testUser = new UserProfileDataTransferObject { Username = "target" };

            this.systemUnderTest.SelectedUser = testUser;

            Assert.True(this.systemUnderTest.SuspendUserCommand.CanExecute(null));
            Assert.True(this.systemUnderTest.ResetPasswordCommand.CanExecute(null));
        }

        [Fact]
        public void SelectedUser_WhenNull_CommandsCannotExecute()
        {
            this.systemUnderTest.SelectedUser = null;

            Assert.False(this.systemUnderTest.SuspendUserCommand.CanExecute(null));
            Assert.False(this.systemUnderTest.UnsuspendUserCommand.CanExecute(null));
            Assert.False(this.systemUnderTest.UnlockAccountCommand.CanExecute(null));
            Assert.False(this.systemUnderTest.ResetPasswordCommand.CanExecute(null));
        }

        [Fact]
        public async Task SuspendUserAsync_UserSelected_CallsServiceAndReloads()
        {
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId, Username = "victim" };

            this.mockAdminService
                .Setup(service => service.SuspendUserAsync(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.SuspendUserCommand.ExecuteAsync(null);

            this.mockAdminService.Verify(service => service.SuspendUserAsync(userId), Times.Once);
            this.mockAdminService.Verify(
                service => service.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task SuspendUserAsync_ServiceFails_SetsErrorMessage()
        {
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId };

            this.mockAdminService
                .Setup(service => service.SuspendUserAsync(userId))
                .ReturnsAsync(ServiceResult<bool>.Fail("User not found."));

            await this.systemUnderTest.SuspendUserCommand.ExecuteAsync(null);

            Assert.Equal("User not found.", this.systemUnderTest.ErrorMessage);
        }

        [Fact]
        public async Task NextPageAsync_UnderTotalPages_IncrementsPageAndReloads()
        {
            this.systemUnderTest.CurrentPage = 1;
            this.systemUnderTest.TotalPages = 5;

            this.mockAdminService
                .Setup(service => service.GetAllUsersAsync(2, 10))
                .ReturnsAsync(ServiceResult<PaginatedResult<UserProfileDataTransferObject>>.Ok(
                    new PaginatedResult<UserProfileDataTransferObject>
                    {
                        Items = new List<UserProfileDataTransferObject>(),
                        TotalItemCount = 50,
                        PageNumber = 2,
                        PageSize = 10,
                    }));

            await this.systemUnderTest.NextPageCommand.ExecuteAsync(null);

            Assert.Equal(2, this.systemUnderTest.CurrentPage);
            this.mockAdminService.Verify(service => service.GetAllUsersAsync(2, 10), Times.Once);
        }

        [Fact]
        public async Task PreviousPageAsync_AboveFirstPage_DecrementsPageAndReloads()
        {
            this.systemUnderTest.CurrentPage = 3;
            this.systemUnderTest.TotalPages = 5;

            await this.systemUnderTest.PreviousPageCommand.ExecuteAsync(null);

            Assert.Equal(2, this.systemUnderTest.CurrentPage);
        }

        [Fact]
        public async Task UnlockAccountAsync_SuccessfulCall_SetsSuccessMessage()
        {
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId };

            this.mockAdminService
                .Setup(service => service.UnlockAccountAsync(userId))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.UnlockAccountCommand.ExecuteAsync(null);

            Assert.Equal("Account unlocked successfully.", this.systemUnderTest.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordWithValueAsync_ValidPassword_SetsSuccessMessage()
        {
            Guid userId = Guid.NewGuid();
            this.systemUnderTest.SelectedUser = new UserProfileDataTransferObject { Id = userId };
            string newSecret = "NewSecurePass123!";

            this.mockAdminService
                .Setup(service => service.ResetPasswordAsync(userId, newSecret))
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.ResetPasswordWithValueAsync(newSecret);

            Assert.Equal("Password has been reset successfully.", this.systemUnderTest.ErrorMessage);
        }

        [Fact]
        public async Task ResetPasswordWithValueAsync_NoUserSelected_DoesNotCallService()
        {
            this.systemUnderTest.SelectedUser = null;

            await this.systemUnderTest.ResetPasswordWithValueAsync("newpassword");

            this.mockAdminService.Verify(
                service => service.ResetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SignOutCommand_WhenExecuted_CallsAuthServiceLogout()
        {
            this.mockAuthService
                .Setup(service => service.LogoutAsync())
                .ReturnsAsync(ServiceResult<bool>.Ok(true));

            await this.systemUnderTest.SignOutCommand.ExecuteAsync(null);

            this.mockAuthService.Verify(service => service.LogoutAsync(), Times.Once);
        }
    }
}