using AquaTrack.Model;
using AquaTrack.View;
using System;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class NavBarViewModel : ObservableObject
    {
        private Window _currentWindow;
        Account CurrentUser { get; set; }
        HouseholdModel HouseHold { get; set; }
        public MeterModel Meter { get; set; }

        public ICommand GoToSubmitMeterCommand { get; set; }
        public ICommand GoToMainDashboardCommand { get; set; }
        public ICommand GoToBillingCommand { get; set; }
        public ICommand GoToAlertsCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public NavBarViewModel(Account currentUser, HouseholdModel household, Window currentWindow, MeterModel meter)
        {
            CurrentUser = currentUser;
            HouseHold = household;
            Meter = meter;
            _currentWindow = currentWindow;

            GoToMainDashboardCommand = new RelayCommand(GoToMainDashboard);
            GoToSubmitMeterCommand = new RelayCommand(GoToSubmitMeter);
            GoToBillingCommand = new RelayCommand(GoToBilling);
            GoToAlertsCommand = new RelayCommand(GoToAlerts);
            LogoutCommand = new RelayCommand(Logout);
        }

        public void GoToSubmitMeter(object? parameter)
        {
            var win = new SubmitReading(CurrentUser, HouseHold, Meter);
            win.Show();
            _currentWindow.Close();
        }

        public void GoToMainDashboard(object? parameter)
        {
            var win = new DashboardWindow(CurrentUser, HouseHold, Meter);
            win.Show();
            _currentWindow.Close();
        }

        public void GoToBilling(object? parameter)
        {
            var win = new Billing(CurrentUser, HouseHold, Meter);
            win.Show();
            _currentWindow.Close();
        }

        public void GoToAlerts(object? parameter)
        {
            var win = new Alerts(CurrentUser, HouseHold, Meter);
            win.Show();
            _currentWindow.Close();
        }

        public void Logout(object? parameter)
        {
            var win = new MainWindow();
            win.Show();
            _currentWindow.Close();
        }
    }
}