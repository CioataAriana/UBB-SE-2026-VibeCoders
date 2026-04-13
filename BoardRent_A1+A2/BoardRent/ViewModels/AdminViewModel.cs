namespace BoardRent.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using BoardRent.DataTransferObjects;
    using BoardRent.Services;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class AdminViewModel : BaseViewModel
    {
        private const int PageSize = 10;
        private readonly IAdminService adminService;

        [ObservableProperty]
        private ObservableCollection<UserProfileDataTransferObject> users = new ();

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

        public AdminViewModel(IAdminService adminService)
        {
            this.adminService = adminService;
            this.LoadUsersAsync().FireAndForgetSafeAsync();
        }

        public async Task LoadUsersAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            var result = await this.adminService.GetAllUsersAsync(this.CurrentPage, PageSize);
            if (result.Success && result.Data != null)
            {
                this.Users = new ObservableCollection<UserProfileDataTransferObject>(result.Data);
                this.TotalPages = result.Data.Count == PageSize ? this.CurrentPage + 1 : this.CurrentPage;
            }
            else
            {
                this.ErrorMessage = result.Error ?? "Failed to load users.";
            }

            this.IsLoading = false;
        }

        public async Task ResetPasswordWithValueAsync(string newPassword)
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var result = await this.adminService.ResetPasswordAsync(this.SelectedUser.Id, newPassword);
            if (result.Success)
            {
                this.ErrorMessage = "Password has been reset successfully.";
            }
            else
            {
                this.ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task SuspendUserAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var result = await this.adminService.SuspendUserAsync(this.SelectedUser.Id);
            if (result.Success)
            {
                this.SelectedUser.IsSuspended = true;
                await this.LoadUsersAsync();
            }
            else
            {
                this.ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnsuspendUserAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var result = await this.adminService.UnsuspendUserAsync(this.SelectedUser.Id);
            if (result.Success)
            {
                this.SelectedUser.IsSuspended = false;
                await this.LoadUsersAsync();
            }
            else
            {
                this.ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task ResetPasswordAsync()
        {
            // Placeholder for command activation
            await Task.CompletedTask;
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnlockAccountAsync()
        {
            if (this.SelectedUser == null)
            {
                return;
            }

            var result = await this.adminService.UnlockAccountAsync(this.SelectedUser.Id);
            if (result.Success)
            {
                this.ErrorMessage = "Account unlocked successfully.";
            }
            else
            {
                this.ErrorMessage = result.Error;
            }
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

        private bool CanModifySelectedUser() => this.SelectedUser != null;

        private bool CanGoToPreviousPage() => this.CurrentPage > 1;

        private bool CanGoToNextPage() => this.CurrentPage < this.TotalPages;
    }

    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // Log or handle exception
            }
        }
    }
}