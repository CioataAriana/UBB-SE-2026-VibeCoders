using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieMarketplace.Models;
using MovieMarketplace.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MovieMarketplace.Views
{
    public sealed partial class MyOrdersPage : Page
    {
        private readonly EquipmentRepository _repo = new EquipmentRepository();

        public ObservableCollection<TransactionView> Transactions { get; set; } = new();

        public MyOrdersPage()
        {
            this.InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            var data = _repo.FetchUserOrders(SessionManager.CurrentUserID);

            Transactions.Clear();

            foreach (var item in data)
            {
                Transactions.Add(item);
            }

            OrdersList.ItemsSource = null;
            OrdersList.ItemsSource = Transactions;
        }

        private async void ConfirmDelivery_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int transactionId)
            {
                try
                {
                    _repo.ConfirmDelivery(transactionId);

                    ContentDialog success = new ContentDialog
                    {
                        Title = "Livrare Confirmată",
                        Content = "Tranzacția a fost finalizată cu succes. Vânzătorul a primit banii.",
                        CloseButtonText = "Ok",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await success.ShowAsync();

                    LoadOrders();
                }
                catch (Exception ex)
                {
                    ContentDialog errorDlg = new ContentDialog
                    {
                        Title = "Eroare",
                        Content = "Nu s-a putut confirma livrarea: " + ex.Message,
                        CloseButtonText = "Ok",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorDlg.ShowAsync();
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(StartPage));
        }
    }
}