using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace AquaTrack.ViewModel
{
    internal class DashboardViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-6085EPQ\SQLEXPRESS;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; set; }
        public HouseholdModel HouseHold { get; set; }
        public MeterModel Meter { get; set; }
        public NavBarViewModel NavBar { get; set; }

        // Summary stats shown on the dashboard cards
        private int _unpaidBills;
        private int _pendingAlerts;
        private decimal _totalUnpaid;
        private string _lastReadDate = "—";
        private decimal _lastConsumption;
        private string _lastUsageStatus = "—";

        public int UnpaidBills
        {
            get => _unpaidBills;
            set { _unpaidBills = value; OnPropertyChanged(); }
        }
        public int PendingAlerts
        {
            get => _pendingAlerts;
            set { _pendingAlerts = value; OnPropertyChanged(); }
        }
        public decimal TotalUnpaid
        {
            get => _totalUnpaid;
            set { _totalUnpaid = value; OnPropertyChanged(); }
        }
        public string LastReadDate
        {
            get => _lastReadDate;
            set { _lastReadDate = value; OnPropertyChanged(); }
        }
        public decimal LastConsumption
        {
            get => _lastConsumption;
            set { _lastConsumption = value; OnPropertyChanged(); }
        }
        public string LastUsageStatus
        {
            get => _lastUsageStatus;
            set { _lastUsageStatus = value; OnPropertyChanged(); }
        }

        public DashboardViewModel(Account currentUser, HouseholdModel household, Window currentWindow, MeterModel meter)
        {
            CurrentUser = currentUser;
            HouseHold = household;
            Meter = meter;
            NavBar = new NavBarViewModel(currentUser, household, currentWindow, meter);
            LoadSummaryAsync();
        }

        private async Task LoadSummaryAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                // Unpaid bill count for this household
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Bill WHERE Household_ID = @HH AND Status = 'Unpaid'", conn))
                {
                    cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
                    UnpaidBills = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // Total unpaid amount
                using (var cmd = new SqlCommand(
                    "SELECT dbo.fn_GetTotalUnpaidAmount(@HH)", conn))
                {
                    cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
                    var result = await cmd.ExecuteScalarAsync();
                    TotalUnpaid = result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }

                // Pending alert count for this household
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Alert WHERE Household_ID = @HH AND Status = 'Pending'", conn))
                {
                    cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
                    PendingAlerts = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // Most recent meter reading
                const string recentQ = @"
                    SELECT TOP 1 u.Read_Date, u.Consumption, u.Usage_Status
                    FROM Usage u
                    INNER JOIN Meter m ON u.Meter_ID = m.Meter_ID
                    WHERE m.Household_ID = @HH
                    ORDER BY u.Read_Date DESC";
                using (var cmd = new SqlCommand(recentQ, conn))
                {
                    cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
                    using var r = await cmd.ExecuteReaderAsync();
                    if (await r.ReadAsync())
                    {
                        LastReadDate = Convert.ToDateTime(r["Read_Date"]).ToString("MMM dd, yyyy");
                        LastConsumption = Convert.ToDecimal(r["Consumption"]);
                        LastUsageStatus = r["Usage_Status"].ToString()!;
                    }
                    else
                    {
                        LastReadDate = "No readings yet";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dashboard load error: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}