using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieMarketplace.Models;
using MovieMarketplace.Repositories;
using MovieMarketplace.ViewModels;
using System;

namespace MovieMarketplace.Views
{
    public sealed partial class EquipmentDetailPage : Page
    {
        private readonly EquipmentRepository _repo = new EquipmentRepository();
        public BuyViewModel ViewModel { get; set; } = new BuyViewModel();

        public EquipmentDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Equipment item)
            {
                ViewModel.SelectedItem = item;
                this.Loaded += (s, a) => PopulateUI();
            }
        }

        private void PopulateUI()
        {
            if (ViewModel.SelectedItem == null) return;
            TitleLabel.Text = ViewModel.SelectedItem.Title;
            DescriptionLabel.Text = ViewModel.SelectedItem.Description ?? "No description.";
            CategoryLabel.Text = ViewModel.SelectedItem.Category;
            ConditionLabel.Text = ViewModel.SelectedItem.Condition;
            PriceLabel.Text = $"Price: ${ViewModel.SelectedItem.Price:F2}";

            if (!string.IsNullOrEmpty(ViewModel.SelectedItem.ImageUrl))
                ItemImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(ViewModel.SelectedItem.ImageUrl));

            // Validare buton (VIB-36)
            BuyButton.IsEnabled = ViewModel.CanPurchase;
            ErrorText.Visibility = ViewModel.CanPurchase ? Visibility.Collapsed : Visibility.Visible;
            if (!ViewModel.CanPurchase)
                ErrorText.Text = SessionManager.CurrentUserID <= 0 ? "Please log in." : "Insufficient funds.";
        }

        // Deschide Modalul Custom (VIB-73)
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            ShippingModal.Visibility = Visibility.Visible;
        }

        private void CancelShipping_Click(object sender, RoutedEventArgs e)
        {
            ShippingModal.Visibility = Visibility.Collapsed;
        }

        // Validare și Finalizare (VIB-37 & VIB-39)
        private void ConfirmShipping_Click(object sender, RoutedEventArgs e)
        {
            ModalErrorText.Visibility = Visibility.Collapsed;
            string error = "";

            if (string.IsNullOrWhiteSpace(ModalNameInput.Text)) error += "- Name is required.\n";
            if (ModalAddressInput.Text.Length < 10) error += "- Address too short (min 10).\n";
            if (ModalPhoneInput.Text.Length != 10) error += "- Phone must be 10 digits.\n";

            if (!string.IsNullOrEmpty(error))
            {
                ModalErrorText.Text = error;
                ModalErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // VIB-39: Status Update in DB
                _repo.PurchaseEquipment(
                    ViewModel.SelectedItem.ID,
                    SessionManager.CurrentUserID,
                    ViewModel.SelectedItem.Price,
                    ModalAddressInput.Text
                );

                ShippingModal.Visibility = Visibility.Collapsed;
                this.Frame.Navigate(typeof(MarketplacePage));
            }
            catch (Exception ex)
            {
                ModalErrorText.Text = ex.Message;
                ModalErrorText.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => this.Frame.GoBack();
    }
}