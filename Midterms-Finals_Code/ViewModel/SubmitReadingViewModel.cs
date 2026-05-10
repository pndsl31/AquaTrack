using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace AquaTrack.ViewModel
{
    public class SubmitReadingViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-2SQJPO3\SQLEXPRESS;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public HouseholdModel HouseHold { get; }
        public MeterModel Meter { get; }
        public NavBarViewModel NavBar { get; }

        public ObservableCollection<UsageModel> RecentReadings { get; } = new();

        private string _consumption = "";
        private string _readDate = DateTime.Today.ToString("yyyy-MM-dd");
        private string _message = "";
        private bool _isSuccess;
        private string _meterDisplay = "—";

        public string Consumption
        {
            get => _consumption;
            set { _consumption = value; OnPropertyChanged(); }
        }
        public string ReadDate
        {
            get => _readDate;
            set { _readDate = value; OnPropertyChanged(); }
        }
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }
        public bool IsSuccess
        {
            get => _isSuccess;
            set { _isSuccess = value; OnPropertyChanged(); }
        }
        public string MeterDisplay
        {
            get => _meterDisplay;
            set { _meterDisplay = value; OnPropertyChanged(); }
        }

        public RelayCommand SubmitCommand { get; }

        public SubmitReadingViewModel(Account user, HouseholdModel household, MeterModel meter, Window win)
        {
            CurrentUser = user;
            HouseHold = household;
            Meter = meter;
            NavBar = new NavBarViewModel(user, household, win, meter);

            MeterDisplay = string.IsNullOrWhiteSpace(meter.MeterNumber) ? "—" : meter.MeterNumber;
            SubmitCommand = new RelayCommand(async _ => await SubmitAsync());

            _ = LoadRecentAsync();
        }

        private async Task SubmitAsync()
        {
            Message = "";

            if (!decimal.TryParse(Consumption, out decimal c) || c <= 0)
            {
                IsSuccess = false;
                Message = "⚠ Please enter a valid consumption amount greater than 0.";
                return;
            }

            if (!DateTime.TryParse(ReadDate, out DateTime rd) || rd > DateTime.Today)
            {
                IsSuccess = false;
                Message = "⚠ Please enter a valid date that is not in the future.";
                return;
            }

            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("sp_SubmitMeterReading", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Account_Number", CurrentUser.AccountNumber);
                cmd.Parameters.AddWithValue("@Read_Date", rd.Date);
                cmd.Parameters.AddWithValue("@Consumption", c);
                await cmd.ExecuteNonQueryAsync();

                decimal amt = c * 23.00m;
                bool excessive = c > 30;

                IsSuccess = true;
                Message = $"✔ Reading submitted! Bill: PHP {amt:F2}. Due: {rd.AddDays(30):MMM dd, yyyy}.";
                if (excessive)
                    Message += " ⚠ Usage flagged as Excessive — an alert has been raised.";

                Consumption = "";
                ReadDate = DateTime.Today.ToString("yyyy-MM-dd");
                await LoadRecentAsync();
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                Message = "⚠ " + ex.Message;
            }
        }

        private async Task LoadRecentAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                const string q = @"
                    SELECT TOP 10 u.Usage_ID, u.Read_Date, u.Consumption, u.Usage_Status, u.Meter_ID
                    FROM Usage u
                    INNER JOIN Meter m ON u.Meter_ID = m.Meter_ID
                    WHERE m.Household_ID = @HH
                    ORDER BY u.Read_Date DESC";

                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
                using var r = await cmd.ExecuteReaderAsync();

                RecentReadings.Clear();
                while (await r.ReadAsync())
                    RecentReadings.Add(new UsageModel
                    {
                        UsageID = r["Usage_ID"].ToString()!,
                        ReadDate = Convert.ToDateTime(r["Read_Date"]).ToString("MMM dd, yyyy"),
                        Consumption = Convert.ToDecimal(r["Consumption"]),
                        UsageStatus = r["Usage_Status"].ToString()!,
                        MeterID = r["Meter_ID"].ToString()!
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Recent readings load error: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}