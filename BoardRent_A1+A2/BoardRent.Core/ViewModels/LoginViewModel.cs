namespace BoardRent.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService authService;
        private readonly ILoginPreferenceStore loginPreferenceStore;

        [ObservableProperty]
        private string usernameOrEmail = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        public LoginViewModel(IAuthService authService, ILoginPreferenceStore loginPreferenceStore)
        {
            this.authService = authService;
            this.loginPreferenceStore = loginPreferenceStore;
            this.UsernameOrEmail = this.loginPreferenceStore.GetRememberedUsername();
            this.RememberMe = !string.IsNullOrWhiteSpace(this.UsernameOrEmail);
        }

        public Action<string> OnLoginSuccess { get; set; }

        public Action OnNavigateToRegister { get; set; }

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

            LoginDataTransferObject loginRequest = new LoginDataTransferObject
            {
                UsernameOrEmail = this.UsernameOrEmail,
                Password = this.Password,
                RememberMe = this.RememberMe,
            };

            ServiceResult<UserProfileDataTransferObject> loginResult = await this.authService.LoginAsync(loginRequest);

            if (loginResult.Success && loginResult.Data != null)
            {
                if (this.RememberMe)
                {
                    this.loginPreferenceStore.SaveRememberedUsername(loginResult.Data.Username);
                }
                else
                {
                    this.loginPreferenceStore.ClearRememberedUsername();
                }

                string userRole = loginResult.Data.Role?.Name ?? "Standard User";
                this.OnLoginSuccess?.Invoke(userRole);
            }
            else
            {
                this.ErrorMessage = loginResult.Error ?? "Login failed.";
            }

            this.IsLoading = false;
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            this.OnNavigateToRegister?.Invoke();
        }
    }
}