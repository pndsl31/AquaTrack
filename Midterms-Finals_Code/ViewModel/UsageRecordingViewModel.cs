using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class UsageRecordingViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-6085EPQ;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }
        public ObservableCollection<HouseholdModel> Households { get; } = new();
        public ObservableCollection<UsageModel> RecentUsage { get; } = new();

        private HouseholdModel? _selectedHH;
        public HouseholdModel? SelectedHousehold
        {
            get => _selectedHH;
            set { _selectedHH = value; OnPropertyChanged(); _ = LoadMeterAsync(); }
        }

        private string _meterDisplay = "—", _readDate = DateTime.Today.ToString("yyyy-MM-dd");
        private string _consumption = "", _message = ""; bool _isSuccess;

        public string MeterDisplay { get => _meterDisplay; set { _meterDisplay = value; OnPropertyChanged(); } }
        public string ReadDate { get => _readDate; set { _readDate = value; OnPropertyChanged(); } }
        public string Consumption { get => _consumption; set { _consumption = value; OnPropertyChanged(); } }
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
        public bool IsSuccess { get => _isSuccess; set { _isSuccess = value; OnPropertyChanged(); } }

        public ICommand RecordCommand { get; }

        public UsageRecordingViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            RecordCommand = new RelayCommand(async _ => await RecordAsync());
            _ = LoadHouseholdsAsync();
        }

        private async Task LoadHouseholdsAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = "SELECT Household_ID, Owner_Name FROM HouseHold WHERE IsActive = 1 ORDER BY Household_ID";
                using var cmd = new SqlCommand(q, conn);
                using var r = await cmd.ExecuteReaderAsync();
                Households.Clear();
                while (await r.ReadAsync())
                    Households.Add(new HouseholdModel
                    {
                        HouseholdID = r["Household_ID"].ToString()!,
                        OwnerName = r["Owner_Name"].ToString()!
                    });
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private async Task LoadMeterAsync()
        {
            if (SelectedHousehold == null) { MeterDisplay = "—"; return; }
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = "SELECT Meter_Number FROM Meter WHERE Household_ID = @HH AND Status = 'Active'";
                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@HH", SelectedHousehold.HouseholdID);
                var result = await cmd.ExecuteScalarAsync();
                MeterDisplay = result?.ToString() ?? "No active meter";
            }
            catch { MeterDisplay = "Error"; }
        }

        private async Task RecordAsync()
        {
            Message = "";
            if (SelectedHousehold == null)
            { IsSuccess = false; Message = "⚠ Select a household."; return; }
            if (!decimal.TryParse(Consumption, out decimal c) || c <= 0)
            { IsSuccess = false; Message = "⚠ Consumption must be a positive number."; return; }
            if (!DateTime.TryParse(ReadDate, out DateTime rd) || rd > DateTime.Today)
            { IsSuccess = false; Message = "⚠ Invalid or future date."; return; }

            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                // Get Meter_ID from household
                string? meterID;
                using (var cmd2 = new SqlCommand("SELECT Meter_ID FROM Meter WHERE Household_ID = @HH AND Status='Active'", conn))
                {
                    cmd2.Parameters.AddWithValue("@HH", SelectedHousehold.HouseholdID);
                    meterID = (await cmd2.ExecuteScalarAsync())?.ToString();
                }
                if (meterID == null) { IsSuccess = false; Message = "⚠ No active meter for this household."; return; }

                using var cmd = new SqlCommand("sp_RecordUsage", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Meter_ID", meterID);
                cmd.Parameters.AddWithValue("@Read_Date", rd.Date);
                cmd.Parameters.AddWithValue("@Consumption", c);
                await cmd.ExecuteNonQueryAsync();

                decimal amt = c * 23.00m;
                bool excessive = c > 30;
                IsSuccess = true;
                Message = $"✔ Recorded! Bill: PHP {amt:F2}. Due: {rd.AddDays(30):MMM dd, yyyy}.";
                if (excessive) Message += " ⚠ Excessive — alert raised.";
                Consumption = "";
                await LoadRecentAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }

        private async Task LoadRecentAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = @"
                    SELECT TOP 10 u.Usage_ID, u.Read_Date, u.Consumption, u.Usage_Status,
                           h.Owner_Name
                    FROM Usage u
                    INNER JOIN Meter m ON u.Meter_ID = m.Meter_ID
                    INNER JOIN HouseHold h ON m.Household_ID = h.Household_ID
                    ORDER BY u.Read_Date DESC";
                using var cmd = new SqlCommand(q, conn);
                using var r = await cmd.ExecuteReaderAsync();
                RecentUsage.Clear();
                while (await r.ReadAsync())
                    RecentUsage.Add(new UsageModel
                    {
                        UsageID = r["Usage_ID"].ToString()!,
                        ReadDate = Convert.ToDateTime(r["Read_Date"]).ToString("MMM dd, yyyy"),
                        Consumption = Convert.ToDecimal(r["Consumption"]),
                        UsageStatus = r["Usage_Status"].ToString()!,
                        OwnerName = r["Owner_Name"].ToString()!
                    });
            }
            catch (Exception ex) { MessageBox.Show("Recent load error: " + ex.Message); }
        }
    }
}