using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieMarketplace.Models;
using MovieMarketplace.Repositories;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace MovieMarketplace.Views
{
    public class SellViewModel : INotifyPropertyChanged
    {
        private string _newItemTitle = string.Empty;
        private string _newItemPrice = string.Empty;
        private string _newItemDesc = string.Empty;

        public string NewItemTitle { get => _newItemTitle; set { _newItemTitle = value; OnPropertyChanged(); } }
        public string NewItemPrice { get => _newItemPrice; set { _newItemPrice = value; OnPropertyChanged(); } }
        public string NewItemDesc { get => _newItemDesc; set { _newItemDesc = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed partial class SellPage : Page
    {
        private readonly EquipmentRepository _repo = new EquipmentRepository();
        public SellViewModel ViewModel { get; set; } = new SellViewModel();
        private string _selectedLocalPath = string.Empty;

        public SellPage()
        {
            this.InitializeComponent();
        }

        private async void ImageUploadPlaceholder_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                _selectedLocalPath = file.Path;
                FileNameLabel.Text = $"Selected: {file.Name}";
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ValidationErrorText.Visibility = Visibility.Collapsed;
            decimal validatedPrice = 0m;
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(ViewModel.NewItemTitle) ||
                string.IsNullOrWhiteSpace(ViewModel.NewItemDesc) ||
                CategoryInput.SelectedItem == null ||
                ConditionInput.SelectedItem == null ||
                !decimal.TryParse(ViewModel.NewItemPrice, out validatedPrice))
            {
                hasError = true;
            }

            if (hasError)
            {
                ValidationErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                var newEquip = new Equipment
                {
                    SellerID = SessionManager.CurrentUserID,
                    Title = ViewModel.NewItemTitle,
                    Description = ViewModel.NewItemDesc,
                    Price = validatedPrice,
                    ImageUrl = !string.IsNullOrEmpty(_selectedLocalPath) ? _selectedLocalPath : (ImageUrlInput.Text ?? ""),
                    Status = EquipmentStatus.Available,
                    Category = (CategoryInput.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "",
                    Condition = (ConditionInput.SelectedItem as ComboBoxItem)?.Content.ToString() ?? ""
                };

                _repo.AddEquipment(newEquip);

                ContentDialog success = new ContentDialog
                {
                    Title = "Success",
                    Content = "Item listed!",
                    CloseButtonText = "Ok",
                    XamlRoot = this.Content.XamlRoot
                };
                await success.ShowAsync();
                this.Frame.Navigate(typeof(MarketplacePage));
            }
            catch (Exception ex)
            {
                ContentDialog err = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "Ok",
                    XamlRoot = this.Content.XamlRoot
                };
                await err.ShowAsync();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => this.Frame.Navigate(typeof(MarketplacePage));
    }
}