using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;

namespace MovieShop.Views
{
    public sealed partial class SellPage : Page
    {
        private readonly EquipmentRepo _repo = new EquipmentRepo();
        public SellEquipmentViewModel ViewModel { get; set; } = new SellEquipmentViewModel();
        private string _selectedLocalPath = string.Empty;

        public SellPage()
        {
            this.InitializeComponent();
        }

        private async void ImageUploadPlaceholder_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var picker = new FileOpenPicker();

            if (App._window != null)
            {
                var hwnd = WindowNative.GetWindowHandle(App._window);
                InitializeWithWindow.Initialize(picker, hwnd);
            }

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                _selectedLocalPath = file.Path;
                FileNameLabel.Text = $"Selected: {file.Name}";
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newItem = new Equipment
                {
                    SellerID = SessionManager.CurrentUserID,
                    Title = ViewModel.NewItemTitle,
                    Description = ViewModel.NewItemDesc,
                    Price = ViewModel.ValidatedPrice,
                    Category = (CategoryInput.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    Condition = (ConditionInput.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    ImageUrl = !string.IsNullOrEmpty(_selectedLocalPath) ? _selectedLocalPath : ImageUrlInput.Text,
                    Status = EquipmentStatus.Available
                };

                _repo.ListItem(newItem);

                if (this.Parent is ContentControl contentArea)
                {
                    contentArea.Content = new StartPageEquipment();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare la salvare: " + ex.Message);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new StartPageEquipment();
            }
        }
    }
}