using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MovieShop.ViewModels
{
    public class FlashSaleViewModel : INotifyPropertyChanged
    {
        //DispatcherTimer because it runs on the UI Thread,
        //which allows it to update the timerText
        //every second without crashing the app.

        private DispatcherTimer _timer;
        private string _displayText;
        private string _timerText;
        private DateTime _expiryDate;
        private bool _isActive;

        private Action _onExpiredAction;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public string TimerText
        {
            get => _timerText;
            set
            {
                _timerText = value;
                OnPropertyChanged();
            }
        }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                OnPropertyChanged();
            }
        }

        // returns visible or collapsed based on IsActive
        public FlashSaleViewModel(DateTime saleEndTime, Action onExpired)
        {
            _onExpiredAction = onExpired;

            if(saleEndTime <= DateTime.Now)
            {
                IsActive = false;
                return;
            }

            _expiryDate = saleEndTime;
            IsActive = true;


            //initialize DispatcherTimer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerTick;
            _timer.Start();

            //Initial calculation
            UpdateCountdown();
        }

        private void TimerTick(object sender, object e)
        {
            UpdateCountdown();
        }

        private void UpdateCountdown()
        {
            TimeSpan timeRemaining = _expiryDate - DateTime.Now;

            if (timeRemaining.TotalSeconds > 0)
            {
                //DoD: TimerText property updates HH:MM::SS every second

                DisplayText = "Flash sale";
                TimerText = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                                (int)timeRemaining.TotalHours,
                                                (int)timeRemaining.Minutes,
                                                (int)timeRemaining.Seconds);
            }
            else
            {
                _timer.Stop();
                DisplayText = "Flash sale has expired!";
                TimerText = "00:00:00";

                IsActive = false;

                _onExpiredAction?.Invoke();

                //HandleSaleExpiry();
            }
        }
        
        private void HandleSaleExpiry()
        {
            Debug.WriteLine("Flash Sale has expired!");
        }

        public Visibility BannerVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
