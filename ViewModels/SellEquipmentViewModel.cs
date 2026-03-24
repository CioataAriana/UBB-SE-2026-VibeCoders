using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.ViewModels
{
    public class SellEquipmentViewModel : INotifyPropertyChanged
    {
        private decimal _newItemPrice;
        private string _priceErrorMessage;
        private bool _canPost;

        private string _priceInput = string.Empty;

        public string PriceInput
        {
            get => _priceInput;
            set
            {
                _priceInput = value;
                ValidatePrice();
                OnPropertyChanged();
            }
        }

        public string PriceErrorMessage
        {
            get => _priceErrorMessage;
            set
            {
                _priceErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool CanPost
        {
            get => _canPost;
            set
            {
                _canPost = value;
                OnPropertyChanged();
            }
        }

        public decimal ValidatedPrice => _newItemPrice;

        private void ValidatePrice()
        {
            if (!decimal.TryParse(_priceInput, out decimal result))
            {
                PriceErrorMessage = "Please enter a valid numeric price!";
                CanPost = false;
                return;
            }

            if (result <= 0)
            {
                PriceErrorMessage = "Price must be greater than 0!";
                CanPost = false;
            }

            else
            {
                _newItemPrice = result; //the conversion happens here
                PriceErrorMessage = string.Empty;
                CanPost = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
