using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using MovieShop.Models;

namespace MovieShop.ViewModels
{
    public class ConfirmReceiptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int _currentUserID;

        // --- Transaction being confirmed ---
        private Transaction _transaction;
        public Transaction Transaction
        {
            get => _transaction;
            set { _transaction = value; OnPropertyChanged(nameof(Transaction)); }
        }

        // --- Seller balance (will increase after confirmation) ---
        private decimal _sellerBalance;
        public decimal SellerBalance
        {
            get => _sellerBalance;
            set { _sellerBalance = value; OnPropertyChanged(nameof(SellerBalance)); }
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

        // --- Visibility ---
        private bool _isConfirmButtonVisible;
        public bool IsConfirmButtonVisible
        {
            get => _isConfirmButtonVisible;
            set { _isConfirmButtonVisible = value; OnPropertyChanged(nameof(IsConfirmButtonVisible)); }
        }

        private bool _isSuccessVisible;
        public bool IsSuccessVisible
        {
            get => _isSuccessVisible;
            set { _isSuccessVisible = value; OnPropertyChanged(nameof(IsSuccessVisible)); }
        }

        // --- Commands ---
        public IRelayCommand ConfirmReceiptCommand { get; }
        public IRelayCommand DismissSuccessCommand { get; }

        // --- Constructor ---
        public ConfirmReceiptViewModel(int userID, Transaction transaction, decimal sellerBalance)
        {
            _currentUserID = userID;
            _transaction = transaction;
            _sellerBalance = sellerBalance;

            // Only show confirm button if transaction is still pending
            IsConfirmButtonVisible = transaction.Status == "Pending";

            ConfirmReceiptCommand = new RelayCommand(ConfirmReceipt);
            DismissSuccessCommand = new RelayCommand(DismissSuccess);
        }

        // --- Confirm Receipt Logic ---
        private void ConfirmReceipt()
        {
            ErrorMessage = string.Empty;

            if (Transaction == null)
            {
                ErrorMessage = "No transaction found.";
                return;
            }

            if (Transaction.Status != "Pending")
            {
                ErrorMessage = "This transaction has already been completed.";
                return;
            }

            if (Transaction.BuyerID.ID != _currentUserID)
            {
                ErrorMessage = "Only the buyer can confirm receipt.";
                return;
            }

            // Release escrow — pay the seller
            ReleaseEscrowToSeller();

            // Mark buyer transaction as Completed
            UpdateTransactionStatus("Completed");

            // Hide confirm button since it's done
            IsConfirmButtonVisible = false;
            IsSuccessVisible = true;
            SuccessMessage = "Receipt confirmed! The seller has been paid.";
        }

        // --- Helpers ---
        private void ReleaseEscrowToSeller()
        {
            if (Transaction.SellerID!= null)
            {
                SellerBalance += Transaction.Amount;
                // TODO: call UserRepository.UpdateBalance(Transaction.SellerID.Value, SellerBalance)
            }
        }

        private void UpdateTransactionStatus(string newStatus)
        {
            Transaction.Status = newStatus;
            OnPropertyChanged(nameof(Transaction));
            // TODO: call TransactionRepository.UpdateStatus(Transaction.TransactionID, newStatus)
        }

        private void DismissSuccess()
        {
            IsSuccessVisible = false;
            SuccessMessage = string.Empty;
        }
    }
}