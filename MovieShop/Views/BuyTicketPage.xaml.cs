using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using Microsoft.UI.Xaml;
using System;

namespace MovieShop.Views
{
    public sealed partial class BuyTicketPage : Page
    {
        private MovieEvent? _event;

        public BuyTicketPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int id)
            {
                _event = new EventRepo().GetEventById(id);
            }
            else if (e.Parameter is MovieEvent me)
            {
                _event = me;
            }

            if (_event == null)
                return;

            // bind x:Bind expects these properties on the Page's DataContext; we set them on the page for x:Bind
            this.DataContext = _event;

            // update UI state
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            var loggedIn = Models.SessionManager.IsLoggedIn;
            var balance = Models.SessionManager.CurrentUserBalance;

            // If logged in, refresh balance from DB to ensure UI validation is accurate
            if (loggedIn)
            {
                try
                {
                    balance = new UserRepo().GetBalance(Models.SessionManager.CurrentUserID);
                    Models.SessionManager.CurrentUserBalance = balance;
                }
                catch
                {
                    // ignore DB errors here; leave balance as-is
                }
            }

            if (!loggedIn)
            {
                // When user is not logged in, disable purchasing (no login flow here)
                ConfirmButton.IsEnabled = false;
                InsufficientText.Text = "You must be signed in to purchase.";
                InsufficientText.Visibility = Visibility.Visible;
                return;
            }

            if (_event == null)
            {
                ConfirmButton.IsEnabled = false;
                return;
            }

            var insufficient = balance < _event.TicketPrice;
            ConfirmButton.IsEnabled = !insufficient;
            if (insufficient)
            {
                InsufficientText.Text = $"Insufficient funds. Balance: {balance:C} — Price: {_event.TicketPrice:C}";
                InsufficientText.Visibility = Visibility.Visible;
            }
            else
            {
                InsufficientText.Visibility = Visibility.Collapsed;
            }
        }

        private async void ConfirmButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (_event == null) return;

            if (!Models.SessionManager.IsLoggedIn)
            {
                // show a simple login dialog
                var dlg = new ContentDialog
                {
                    Title = "Sign in",
                    Content = "Please sign in to continue.",
                    PrimaryButtonText = "Sign in",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };

                if (await dlg.ShowAsync() != ContentDialogResult.Primary)
                    return;

                // For demo purposes set session
                Models.SessionManager.CurrentUserID = 1;
                Models.SessionManager.CurrentUserBalance = new Repositories.UserRepo().GetBalance(1);
                UpdateButtonState();
                return;
            }

            try
            {
                new EventRepo().PurchaseTicket(Models.SessionManager.CurrentUserID, _event.ID);

                // refresh session balance
                Models.SessionManager.CurrentUserBalance = new Repositories.UserRepo().GetBalance(Models.SessionManager.CurrentUserID);
                UpdateButtonState();

                var dialog = new ContentDialog
                {
                    Title = "Purchase successful",
                    Content = $"Ticket for '{_event.Title}' purchased and added to your library.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };

                await dialog.ShowAsync();

                // Navigate to Inventory so the user can see the purchased ticket
                if (this.XamlRoot?.Content is NavigationPage navPage)
                {
                    navPage.ViewModel.CurrentViewModel = "Inventory";
                }
            }
            catch (InvalidOperationException ex)
            {
                var err = new ContentDialog
                {
                    Title = "Cannot complete purchase",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await err.ShowAsync();
            }
        }
    }
}
