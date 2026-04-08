using BoardRent.DTOs;
using BoardRent.Services;
using BoardRent.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using System;
using System.Collections.Generic;
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
        public List<string> AvailableCountries { get; } = new List<string>
        {
            "Romania",
            "Germany",
            "France"
        };

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

            IsLoading = true;

            var registrationRequest = new RegisterDto
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

            var registrationResult = await _authService.RegisterAsync(registrationRequest);

            if (registrationResult.Success)
            {
                App.NavigateTo(typeof(ProfilePage), clearBackStack: true);
            }
            else
            {
                const int MaximumSplitSubstrings = 2;
                var parsedFieldErrors = registrationResult.Error.Split(';', StringSplitOptions.RemoveEmptyEntries);
               
                foreach (var fieldError in parsedFieldErrors)
                {
                    var errorComponents = fieldError.Split('|', MaximumSplitSubstrings);
                    if (errorComponents.Length == MaximumSplitSubstrings)
                    {
                        switch (errorComponents[0])
                        {
                            case "DisplayName": DisplayNameError = errorComponents[1]; break;
                            case "Username": UsernameError = errorComponents[1]; break;
                            case "Email": EmailError = errorComponents[1]; break;
                            case "Password": PasswordError = errorComponents[1]; break;
                            case "ConfirmPassword": ConfirmPasswordError = errorComponents[1]; break;
                            case "PhoneNumber": PhoneNumberError = errorComponents[1]; break;
                            default: ErrorMessage = errorComponents[1]; break;
                        }
                    }
                    else
                    {
                        ErrorMessage = fieldError;
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
