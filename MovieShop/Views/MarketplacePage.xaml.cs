using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.ViewModels;
using System;

namespace MovieShop.Views
{
    public sealed partial class MarketplacePage : Page
    {
        public MarketplaceViewModel ViewModel { get; } = new MarketplaceViewModel();

        public MarketplacePage()
        {
            this.InitializeComponent();
            ViewModel.LoadData();
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryFilter?.SelectedItem is ComboBoxItem selectedItem)
            {
                string category = selectedItem.Content.ToString();
                ViewModel.FilterByCategory(category == "All" ? null : category);
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Equipment selectedItem)
            {
                if (this.Parent is ContentControl contentArea)
                {
                    contentArea.Content = new EquipmentDetailPage(selectedItem);
                }
            }
        }

        private void GoToSell_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new SellPage();
            }
        }

        private void BackToStart_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new StartPageEquipment();
            }
        }
    }
}