using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;

namespace MovieShop.Views
{
    public sealed partial class StartPageEquipment : Page
    {
        public StartPageEquipment()
        {
            this.InitializeComponent();
            CheckLoginStatus();
        }

        private void CheckLoginStatus()
        {
            bool loggedIn = SessionManager.IsLoggedIn;

            BuyButton.IsEnabled = loggedIn;
            SellButton.IsEnabled = loggedIn;

            LoginWarningText.Visibility = loggedIn ? Visibility.Collapsed : Visibility.Visible;
        }

        private void BuyEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new MarketplacePage();
            }
        }

        private void SellEquipment_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new SellPage();
            }
        }
    }
}