using BoardRent.DTOs;
using BoardRent.Services;
using BoardRent.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace BoardRent.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string usernameOrEmail = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username/email and password.";
                return;
            }

            IsLoading = true;

            var loginDto = new LoginDto
            {
                UsernameOrEmail = this.UsernameOrEmail,
                Password = this.Password,
                RememberMe = this.RememberMe
            };

            var result = await _authService.LoginAsync(loginDto);

            if (result.Success)
            {
                if (result.Data.Role?.Name == "Administrator")
                {
                    App.NavigateTo(typeof(AdminPage), clearBackStack: true);
                }
                else
                {
                    App.NavigateTo(typeof(ProfilePage), clearBackStack: true);
                }
            }
            else
            {
                ErrorMessage = result.Error ?? "Login failed.";
            }

            IsLoading = false;
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            App.NavigateTo(typeof(RegisterPage));
        }
    }
}
