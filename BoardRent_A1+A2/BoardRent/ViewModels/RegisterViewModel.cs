using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoardRent.DataTransferObjects;
using BoardRent.Services;
using BoardRent.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoardRent.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService authService;

        [ObservableProperty]
        private string displayName = string.Empty;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string phoneNumber = string.Empty;

        [ObservableProperty]
        private string country = string.Empty;

        [ObservableProperty]
        private string city = string.Empty;

        [ObservableProperty]
        private string streetName = string.Empty;

        [ObservableProperty]
        private string streetNumber = string.Empty;

        [ObservableProperty]
        private string displayNameError = string.Empty;

        [ObservableProperty]
        private string usernameError = string.Empty;

        [ObservableProperty]
        private string emailError = string.Empty;

        [ObservableProperty]
        private string passwordError = string.Empty;

        [ObservableProperty]
        private string confirmPasswordError = string.Empty;

        [ObservableProperty]
        private string phoneNumberError = string.Empty;

        public RegisterViewModel(IAuthService authService)
        {
            this.authService = authService;
        }

        public List<string> AvailableCountries { get; } = new List<string>
        {
            "Romania",
            "Germany",
            "France",
        };

        private void ClearErrors()
        {
            this.DisplayNameError = string.Empty;
            this.UsernameError = string.Empty;
            this.EmailError = string.Empty;
            this.PasswordError = string.Empty;
            this.ConfirmPasswordError = string.Empty;
            this.PhoneNumberError = string.Empty;
            this.ErrorMessage = string.Empty;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            this.ClearErrors();

            this.IsLoading = true;

            var registrationRequest = new RegisterDataTransferObject
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
                StreetNumber = this.StreetNumber,
            };

            var registrationResult = await this.authService.RegisterAsync(registrationRequest);

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
                            case "DisplayName": this.DisplayNameError = errorComponents[1]; break;
                            case "Username": this.UsernameError = errorComponents[1]; break;
                            case "Email": this.EmailError = errorComponents[1]; break;
                            case "Password": this.PasswordError = errorComponents[1]; break;
                            case "ConfirmPassword": this.ConfirmPasswordError = errorComponents[1]; break;
                            case "PhoneNumber": this.PhoneNumberError = errorComponents[1]; break;
                            default: this.ErrorMessage = errorComponents[1]; break;
                        }
                    }
                    else
                    {
                        this.ErrorMessage = fieldError;
                    }
                }
            }

            this.IsLoading = false;
        }

        [RelayCommand]
        private void GoToLogin()
        {
            App.NavigateBack();
        }
    }
}