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
                catch { /* Imagine invalidă */ }
            }

            bool canAfford = SessionManager.CurrentUserBalance >= _selectedItem.Price;
            ConfirmBuyButton.IsEnabled = canAfford;
            ErrorText.Visibility = canAfford ? Visibility.Collapsed : Visibility.Visible;
            if (!canAfford) ErrorText.Text = "Insufficient funds in your wallet.";
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Visible;
        private void CancelShipping_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Collapsed;

        private void ConfirmShipping_Click(object sender, RoutedEventArgs e)
        {
            ModalErrorText.Visibility = Visibility.Collapsed;
            string error = "";

            // 1. Validare Nume
            if (string.IsNullOrWhiteSpace(ModalNameInput.Text))
                error += "- Name is required.\n";

            // 2. Validare Adresă (minim 10 caractere)
            if (ModalAddressInput.Text.Length < 10)
                error += "- Address too short (min 10 chars).\n";

            // 3. Validare Telefon (Trebuie să fie exact 10 cifre)
            string phone = ModalPhoneInput.Text.Trim();
            if (phone.Length != 10 || !phone.All(char.IsDigit))
            {
                error += "- Phone must be exactly 10 digits.\n";
            }

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

                if (App._window.Content is MovieShop.Views.NavigationPage navPage)
                {
                    navPage.ViewModel.Balance -= _selectedItem.Price;

                    if (navPage.ViewModel.CurrentViewModel is WalletViewModel walletVM)
                    {
                        walletVM.Balance = navPage.ViewModel.Balance;
                        _ = walletVM.LoadTransactionsAsync();
                    }
                }

                ShippingModal.Visibility = Visibility.Collapsed;
                if (this.Parent is ContentControl contentArea)
                {
                    contentArea.Content = new MarketplacePage();
                }
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
            {
                contentArea.Content = new MarketplacePage();
            }
        }
    }
}