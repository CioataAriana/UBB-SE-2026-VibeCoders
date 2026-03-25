using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieMarketplace.Models;

namespace MovieMarketplace.Views
{
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            this.InitializeComponent();
            CheckLoginStatus();
        }

        private void CheckLoginStatus()
        {
            bool loggedIn = SessionManager.IsLoggedIn;

            BuyButton.IsEnabled = loggedIn;
            SellButton.IsEnabled = loggedIn;
            OrdersButton.IsEnabled = loggedIn;
            SalesButton.IsEnabled = loggedIn;

            if (!loggedIn)
            {
                LoginWarningText.Visibility = Visibility.Visible;

                string msg = "Please log in to enter the marketplace";
                ToolTipService.SetToolTip(BuyButton, msg);
                ToolTipService.SetToolTip(SellButton, msg);
                ToolTipService.SetToolTip(OrdersButton, msg);
                ToolTipService.SetToolTip(SalesButton, msg);
            }
            else
            {
                LoginWarningText.Visibility = Visibility.Collapsed;
                ToolTipService.SetToolTip(BuyButton, null);
                ToolTipService.SetToolTip(SellButton, null);
                ToolTipService.SetToolTip(OrdersButton, null);
                ToolTipService.SetToolTip(SalesButton, null);
            }
        }

        private void BuyEquipment_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MarketplacePage));
        }

        private void SellEquipment_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SellPage));
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MyOrdersPage));
        }

        private void SalesButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MovieMarketplace.Views.MySalesPage));
        }
    }
}