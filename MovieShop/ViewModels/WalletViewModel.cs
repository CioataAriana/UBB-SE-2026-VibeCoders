using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MovieShop.Models;
using MovieShop.Repositories;
using CommunityToolkit.Mvvm.Input;

namespace MovieShop.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _currentUserID;

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


        // --- TopUp Form Fields ---
        private string _cardHolderName = string.Empty;
        public string CardHolderName
        {
            get => _cardHolderName;
            set { _cardHolderName = value; OnPropertyChanged(nameof(CardHolderName)); }
        }

        private string _cardNumber = string.Empty;
        public string CardNumber
        {
            get => _cardNumber;
            set
            {
                _cardNumber = value;
                OnPropertyChanged(nameof(CardNumber));
            }
        }

        private string _expirationDate = string.Empty;
        public string ExpirationDate
        {
            get => _expirationDate;
            set { _expirationDate = value; OnPropertyChanged(nameof(ExpirationDate)); }
        }

        private string _cvv = string.Empty;
        public string CVV
        {
            get => _cvv;
            set { _cvv = value; OnPropertyChanged(nameof(CVV)); }
        }

        // --- TopUpAmount as double for NumberBox binding ---
        private double _topUpAmount;
        public double TopUpAmount
        {
            get => _topUpAmount;
            set { _topUpAmount = value; OnPropertyChanged(nameof(TopUpAmount)); }
        }

        // --- Feedback Messages ---
        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        private string _successMessage = string.Empty;
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(nameof(SuccessMessage)); }
        }

        // --- Loading State ---
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        // --- Transaction History ---
        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set { _transactions = value; OnPropertyChanged(nameof(Transactions)); }
        }

        // --- Commands ---
        public IRelayCommand TopUpCommand { get; }
        public IAsyncRelayCommand LoadTransactionsCommand { get; }

        // --- Repos ---
        private readonly TransactionRepo _transactionRepo = new TransactionRepo();
        private readonly UserRepo _userRepo = new UserRepo();

        // --- Constructor ---
        public WalletViewModel(int userID, decimal currentBalance)
        {
            _currentUserID = userID;
            _balance = currentBalance;
            _transactions = new ObservableCollection<Transaction>();
            TopUpCommand = new RelayCommand(ExecuteTopUp);
            LoadTransactionsCommand = new AsyncRelayCommand(LoadTransactionsAsync);
        }

        // --- Load Transactions ---
        public async Task LoadTransactionsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await Task.Run(() => _transactionRepo.GetTransactionsByUserId(_currentUserID));

                Transactions.Clear();
                foreach (var t in result)
                    Transactions.Add(t);
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Failed to load transactions: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LogTopUpTransaction(decimal amount)
        {
            var transaction = new Transaction
            {
                BuyerID = new User { ID = _currentUserID },
                Amount = amount,
                Type = "TopUp",
                Status = "Completed",
                Timestamp = System.DateTime.Now
            };

            Task.Run(() => _transactionRepo.LogTransaction(transaction));

            Transactions.Insert(0, transaction);
        }

        private void SortTransactions()
        {
            var sorted = Transactions.OrderByDescending(t => t.Timestamp).ToList();
            Transactions.Clear();
            foreach (var t in sorted)
                Transactions.Add(t);
        }

        // --- TopUp Logic ---
        private void ExecuteTopUp()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!ValidateCard())
                return;

            UpdateBalance((decimal)TopUpAmount);
            LogTopUpTransaction((decimal)TopUpAmount);

            SuccessMessage = $"Successfully added {TopUpAmount:C} to your wallet!";
            ClearForm();
        }

        // --- Validation ---
        private bool ValidateCard()
        {
            if (string.IsNullOrWhiteSpace(CardHolderName))
            {
                ErrorMessage = "Please enter the cardholder name.";
                return false;
            }

            foreach (char c in CardHolderName)
            {
                if (!char.IsLetter(c) && c != ' ')
                {
                    ErrorMessage = "Cardholder name can only contain letters and spaces.";
                    return false;
                }
            }

            var parts = CardHolderName.Trim().Split(' ');
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                ErrorMessage = "Please enter both first and last name.";
                return false;
            }

            foreach (var part in parts)
            {
                if (part.Length < 2)
                {
                    ErrorMessage = "Each name must be at least 2 characters long.";
                    return false;
                }
            }

            if (CardNumber.Length != 16 || !long.TryParse(CardNumber, out _))
            {
                ErrorMessage = "Card number must be exactly 16 digits.";
                return false;
            }

            if (!System.DateTime.TryParseExact(ExpirationDate, "MM/yy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var expDate))
            {
                ErrorMessage = "Invalid expiration date. Use MM/YY format.";
                return false;
            }

            var lastDayOfMonth = new System.DateTime(expDate.Year, expDate.Month,
                System.DateTime.DaysInMonth(expDate.Year, expDate.Month));

            if (lastDayOfMonth < System.DateTime.Now)
            {
                ErrorMessage = "Your card has expired.";
                return false;
            }

            if (CVV.Length != 3 || !int.TryParse(CVV, out _))
            {
                ErrorMessage = "CVV must be exactly 3 digits.";
                return false;
            }

            if (TopUpAmount <= 0)
            {
                ErrorMessage = "Amount must be greater than 0.";
                return false;
            }

            return true;
        }

        // --- Helpers ---
        private void UpdateBalance(decimal amount)
        {
            Balance += amount;
            _userRepo.UpdateBalance(_currentUserID, Balance);
        }

        private void ClearForm()
        {
            CardHolderName = string.Empty;
            CardNumber = string.Empty;
            ExpirationDate = string.Empty;
            CVV = string.Empty;
            TopUpAmount = 0;
        }

        public void OnTransactionCompleted(decimal amount)
        {
            Balance += amount;
            _userRepo.UpdateBalance(_currentUserID, Balance);
            _ = LoadTransactionsAsync();
        }
    }
}