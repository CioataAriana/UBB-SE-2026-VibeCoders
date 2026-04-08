using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BoardRent.DTOs;
using BoardRent.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoardRent.ViewModels
{
    public partial class AdminViewModel : BaseViewModel
    {
        private readonly IAdminService _adminService;
        private const int PageSize = 10;

        [ObservableProperty]
        private ObservableCollection<UserProfileDto> users = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnsuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetPasswordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnlockAccountCommand))]
        private UserProfileDto selectedUser;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int totalPages = 1;

        public AdminViewModel(IAdminService adminService)
        {
            _adminService = adminService;
            LoadUsersAsync().FireAndForgetSafeAsync();
        }

        public async Task LoadUsersAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var result = await _adminService.GetAllUsersAsync(CurrentPage, PageSize);
            if (result.Success && result.Data != null)
            {
                Users = new ObservableCollection<UserProfileDto>(result.Data);
                // Currently repository doesn't give TotalPages, assume 1 for now or dynamically calculate if data length < PageSize
                // This is a placeholder since blocked on repositories
                TotalPages = result.Data.Count == PageSize ? CurrentPage + 1 : CurrentPage; 
            }
            else
            {
                ErrorMessage = result.Error ?? "Failed to load users.";
            }

            IsLoading = false;
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task SuspendUserAsync()
        {
            if (SelectedUser == null) return;
            var result = await _adminService.SuspendUserAsync(SelectedUser.Id);
            if (result.Success)
            {
                SelectedUser.IsSuspended = true;
                // Refresh list or trigger property changed
                await LoadUsersAsync();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnsuspendUserAsync()
        {
            if (SelectedUser == null) return;
            var result = await _adminService.UnsuspendUserAsync(SelectedUser.Id);
            if (result.Success)
            {
                SelectedUser.IsSuspended = false;
                await LoadUsersAsync();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task ResetPasswordAsync()
        {
            // This is kept as a no-op; the actual reset is done via ResetPasswordWithValueAsync
            // triggered from the code-behind dialog.
        }

        public async Task ResetPasswordWithValueAsync(string newPassword)
        {
            if (SelectedUser == null) return;
            var result = await _adminService.ResetPasswordAsync(SelectedUser.Id, newPassword);
            if (result.Success)
            {
                ErrorMessage = "Password has been reset successfully.";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnlockAccountAsync()
        {
            if (SelectedUser == null) return;
            var result = await _adminService.UnlockAccountAsync(SelectedUser.Id);
            if (result.Success)
            {
                ErrorMessage = "Account unlocked successfully.";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadUsersAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadUsersAsync();
            }
        }

        private bool CanModifySelectedUser() => SelectedUser != null;
        private bool CanGoToPreviousPage() => CurrentPage > 1;
        private bool CanGoToNextPage() => CurrentPage < TotalPages;
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
                // Handle or log exception
            }
        }
    }
}
