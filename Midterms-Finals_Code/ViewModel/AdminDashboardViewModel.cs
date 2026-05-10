using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class AdminDashboardViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-2SQJPO3\SQLEXPRESS;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }

        private int _totalHouseholds, _totalMeters, _unpaidBills, _pendingAlerts;
        public int TotalHouseholds { get => _totalHouseholds; set { _totalHouseholds = value; OnPropertyChanged(); } }
        public int TotalMeters { get => _totalMeters; set { _totalMeters = value; OnPropertyChanged(); } }
        public int UnpaidBills { get => _unpaidBills; set { _unpaidBills = value; OnPropertyChanged(); } }
        public int PendingAlerts { get => _pendingAlerts; set { _pendingAlerts = value; OnPropertyChanged(); } }

        public AdminDashboardViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            _ = LoadStatsAsync();
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                async Task<int> Count(string sql)
                {
                    using var c = new SqlCommand(sql, conn);
                    return (int)(await c.ExecuteScalarAsync() ?? 0);
                }

                TotalHouseholds = await Count("SELECT COUNT(*) FROM HouseHold WHERE IsActive = 1");
                TotalMeters = await Count("SELECT COUNT(*) FROM Meter");
                UnpaidBills = await Count("SELECT COUNT(*) FROM Bill WHERE Status = 'Unpaid'");
                PendingAlerts = await Count("SELECT COUNT(*) FROM Alert WHERE Status = 'Pending'");
            }
            catch (Exception ex) { MessageBox.Show("Stats error: " + ex.Message); }
        }
    }
}