using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieMarketplace.Models;
using MovieMarketplace.Repositories;
using System.Collections.ObjectModel;

namespace MovieMarketplace.Views
{
    public sealed partial class MySalesPage : Page
    {
        private readonly EquipmentRepository _repo = new EquipmentRepository();
        public ObservableCollection<TransactionView> Sales { get; set; } = new();

        public MySalesPage()
        {
            this.InitializeComponent();
            LoadSales();
        }

        private void LoadSales()
        {
            var data = _repo.FetchSellerSales(SessionManager.CurrentUserID);
            Sales.Clear();
            foreach (var item in data)
            {
                Sales.Add(item);
            }
            SalesList.ItemsSource = Sales;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage));
        }
    }
}