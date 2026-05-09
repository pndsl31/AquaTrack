using AquaTrack.Model;
using AquaTrack.View;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class AdminNavBarViewModel : ObservableObject
    {
        private readonly Window _win;
        private readonly Account _user;

        public ICommand GoToDashboardCommand { get; }
        public ICommand GoToAccountsCommand { get; }
        public ICommand GoToHouseholdsCommand { get; }
        public ICommand GoToMetersCommand { get; }
        public ICommand GoToUsageCommand { get; }
        public ICommand GoToBillingCommand { get; }
        public ICommand GoToAlertsCommand { get; }
        public ICommand GoToReportsCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminNavBarViewModel(Account user, Window win)
        {
            _user = user; _win = win;
            GoToDashboardCommand = new RelayCommand(_ => Navigate(new AdminDashboardWindow(_user)));
            GoToAccountsCommand = new RelayCommand(_ => Navigate(new AccountManagementWindow(_user)));
            GoToHouseholdsCommand = new RelayCommand(_ => Navigate(new HouseholdManagementWindow(_user)));
            GoToMetersCommand = new RelayCommand(_ => Navigate(new MeterManagementWindow(_user)));
            GoToUsageCommand = new RelayCommand(_ => Navigate(new UsageRecordingWindow(_user)));
            GoToBillingCommand = new RelayCommand(_ => Navigate(new AdminBillingWindow(_user)));
            GoToAlertsCommand = new RelayCommand(_ => Navigate(new AdminAlertsWindow(_user)));
            GoToReportsCommand = new RelayCommand(_ => Navigate(new AdminReportsWindow(_user)));
            LogoutCommand = new RelayCommand(_ => Navigate(new MainWindow()));
        }

        private void Navigate(Window next) { next.Show(); _win.Close(); }
    }
}