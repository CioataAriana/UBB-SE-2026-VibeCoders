using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System;
using System.Linq;

namespace MovieShop.Views
{
    public sealed partial class EquipmentDetailPage : Page
    {
        private readonly EquipmentRepo _repo = new EquipmentRepo();
        private readonly UserRepo _userRepo = new UserRepo();
        private Equipment _selectedItem;

        public EquipmentDetailPage(Equipment item)
        {
            this.InitializeComponent();
            _selectedItem = item;
            PopulateUI();
        }

        private void PopulateUI()
        {
            if (_selectedItem == null) return;

            TitleLabel.Text = _selectedItem.Title;
            DescriptionLabel.Text = _selectedItem.Description ?? "No description available.";
            CategoryLabel.Text = _selectedItem.Category;
            ConditionLabel.Text = _selectedItem.Condition;
            PriceLabel.Text = $"Price: ${_selectedItem.Price:F2}";

            if (!string.IsNullOrEmpty(_selectedItem.ImageUrl))
            {
                try
                {
                    ItemImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(_selectedItem.ImageUrl));
                }
                catch { }
            }

            // Always fetch balance from DB so the check is never based on a stale cached value.
            var currentBalance = _userRepo.GetBalance(SessionManager.CurrentUserID);
            SessionManager.CurrentUserBalance = currentBalance;

            bool canAfford = currentBalance >= _selectedItem.Price;
            ConfirmBuyButton.IsEnabled = canAfford;
            ErrorText.Visibility = canAfford ? Visibility.Collapsed : Visibility.Visible;
            if (!canAfford)
                ErrorText.Text = $"Insufficient funds. Balance: {currentBalance:C} — Price: {_selectedItem.Price:C}";
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Visible;
        private void CancelShipping_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Collapsed;

        private async void ConfirmShipping_Click(object sender, RoutedEventArgs e)
        {
            ModalErrorText.Visibility = Visibility.Collapsed;
            string error = "";

            if (string.IsNullOrWhiteSpace(ModalNameInput.Text))
                error += "- Name is required.\n";

            if (ModalAddressInput.Text.Length < 10)
                error += "- Address too short (min 10 chars).\n";

            string phone = ModalPhoneInput.Text.Trim();
            if (phone.Length != 10 || !phone.All(char.IsDigit))
                error += "- Phone must be exactly 10 digits.\n";

            if (!string.IsNullOrEmpty(error))
            {
                ModalErrorText.Text = error;
                ModalErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                _repo.PurchaseEquipment(
                    _selectedItem.ID,
                    SessionManager.CurrentUserID,
                    _selectedItem.Price,
                    ModalAddressInput.Text
                );

                // Refresh nav bar balance and wallet transaction list.
                if (App._window.Content is NavigationPage navPage)
                    navPage.ViewModel.RefreshWallet();

                ShippingModal.Visibility = Visibility.Collapsed;

                // Show success message before navigating away.
                var dialog = new ContentDialog
                {
                    Title = "Purchase successful",
                    Content = $"\"{_selectedItem.Title}\" has been purchased and added to your inventory.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();

                if (this.Parent is ContentControl contentArea)
                    contentArea.Content = new MarketplacePage();
            }
            catch (Exception ex)
            {
                ModalErrorText.Text = "Transaction failed: " + ex.Message;
                ModalErrorText.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
                contentArea.Content = new MarketplacePage();
        }
    }
}