/*using MovieShop.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public FlashSaleViewModel FlashSaleVM { get; set; }
        private readonly ActiveSalesRepo _salesRepo = new ActiveSalesRepo();
        public MainWindowViewModel()
        {
            var currentSales = _salesRepo.GetCurrentSales();
            var activeSale = currentSales.FirstOrDefault();

            //if activeSale is null we pass datetime.now to keep it inactive
            DateTime expiry = activeSale?.EndTime ?? DateTime.Now;

            FlashSaleVM = new FlashSaleViewModel(expiry, () =>
                {
                    System.Diagnostics.Debug.WriteLine("MainVM noticed the sale ended");

                });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
*/