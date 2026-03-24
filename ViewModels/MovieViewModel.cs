using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Windows.ApplicationModel.Background;

namespace MovieShop.ViewModels
{
    public class MovieViewModel : INotifyPropertyChanged
    {
        private int _saleID;
        private decimal _basePrice;
        private decimal _discountPercent;

        private DateTime _expiryDate;

        private FlashSaleViewModel _saleTimer;
        public FlashSaleViewModel SaleTimer
        {
            get => _saleTimer;
            set
            {
                _saleTimer = value;
                OnPropertyChanged();
            }
        }

        public int SaleID
        {
            get => _saleID;
            set
            {
                _saleID = value;
                OnPropertyChanged(); //announces that SaleID changed
                OnPropertyChanged(nameof(IsSaleActive)); //announces that IsSaleActive also changed
                OnPropertyChanged(nameof(DisplayPrice));
            }
        }

        public decimal BasePrice
        {
            get => _basePrice;
            set
            {
                _basePrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayPrice));
            }
        }

        public decimal SalePrice
        {
            get => BasePrice * (1 - (_discountPercent / 100.0m));
        }

        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                _discountPercent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SalePrice));
            }
        }

        //bool prop returns true during active sale
        public bool IsSaleActive
        {
            get
            {
                if(_saleID !=  0)
                    return true;
                else
                    return false;
            }
        }

        public decimal DisplayPrice => IsSaleActive ? SalePrice : BasePrice;

        //task 46

        public void RevertToOriginalPrice()
        {
            this.SaleID = 0;
            this.SaleTimer = null;
        }

        public void ApplyDatabaseSale(decimal discountPercent, DateTime expiryDate )
        {
            this.DiscountPercent = discountPercent;
            this.SaleID = 1;

            this._expiryDate = expiryDate;

            this.SaleTimer = new FlashSaleViewModel(expiryDate, this.RevertToOriginalPrice);
        }

        public string SaleBadgeText => IsSaleActive ? $"{DiscountPercent:0}% OFF" : "";
        public bool ShowStrike => IsSaleActive;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


    }
}
