using System.Threading.Tasks;
using BoardRent.DataTransferObjects;
using BoardRent.Services;
using BoardRent.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoardRent.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService authService;

        [ObservableProperty]
        private string usernameOrEmail = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        public LoginViewModel(IAuthService authService)
        {
            this.authService = authService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            this.ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(this.UsernameOrEmail) || string.IsNullOrWhiteSpace(this.Password))
            {
                this.ErrorMessage = "Please enter both username/email and password.";
                return;
            }

            this.IsLoading = true;

            var loginDto = new LoginDataTransferObject
            {
                UsernameOrEmail = this.UsernameOrEmail,
                Password = this.Password,
                RememberMe = this.RememberMe,
            };

            var result = await this.authService.LoginAsync(loginDto);

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
                this.ErrorMessage = result.Error ?? "Login failed.";
            }

            this.IsLoading = false;
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            App.NavigateTo(typeof(RegisterPage));
        }
    }
}