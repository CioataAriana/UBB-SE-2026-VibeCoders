using BoardRent.DTOs;
using BoardRent.Services;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using BoardRent.Views;
using System.Collections.ObjectModel;

namespace BoardRent.ViewModels
{
    public class ProfileViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public ICommand SaveProfileCommand { get; }
        public ICommand UploadAvatarCommand { get; }

        public ICommand RemoveAvatarCommand { get; }

        public ICommand SaveNewPasswordCommand { get; }

        public ICommand SignOutCommand { get; }

        public ProfileViewModel(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
            SaveProfileCommand = new RelayCommand(async () => await SaveProfile());
            RemoveAvatarCommand = new RelayCommand(async () => await RemoveAvatar());
            UploadAvatarCommand = new RelayCommand(async () => await UploadAvatar());
            SaveNewPasswordCommand = new RelayCommand(async () => await SaveNewPassword());
            SignOutCommand= new RelayCommand(async () => await SignOut());
            //LoadProfile();
        }

        public Visibility AdminButtonVisibility =>
            SessionContext.GetInstance().Role == "Administrator" ? Visibility.Visible : Visibility.Collapsed;

        public ObservableCollection<string> Countries { get; } = new ObservableCollection<string>{
            "Romania",
            "Germany",
            "France",
        };

        private string _username;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private string _displayName;
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        
        private string _displayNameError;
        public string DisplayNameError { get => _displayNameError; set => SetProperty(ref _displayNameError, value); }


        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _phoneNumber;
        public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

        private string _phoneError;
        public string PhoneError { get => _phoneError; set => SetProperty(ref _phoneError, value); }

        private string _country;
        public string Country { get => _country; set => SetProperty(ref _country, value); }

        private string _city;
        public string City { get => _city; set => SetProperty(ref _city, value); }

        private string _streetName;
        public string StreetName { get => _streetName; set => SetProperty(ref _streetName, value); }

        private string _streetNumber;
        public string StreetNumber { get => _streetNumber; set => SetProperty(ref _streetNumber, value); }

        private string _streetNumberError;
        public string StreetNumberError { get => _streetNumberError; set => SetProperty(ref _streetNumberError, value); }


        private string _avatarUrl;
        public string AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }

        private string _currentPassword;
        public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }

        private string _newPassword;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _confirmPassword;
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

        private string _emailError;
        public string EmailError { get => _emailError; set => SetProperty(ref _emailError, value); }

        private string _error;
        public string ErrorMessage { get => _error; set => SetProperty(ref _error, value); }

        private string _currentPasswordError;
        public string CurrentPasswordError { get => _currentPasswordError; set => SetProperty(ref _currentPasswordError, value); }

        private string _newPasswordError;
        public string NewPasswordError { get => _newPasswordError; set => SetProperty(ref _newPasswordError, value); }

        private string _confirmPasswordError;
        public string ConfirmPasswordError { get => _confirmPasswordError; set => SetProperty(ref _confirmPasswordError, value); }
        public async Task LoadProfile()
        {
            var userId = SessionContext.GetInstance().UserId;
            Debug.WriteLine($"Session UserId: {userId}");
            var result = await _userService.GetProfileAsync(userId);
            Debug.WriteLine($"Session UserId: {result}");
            Debug.WriteLine($"Result username: {result.Success}");

            if (result.Data != null)
            {
                Username = result.Data.Username;
                Debug.WriteLine("INSIDE IF");
                DisplayName = result.Data.DisplayName;
                Email = result.Data.Email;
                PhoneNumber = result.Data.PhoneNumber;
                Country = result.Data.Country;
                City = result.Data.City;
                StreetName = result.Data.StreetName;
                StreetNumber = result.Data.StreetNumber;
                AvatarUrl = result.Data.AvatarUrl;
            }
        }

        private async Task SaveProfile()
        {
            var userId = SessionContext.GetInstance().UserId;

            var dto = new UserProfileDto
            {
                Id = userId,
                Username = Username,
                DisplayName = DisplayName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                Country = Country,
                City = City,
                StreetName = StreetName,
                StreetNumber = StreetNumber
            };

            var result = await _userService.UpdateProfileAsync(userId, dto);

            if (result.Success)
            {
                DisplayNameError = "";
                PhoneError = "";
                StreetNumberError = "";
                EmailError = "";

                var session = SessionContext.GetInstance();
                var user = new Domain.User
                {
                    Id = session.UserId,
                    Username = Username,
                    DisplayName = DisplayName
                };
                session.Populate(user, session.Role);

                Debug.WriteLine("Profile updated successfully");
            }
            else
            {
                DisplayNameError = "";
                PhoneError = "";
                StreetNumberError = "";
                EmailError = "";

                var fieldErrors = result.Error.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var fe in fieldErrors)
                {
                    var parts = fe.Split('|', 2);
                    if (parts.Length != 2) continue;
                    var field = parts[0];
                    var message = parts[1];

                    switch (field)
                    {
                        case "DisplayName":
                            DisplayNameError = message;
                            break;
                        case "PhoneNumber":
                            PhoneError = message;
                            break;
                        case "StreetNumber":
                            StreetNumberError = message;
                            break;
                        case "Email":
                            EmailError = message;
                            break;
                    }
                }
            }
        }

        public async Task<string> UploadAvatar(Guid userId, string filePath)
        {

            return await _userService.UploadAvatarAsync(userId, filePath);
        }

        public async Task UploadAvatar()
        {
            var picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle(App._window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            var userId = SessionContext.GetInstance().UserId;
            var savedPath = await _userService.UploadAvatarAsync(userId, file.Path);

            AvatarUrl = savedPath;

            
        }

        public async Task RemoveAvatar()
        {
            var userId = SessionContext.GetInstance().UserId;
            await _userService.RemoveAvatarAsync(userId);
            AvatarUrl = null;
        }

        public async Task SignOut()
        {
            await _authService.LogoutAsync();
            App.NavigateTo(typeof(LoginPage), clearBackStack: true);
        }

        public async Task SaveNewPassword()
        {
            CurrentPasswordError = NewPasswordError = ConfirmPasswordError = "";

            if (NewPassword != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords don't match";
                return;
            }

            var userId = SessionContext.GetInstance().UserId;
            var result = await _userService.ChangePasswordAsync(userId, CurrentPassword, NewPassword);

            if (result.Success)
            {
                CurrentPassword = NewPassword = ConfirmPassword = "";
                App.NavigateTo(typeof(LoginPage), clearBackStack: true);
            }
            else
            {
                if (result.Error.Contains("incorrect"))
                    CurrentPasswordError = "Current password is wrong";
                else if (result.Error.Contains("short"))
                    NewPasswordError = "Password must contain at least 8 characters";
                else
                    ConfirmPasswordError = result.Error;
            }
        }


    }


}