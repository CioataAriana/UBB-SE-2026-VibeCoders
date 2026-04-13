// <copyright file="ProfileViewModel.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardRent.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using BoardRent.Views;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// View model for the user profile page.
    /// </summary>
    public class ProfileViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IFilePickerService _filePickerService;

        private string _pendingAvatarPath;
        private string _username;
        private string _displayName;
        private string _displayNameError;
        private string _email;
        private string _phoneNumber;
        private string _phoneError;
        private string _country;
        private string _city;
        private string _streetName;
        private string _streetNumber;
        private string _streetNumberError;
        private string _avatarUrl;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _emailError;
        private string _error;
        private string _currentPasswordError;
        private string _newPasswordError;
        private string _confirmPasswordError;

        public ProfileViewModel(IUserService userService, IAuthService authService, IFilePickerService filePickerService)
        {
            this._userService = userService;
            this._authService = authService;
            this._filePickerService = filePickerService;

            this.SaveProfileCommand = new RelayCommand(async () => await this.SaveProfile());
            this.RemoveAvatarCommand = new RelayCommand(async () => await this.RemoveAvatar());
            this.SelectAvatarCommand = new RelayCommand(async () => await this.SelectAvatar());
            this.SaveNewPasswordCommand = new RelayCommand(async () => await this.SaveNewPassword());
            this.SignOutCommand = new RelayCommand(async () => await this.SignOut());
            this.NavigateToAdminPanelCommand = new RelayCommand(this.NavigateToAdminPanel);
        }

        public ICommand SaveProfileCommand { get; }

        public ICommand SelectAvatarCommand { get; }

        public ICommand RemoveAvatarCommand { get; }

        public ICommand SaveNewPasswordCommand { get; }

        public ICommand SignOutCommand { get; }

        public ICommand NavigateToAdminPanelCommand { get; }

        public Visibility AdminButtonVisibility =>
            SessionContext.GetInstance().Role == "Administrator" ? Visibility.Visible : Visibility.Collapsed;

        public ObservableCollection<string> Countries { get; } = new ObservableCollection<string>
        {
            "Romania",
            "Germany",
            "France",
        };

        public string Username { get => this._username; set => this.SetProperty(ref this._username, value); }

        public string DisplayName { get => this._displayName; set => this.SetProperty(ref this._displayName, value); }

        public string DisplayNameError { get => this._displayNameError; set => this.SetProperty(ref this._displayNameError, value); }

        public string Email { get => this._email; set => this.SetProperty(ref this._email, value); }

        public string PhoneNumber { get => this._phoneNumber; set => this.SetProperty(ref this._phoneNumber, value); }

        public string PhoneError { get => this._phoneError; set => this.SetProperty(ref this._phoneError, value); }

        public string Country { get => this._country; set => this.SetProperty(ref this._country, value); }

        public string City { get => this._city; set => this.SetProperty(ref this._city, value); }

        public string StreetName { get => this._streetName; set => this.SetProperty(ref this._streetName, value); }

        public string StreetNumber { get => this._streetNumber; set => this.SetProperty(ref this._streetNumber, value); }

        public string StreetNumberError { get => this._streetNumberError; set => this.SetProperty(ref this._streetNumberError, value); }

        public string AvatarUrl { get => this._avatarUrl; set => this.SetProperty(ref this._avatarUrl, value); }

        public string CurrentPassword { get => this._currentPassword; set => this.SetProperty(ref this._currentPassword, value); }

        public string NewPassword { get => this._newPassword; set => this.SetProperty(ref this._newPassword, value); }

        public string ConfirmPassword { get => this._confirmPassword; set => this.SetProperty(ref this._confirmPassword, value); }

        public string EmailError { get => this._emailError; set => this.SetProperty(ref this._emailError, value); }

        public string ErrorMessage { get => this._error; set => this.SetProperty(ref this._error, value); }

        public string CurrentPasswordError { get => this._currentPasswordError; set => this.SetProperty(ref this._currentPasswordError, value); }

        public string NewPasswordError { get => this._newPasswordError; set => this.SetProperty(ref this._newPasswordError, value); }

        public string ConfirmPasswordError { get => this._confirmPasswordError; set => this.SetProperty(ref this._confirmPasswordError, value); }

        public async Task LoadProfile()
        {
            var userId = SessionContext.GetInstance().UserId;
            var result = await this._userService.GetProfileAsync(userId);

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
            var selectedFilePath = await this._filePickerService.PickImageFileAsync();
            if (selectedFilePath == null)
            {
                return;
            }

            this._pendingAvatarPath = selectedFilePath;
            this.AvatarUrl = selectedFilePath;
        }

        public async Task RemoveAvatar()
        {
            var currentUserId = SessionContext.GetInstance().UserId;
            await this._userService.RemoveAvatarAsync(currentUserId);
            this.AvatarUrl = null;
            this._pendingAvatarPath = null;
        }

        public async Task SignOut()
        {
            await this._authService.LogoutAsync();
            App.NavigateTo(typeof(LoginPage), clearBackStack: true);
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
            var result = await this._userService.ChangePasswordAsync(userId, this.CurrentPassword, this.NewPassword);

            if (result.Success)
            {
                this.CurrentPassword = string.Empty;
                this.NewPassword = string.Empty;
                this.ConfirmPassword = string.Empty;
                App.NavigateTo(typeof(LoginPage), clearBackStack: true);
            }
            else
            {
                if (result.Error.Contains("incorrect"))
                {
                    this.CurrentPasswordError = "Current password is wrong";
                }
                else if (result.Error.Contains("short") || result.Error.Contains("contain"))
                {
                    this.NewPasswordError = result.Error;
                }
                else
                {
                    this.ConfirmPasswordError = result.Error;
                }
            }
        }

        private void NavigateToAdminPanel()
        {
            App.NavigateTo(typeof(AdminPage));
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

            var updateResult = await this._userService.UpdateProfileAsync(currentUserId, profileUpdateData);

            if (updateResult.Success)
            {
                if (!string.IsNullOrEmpty(this._pendingAvatarPath))
                {
                    this.AvatarUrl = await this._userService.UploadAvatarAsync(currentUserId, this._pendingAvatarPath);
                    this._pendingAvatarPath = null;
                }

                this.DisplayNameError = string.Empty;
                this.PhoneError = string.Empty;
                this.StreetNumberError = string.Empty;
                this.EmailError = string.Empty;

                var session = SessionContext.GetInstance();
                var updatedUser = new Domain.User
                {
                    Id = session.UserId,
                    Username = this.Username,
                    DisplayName = this.DisplayName,
                };
                session.Populate(updatedUser, session.Role);
                Debug.WriteLine("Profile updated successfully");
            }
            else
            {
                this.DisplayNameError = string.Empty;
                this.PhoneError = string.Empty;
                this.StreetNumberError = string.Empty;
                this.EmailError = string.Empty;

                var fieldErrorsList = updateResult.Error.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var fieldError in fieldErrorsList)
                {
                    var errorComponents = fieldError.Split('|', 2);
                    if (errorComponents.Length != 2)
                    {
                        continue;
                    }

                    var fieldName = errorComponents[0];
                    var errorMessage = errorComponents[1];

                    switch (fieldName)
                    {
                        case "DisplayName":
                            this.DisplayNameError = errorMessage;
                            break;
                        case "PhoneNumber":
                            this.PhoneError = errorMessage;
                            break;
                        case "StreetNumber":
                            this.StreetNumberError = errorMessage;
                            break;
                        case "Email":
                            this.EmailError = errorMessage;
                            break;
                    }
                }
            }
        }
    }
}