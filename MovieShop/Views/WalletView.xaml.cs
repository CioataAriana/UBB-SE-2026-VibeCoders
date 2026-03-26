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
            _ = ViewModel.LoadTransactionsAsync();
        }

        public WalletView(WalletViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            _ = ViewModel.LoadTransactionsAsync();
        }
    }
}