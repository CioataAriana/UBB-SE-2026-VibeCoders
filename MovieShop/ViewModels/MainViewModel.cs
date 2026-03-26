using CommunityToolkit.Mvvm.Input;
using MovieShop.Models;
using MovieShop.Repositories;
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
        private decimal _balance;
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

        private readonly int _currentUserID = SessionManager.CurrentUserID;
        private readonly UserRepo _userRepo = new UserRepo();

        private WalletViewModel _walletViewModel;
        private MovieViewModel _shopViewModel;

        public IRelayCommand NavigateToShopCommand { get; }
        public IRelayCommand NavigateToWalletCommand { get; }
        public IRelayCommand NavigateToMarketplaceCommand { get; }
        public IRelayCommand NavigateToTicketsCommand { get; }
        public IRelayCommand NavigateToInventoryCommand { get; }

        public MainViewModel()
        {
            if (_currentUserID > 0)
                Balance = _userRepo.GetBalance(_currentUserID);
            else
                Balance = 0;

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

            _walletViewModel = new WalletViewModel(_currentUserID, Balance);
            _walletViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WalletViewModel.Balance))
                    Balance = _walletViewModel.Balance;
            };

            NavigateToShopCommand = new RelayCommand(NavigateToShop);
            NavigateToWalletCommand = new RelayCommand(NavigateToWallet);
            NavigateToMarketplaceCommand = new RelayCommand(NavigateToMarketplace);
            NavigateToTicketsCommand = new RelayCommand(NavigateToTickets);
            NavigateToInventoryCommand = new RelayCommand(NavigateToInventory);

            NavigateToShop();
        }

        public void RefreshBalanceFromDatabase()
        {
            if (_currentUserID <= 0)
            {
                Balance = 0;
                _walletViewModel.Balance = 0;
                return;
            }

            var b = _userRepo.GetBalance(_currentUserID);
            Balance = b;
            _walletViewModel.Balance = b;
        }

        /// <summary>
        /// Refreshes both the nav bar balance and the wallet view model (balance + transaction list).
        /// Call this after any purchase so the wallet stays in sync regardless of which page is active.
        /// </summary>
        public void RefreshWallet()
        {
            RefreshBalanceFromDatabase();
            _ = _walletViewModel.LoadTransactionsAsync();
        }

        private void NavigateToShop() => CurrentViewModel = "Shop";
        private void NavigateToWallet() => CurrentViewModel = _walletViewModel;
        private void NavigateToMarketplace() => CurrentViewModel = "Marketplace";
        private void NavigateToInventory() => CurrentViewModel = "Inventory";
        private void NavigateToTickets() => CurrentViewModel = "Tickets";
        private void NavigateToSales() => CurrentViewModel = "SalesPage";

    }
}