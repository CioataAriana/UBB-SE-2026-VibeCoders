using CommunityToolkit.Mvvm.Input;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;

namespace MovieShop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set { _currentViewModel = value; OnPropertyChanged(nameof(CurrentViewModel)); }
        }

        public FlashSaleViewModel FlashSaleVM { get; set; }
        private readonly ActiveSalesRepo _salesRepo = new ActiveSalesRepo();

        // --- Balance ---
        private decimal _balance = 500.00m;
        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged(nameof(Balance));
                OnPropertyChanged(nameof(DisplayBalance));
            }
        }

        public string DisplayBalance => Balance.ToString("C");

        private readonly int _currentUserID = 1;

        private WalletViewModel _walletViewModel;
        private MovieViewModel _shopViewModel;

        public IRelayCommand NavigateToShopCommand { get; }
        public IRelayCommand NavigateToWalletCommand { get; }
        public IRelayCommand NavigateToMarketplaceCommand { get; }
        public IRelayCommand NavigateToInventoryCommand { get; }

        public MainViewModel()
        {

            //SALE LOGIC
            var currentSales = _salesRepo.GetCurrentSales();
            var activeSale = currentSales.FirstOrDefault();

            //if activeSale is null we pass datetime.now to keep it inactive
            DateTime expiry = activeSale?.EndTime ?? DateTime.Now;

            FlashSaleVM = new FlashSaleViewModel(expiry, () =>
            {
                System.Diagnostics.Debug.WriteLine("MainVM noticed the sale ended");

            });

            MovieShop.Services.SaleService.CurrentSale = this.FlashSaleVM;

            //----------------------------

            _walletViewModel = new WalletViewModel(_currentUserID, _balance);
            _walletViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WalletViewModel.Balance))
                    Balance = _walletViewModel.Balance;
            };

            NavigateToShopCommand = new RelayCommand(NavigateToShop);
            NavigateToWalletCommand = new RelayCommand(NavigateToWallet);
            NavigateToMarketplaceCommand = new RelayCommand(NavigateToMarketplace);
            NavigateToInventoryCommand = new RelayCommand(NavigateToInventory);

            NavigateToShop();
        }

        private void NavigateToShop() => CurrentViewModel = "Shop";
        private void NavigateToWallet() => CurrentViewModel = _walletViewModel;
        private void NavigateToMarketplace() => CurrentViewModel = "Marketplace";
        private void NavigateToInventory() => CurrentViewModel = "Inventory";
        private void NavigateToSales() => CurrentViewModel = "SalesPage";

    }
}