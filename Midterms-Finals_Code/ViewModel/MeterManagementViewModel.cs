using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class MeterManagementViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=Amenoai;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }
        public ObservableCollection<MeterModel> Meters { get; } = new();
        public ObservableCollection<HouseholdModel> AvailableHouseholds { get; } = new();

        private MeterModel? _selected;
        public MeterModel? SelectedMeter { get => _selected; set { _selected = value; OnPropertyChanged(); } }

        private string _meterID = "", _meterNum = "", _location = "", _status = "Active";
        private string _hhID = "", _message = ""; bool _isSuccess;

        public string MeterID { get => _meterID; set { _meterID = value; OnPropertyChanged(); } }
        public string MeterNum { get => _meterNum; set { _meterNum = value; OnPropertyChanged(); } }
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
        public string HhID { get => _hhID; set { _hhID = value; OnPropertyChanged(); } }
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
        public bool IsSuccess { get => _isSuccess; set { _isSuccess = value; OnPropertyChanged(); } }

        public string[] StatusOptions { get; } = { "Active", "Inactive", "Under Maintenance" };

        public ICommand AddMeterCommand { get; }

        public MeterManagementViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            AddMeterCommand = new RelayCommand(async _ => await AddMeterAsync());
            _ = LoadAllAsync();
        }

        private async Task LoadAllAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                const string mq = @"SELECT * FROM Meter ORDER BY Meter_ID";
                using (var cmd = new SqlCommand(mq, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    Meters.Clear();
                    while (await r.ReadAsync())
                        Meters.Add(new MeterModel
                        {
                            MeterID = r["Meter_ID"].ToString()!,
                            MeterNumber = r["Meter_Number"].ToString()!,
                            Location = r["Location"].ToString()!,
                            InstallationDate = r["Installation_Date"].ToString()!,
                            Status = r["Status"].ToString()!,
                            HouseholdID = r["Household_ID"].ToString()!
                        });
                }

                // Households without a meter yet
                const string hq = @"
                    SELECT h.Household_ID, h.Owner_Name FROM HouseHold h
                    WHERE h.IsActive = 1
                      AND h.Household_ID NOT IN (SELECT Household_ID FROM Meter)
                    ORDER BY h.Household_ID";
                using (var cmd = new SqlCommand(hq, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    AvailableHouseholds.Clear();
                    while (await r.ReadAsync())
                        AvailableHouseholds.Add(new HouseholdModel
                        {
                            HouseholdID = r["Household_ID"].ToString()!,
                            OwnerName = r["Owner_Name"].ToString()!
                        });
                }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private async Task AddMeterAsync()
        {
            Message = "";
            if (string.IsNullOrWhiteSpace(MeterID) || string.IsNullOrWhiteSpace(MeterNum)
             || string.IsNullOrWhiteSpace(Location) || string.IsNullOrWhiteSpace(HhID))
            { IsSuccess = false; Message = "⚠ Fill in all fields."; return; }

            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = @"
                    INSERT INTO Meter (Meter_ID, Meter_Number, Location, Installation_Date, Status, Household_ID)
                    VALUES (@MID, @MN, @LOC, @IDATE, @STAT, @HH)";
                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@MID", MeterID.Trim());
                cmd.Parameters.AddWithValue("@MN", MeterNum.Trim());
                cmd.Parameters.AddWithValue("@LOC", Location.Trim());
                cmd.Parameters.AddWithValue("@IDATE", DateTime.Today);
                cmd.Parameters.AddWithValue("@STAT", Status);
                cmd.Parameters.AddWithValue("@HH", HhID);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true; Message = $"✔ Meter {MeterID} added.";
                MeterID = MeterNum = Location = HhID = "";
                await LoadAllAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }
    }
}