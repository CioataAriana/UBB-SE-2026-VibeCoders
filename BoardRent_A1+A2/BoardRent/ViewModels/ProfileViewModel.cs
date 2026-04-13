using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using BoardRent.DataTransferObjects;
using BoardRent.Services;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.Input;

namespace BoardRent.ViewModels
{
    public class ProfileViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;
        private readonly IFilePickerService filePickerService;

        private string pendingAvatarPath;
        private string username;
        private string displayName;
        private string displayNameError;
        private string email;
        private string phoneNumber;
        private string phoneError;
        private string country;
        private string city;
        private string streetName;
        private string streetNumber;
        private string streetNumberError;
        private string avatarUrl;
        private string currentPassword;
        private string newPassword;
        private string confirmPassword;
        private string emailError;
        private string error;
        private string currentPasswordError;
        private string newPasswordError;
        private string confirmPasswordError;

        public ProfileViewModel(IUserService userService, IAuthService authService, IFilePickerService filePickerService)
        {
            this.userService = userService;
            this.authService = authService;
            this.filePickerService = filePickerService;

            this.SaveProfileCommand = new RelayCommand(async () => await this.SaveProfile());
            this.RemoveAvatarCommand = new RelayCommand(async () => await this.RemoveAvatar());
            this.SelectAvatarCommand = new RelayCommand(async () => await this.SelectAvatar());
            this.SaveNewPasswordCommand = new RelayCommand(async () => await this.SaveNewPassword());
            this.SignOutCommand = new RelayCommand(async () => await this.SignOut());
            this.NavigateToAdminPanelCommand = new RelayCommand(() => this.Navigate?.Invoke(typeof(Views.AdminPage)));
        }

        public Action<Type> Navigate { get; set; }

        public ICommand SaveProfileCommand { get; }
        public ICommand SelectAvatarCommand { get; }
        public ICommand RemoveAvatarCommand { get; }
        public ICommand SaveNewPasswordCommand { get; }
        public ICommand SignOutCommand { get; }
        public ICommand NavigateToAdminPanelCommand { get; }

        public bool IsAdmin => SessionContext.GetInstance().Role == "Administrator";

        public ObservableCollection<string> Countries { get; } = new ObservableCollection<string>
        {
            "Romania", "Germany", "France",
        };

        public string Username { get => this.username; set => this.SetProperty(ref this.username, value); }
        public string DisplayName { get => this.displayName; set => this.SetProperty(ref this.displayName, value); }
        public string DisplayNameError { get => this.displayNameError; set => this.SetProperty(ref this.displayNameError, value); }
        public string Email { get => this.email; set => this.SetProperty(ref this.email, value); }
        public string PhoneNumber { get => this.phoneNumber; set => this.SetProperty(ref this.phoneNumber, value); }
        public string PhoneError { get => this.phoneError; set => this.SetProperty(ref this.phoneError, value); }
        public string Country { get => this.country; set => this.SetProperty(ref this.country, value); }
        public string City { get => this.city; set => this.SetProperty(ref this.city, value); }
        public string StreetName { get => this.streetName; set => this.SetProperty(ref this.streetName, value); }
        public string StreetNumber { get => this.streetNumber; set => this.SetProperty(ref this.streetNumber, value); }
        public string StreetNumberError { get => this.streetNumberError; set => this.SetProperty(ref this.streetNumberError, value); }
        public string AvatarUrl { get => this.avatarUrl; set => this.SetProperty(ref this.avatarUrl, value); }
        public string CurrentPassword { get => this.currentPassword; set => this.SetProperty(ref this.currentPassword, value); }
        public string NewPassword { get => this.newPassword; set => this.SetProperty(ref this.newPassword, value); }
        public string ConfirmPassword { get => this.confirmPassword; set => this.SetProperty(ref this.confirmPassword, value); }
        public string EmailError { get => this.emailError; set => this.SetProperty(ref this.emailError, value); }
        public string ErrorMessage { get => this.error; set => this.SetProperty(ref this.error, value); }
        public string CurrentPasswordError { get => this.currentPasswordError; set => this.SetProperty(ref this.currentPasswordError, value); }
        public string NewPasswordError { get => this.newPasswordError; set => this.SetProperty(ref this.newPasswordError, value); }
        public string ConfirmPasswordError { get => this.confirmPasswordError; set => this.SetProperty(ref this.confirmPasswordError, value); }

        public async Task LoadProfile()
        {
            var userId = SessionContext.GetInstance().UserId;
            var result = await this.userService.GetProfileAsync(userId);

            if (result.Data != null)
            {
                this.Username = result.Data.Username;
                this.DisplayName = result.Data.DisplayName;
                this.Email = result.Data.Email;
                this.PhoneNumber = result.Data.PhoneNumber;
                this.Country = result.Data.Country;
                this.City = result.Data.City;
                this.StreetName = result.Data.StreetName;
                this.StreetNumber = result.Data.StreetNumber;
                this.AvatarUrl = result.Data.AvatarUrl;
            }
        }

        public async Task SelectAvatar()
        {
            var selectedFilePath = await this.filePickerService.PickImageFileAsync();
            if (selectedFilePath == null)
            {
                return;
            }

            this.pendingAvatarPath = selectedFilePath;
            this.AvatarUrl = selectedFilePath;
        }

        public async Task RemoveAvatar()
        {
            var currentUserId = SessionContext.GetInstance().UserId;
            await this.userService.RemoveAvatarAsync(currentUserId);
            this.AvatarUrl = null;
            this.pendingAvatarPath = null;
        }

        public async Task SignOut()
        {
            await this.authService.LogoutAsync();
            this.Navigate?.Invoke(typeof(Views.LoginPage));
        }

        public async Task SaveNewPassword()
        {
            this.CurrentPasswordError = string.Empty;
            this.NewPasswordError = string.Empty;
            this.ConfirmPasswordError = string.Empty;

            if (this.NewPassword != this.ConfirmPassword)
            {
                this.ConfirmPasswordError = "Passwords don't match";
                return;
            }

            var userId = SessionContext.GetInstance().UserId;
            var result = await this.userService.ChangePasswordAsync(userId, this.CurrentPassword, this.NewPassword);

            if (result.Success)
            {
                this.CurrentPassword = this.NewPassword = this.ConfirmPassword = string.Empty;
                this.Navigate?.Invoke(typeof(Views.LoginPage));
            }
        }

        private async Task SaveProfile()
        {
            var currentUserId = SessionContext.GetInstance().UserId;

            var profileUpdateData = new UserProfileDataTransferObject
            {
                Id = currentUserId,
                Username = this.Username,
                DisplayName = this.DisplayName,
                Email = this.Email,
                PhoneNumber = this.PhoneNumber,
                Country = this.Country,
                City = this.City,
                StreetName = this.StreetName,
                StreetNumber = this.StreetNumber,
            };

            var updateResult = await this.userService.UpdateProfileAsync(currentUserId, profileUpdateData);

            if (updateResult.Success)
            {
                if (!string.IsNullOrEmpty(this.pendingAvatarPath))
                {
                    this.AvatarUrl = await this.userService.UploadAvatarAsync(currentUserId, this.pendingAvatarPath);
                    this.pendingAvatarPath = null;
                }

                this.DisplayNameError = this.PhoneError = this.StreetNumberError = this.EmailError = string.Empty;
            }
        }
    }
}