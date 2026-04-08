using BoardRent.DTOs;
using BoardRent.Services;
using BoardRent.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace BoardRent.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty] private string displayName = string.Empty;
        [ObservableProperty] private string username = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private string phoneNumber = string.Empty;
        [ObservableProperty] private string country = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private string streetName = string.Empty;
        [ObservableProperty] private string streetNumber = string.Empty;

        [ObservableProperty] private string displayNameError = string.Empty;
        [ObservableProperty] private string usernameError = string.Empty;
        [ObservableProperty] private string emailError = string.Empty;
        [ObservableProperty] private string passwordError = string.Empty;
        [ObservableProperty] private string confirmPasswordError = string.Empty;
        [ObservableProperty] private string phoneNumberError = string.Empty;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        private void ClearErrors()
        {
            DisplayNameError = string.Empty;
            UsernameError = string.Empty;
            EmailError = string.Empty;
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
            PhoneNumberError = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            ClearErrors();

            bool hasClientError = false;

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                DisplayNameError = "Display name is required.";
                hasClientError = true;
            }
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Username is required.";
                hasClientError = true;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email is required.";
                hasClientError = true;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required.";
                hasClientError = true;
            }
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Please confirm your password.";
                hasClientError = true;
            }

            if (hasClientError)
                return;

            IsLoading = true;

            var registerDto = new RegisterDto
            {
                DisplayName = this.DisplayName,
                Username = this.Username,
                Email = this.Email,
                Password = this.Password,
                ConfirmPassword = this.ConfirmPassword,
                PhoneNumber = this.PhoneNumber,
                Country = this.Country,
                City = this.City,
                StreetName = this.StreetName,
                StreetNumber = this.StreetNumber
            };

            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success)
            {
                App.NavigateTo(typeof(ProfilePage), clearBackStack: true);
            }
            else
            {
                var fieldErrors = result.Error.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var fe in fieldErrors)
                {
                    var parts = fe.Split('|', 2);
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                            case "DisplayName": DisplayNameError = parts[1]; break;
                            case "Username": UsernameError = parts[1]; break;
                            case "Email": EmailError = parts[1]; break;
                            case "Password": PasswordError = parts[1]; break;
                            case "ConfirmPassword": ConfirmPasswordError = parts[1]; break;
                            case "PhoneNumber": PhoneNumberError = parts[1]; break;
                            default: ErrorMessage = parts[1]; break;
                        }
                    }
                    else
                    {
                        ErrorMessage = fe;
                    }
                }
            }

            IsLoading = false;
        }

        [RelayCommand]
        private void GoToLogin()
        {
            App.NavigateBack();
        }
    }
}
