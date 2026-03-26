using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MovieShop.ViewModels
{
    public class SellEquipmentViewModel : INotifyPropertyChanged
    {
        
        private string _newItemTitle = string.Empty;
        private string _newItemDesc = string.Empty;
        private string _priceInput = string.Empty;
        private decimal _validatedPrice;
        private string _priceErrorMessage = string.Empty;
        private bool _canPost;

        // 1. Titlul Echipamentului
        public string NewItemTitle
        {
            get => _newItemTitle;
            set { _newItemTitle = value; OnPropertyChanged(); ValidateForm(); }
        }

        // 2. Descrierea Echipamentului
        public string NewItemDesc
        {
            get => _newItemDesc;
            set { _newItemDesc = value; OnPropertyChanged(); ValidateForm(); }
        }

        // 3. Input-ul de preț (ce scrie utilizatorul în căsuță)
        public string PriceInput
        {
            get => _priceInput;
            set
            {
                _priceInput = value;
                ValidateForm();
                OnPropertyChanged();
            }
        }

        // 4. Mesajul de eroare pentru interfață
        public string PriceErrorMessage
        {
            get => _priceErrorMessage;
            set { _priceErrorMessage = value; OnPropertyChanged(); }
        }

        // 5. Starea butonului de Submit (True doar dacă totul e valid)
        public bool CanPost
        {
            get => _canPost;
            set { _canPost = value; OnPropertyChanged(); }
        }

        // 6. Prețul final convertit (pe care îl trimitem la Repository)
        public decimal ValidatedPrice => _validatedPrice;

        /// <summary>
        /// Logica de validare combinată: verifică prețul și dacă restul câmpurilor sunt completate.
        /// </summary>
        private void ValidateForm()
        {
            bool isPriceValid = decimal.TryParse(_priceInput, out decimal result);
            bool isTitleValid = !string.IsNullOrWhiteSpace(_newItemTitle);

            // Validare numerică
            if (!isPriceValid && !string.IsNullOrEmpty(_priceInput))
            {
                PriceErrorMessage = "Please enter a valid numeric price!";
                CanPost = false;
                return;
            }

            // Validare valoare pozitivă
            if (isPriceValid && result <= 0)
            {
                PriceErrorMessage = "Price must be greater than 0!";
                CanPost = false;
                return;
            }

            // Dacă prețul e ok, verificăm și restul formularului
            if (isPriceValid && isTitleValid)
            {
                _validatedPrice = result;
                PriceErrorMessage = string.Empty;
                CanPost = true; 
            }
            else
            {
                CanPost = false; 
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}