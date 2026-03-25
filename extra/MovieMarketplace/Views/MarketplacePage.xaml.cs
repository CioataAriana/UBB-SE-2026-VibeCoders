using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieMarketplace.Models;
using System;

namespace MovieMarketplace.Views
{
    public sealed partial class MarketplacePage : Page
    {
        public MovieMarketplace.ViewModels.MarketplaceViewModel ViewModel { get; } = new();

        public MarketplacePage()
        {
            this.InitializeComponent();
            ViewModel.LoadData();
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilter == null || CategoryFilter.SelectedItem == null) return;

            if (CategoryFilter.SelectedItem is ComboBoxItem selectedItem)
            {
                string category = selectedItem.Content.ToString();

                if (category == "All") category = null;

                ViewModel.FilterByCategory(category);
            }
        }

        private void GoToSell_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SellPage));
        }

        private void BackToStart_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage));
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Equipment selectedItem)
            {
                this.Frame.Navigate(typeof(EquipmentDetailPage), selectedItem);
            }
        }
    }
}