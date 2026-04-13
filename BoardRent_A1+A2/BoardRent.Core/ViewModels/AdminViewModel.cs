namespace BoardRent.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using BoardRent.Utils;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class AdminViewModel : BaseViewModel
    {
        private const int DefaultPageSize = 10;

        private readonly IAdminService adminService;
        private readonly IAuthService authService;
        private readonly ISessionContext sessionContext;

        [ObservableProperty]
        private ObservableCollection<UserProfileDataTransferObject> users =
            new ObservableCollection<UserProfileDataTransferObject>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnsuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetPasswordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnlockAccountCommand))]
        private UserProfileDataTransferObject selectedUser;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int totalPages = 1;

        public AdminViewModel(
            IAdminService adminService,
            IAuthService authService,
            ISessionContext sessionContext)
        {
            this.adminService = adminService;
            this.authService = authService;
            this.sessionContext = sessionContext;

            TaskUtilities.FireAndForgetSafeAsync(this.LoadUsersAsync());
        }

        public bool IsUnauthorized =>
            !this.sessionContext.IsLoggedIn || this.sessionContext.Role != "Administrator";

        public async Task LoadUsersAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            var serviceResult =
                await this.adminService.GetAllUsersAsync(this.CurrentPage, DefaultPageSize);

            if (serviceResult.Success && serviceResult.Data != null)
            {
                this.Users = new ObservableCollection<UserProfileDataTransferObject>(
                    serviceResult.Data.Items);

                this.TotalPages = serviceResult.Data.TotalPageCount;
            }
            else
            {
                this.ErrorMessage = serviceResult.Error ?? "Failed to load users.";
            }

            this.IsLoading = false;
        }

        public async Task ResetPasswordWithValueAsync(string newPassword)
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult =
                await this.adminService.ResetPasswordAsync(this.SelectedUser.Id, newPassword);

            this.ErrorMessage = serviceResult.Success
                ? "Password has been reset successfully."
                : serviceResult.Error;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedUser))]
        private async Task SuspendUserAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult = await this.adminService.SuspendUserAsync(this.SelectedUser.Id);

            if (serviceResult.Success)
            {
                await this.LoadUsersAsync();
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelectedUser))]
        private async Task UnsuspendUserAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult = await this.adminService.UnsuspendUserAsync(this.SelectedUser.Id);

            if (serviceResult.Success)
            {
                await this.LoadUsersAsync();
            }
            else
            {
                this.ErrorMessage = serviceResult.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(HasSelectedUser))]
        private async Task ResetPasswordAsync()
        {
            // This command is intentionally empty in the ViewModel.
            // The View is responsible for prompting the administrator for the new
            // password and then calling ResetPasswordWithValueAsync directly.
            await Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedUser))]
        private async Task UnlockAccountAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var serviceResult =
                await this.adminService.UnlockAccountAsync(this.SelectedUser.Id);

            this.ErrorMessage = serviceResult.Success
                ? "Account unlocked successfully."
                : serviceResult.Error;
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private async Task PreviousPageAsync()
        {
            if (this.CurrentPage > 1)
            {
                this.CurrentPage--;
                await this.LoadUsersAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task NextPageAsync()
        {
            if (this.CurrentPage < this.TotalPages)
            {
                this.CurrentPage++;
                await this.LoadUsersAsync();
            }
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await this.authService.LogoutAsync();
            this.OnPropertyChanged(nameof(this.IsUnauthorized));
        }

        // ── CanExecute predicates ────────────────────────────────────────────────
        private bool HasSelectedUser() => this.SelectedUser != null;

        private bool CanGoToPreviousPage() => this.CurrentPage > 1;

        private bool CanGoToNextPage() => this.CurrentPage < this.TotalPages;
    }
}