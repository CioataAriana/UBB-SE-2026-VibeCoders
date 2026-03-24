using Microsoft.UI.Xaml.Controls;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class WalletView : Page
    {
        public WalletViewModel ViewModel { get; } = new WalletViewModel(1, 500.00m);

        public WalletView()
        {
            this.InitializeComponent();
            LoadIfEmpty();
        }

        public WalletView(WalletViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            LoadIfEmpty();
        }

        private void LoadIfEmpty()
        {
            // Only load mock data if no transactions exist yet
            if (ViewModel.Transactions.Count == 0)
            {
                _ = ViewModel.LoadTransactionsAsync();
            }
        }
    }
}