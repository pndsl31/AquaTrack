using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class AdminReportsViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-2SQJPO3\SQLEXPRESS;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }
        public ObservableCollection<HouseholdModel> Households { get; } = new();
        public ObservableCollection<UsageModel> UsageHistory { get; } = new();
        public ObservableCollection<AlertModel> AlertHistory { get; } = new();

        private HouseholdModel? _selectedHH;
        public HouseholdModel? SelectedHousehold
        {
            get => _selectedHH;
            set { _selectedHH = value; OnPropertyChanged(); _ = LoadReportAsync(); }
        }

        private string _totalUnpaid = "PHP 0.00", _alertCount = "0", _totalConsumption = "0 m³";
        public string TotalUnpaid { get => _totalUnpaid; set { _totalUnpaid = value; OnPropertyChanged(); } }
        public string AlertCount { get => _alertCount; set { _alertCount = value; OnPropertyChanged(); } }
        public string TotalConsumption { get => _totalConsumption; set { _totalConsumption = value; OnPropertyChanged(); } }

        public AdminReportsViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            _ = LoadHouseholdsAsync();
        }

        private async Task LoadHouseholdsAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = "SELECT Household_ID, Owner_Name FROM HouseHold ORDER BY Household_ID";
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

        private async Task LoadReportAsync()
        {
            if (SelectedHousehold == null) return;
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                // sp_GetHouseholdReport returns two result sets
                using var cmd = new SqlCommand("sp_GetHouseholdReport", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Household_ID", SelectedHousehold.HouseholdID);
                using var r = await cmd.ExecuteReaderAsync();

                UsageHistory.Clear();
                decimal totalConsumption = 0;
                while (await r.ReadAsync())
                {
                    decimal c = Convert.ToDecimal(r["Consumption"]);
                    totalConsumption += c;
                    UsageHistory.Add(new UsageModel
                    {
                        UsageID = r["Usage_ID"].ToString()!,
                        ReadDate = Convert.ToDateTime(r["Read_Date"]).ToString("MMM dd, yyyy"),
                        Consumption = c,
                        UsageStatus = r["Usage_Status"].ToString()!,
                        MeterID = r["Meter_ID"].ToString()!
                    });
                }
                TotalConsumption = $"{totalConsumption} m³";

                await r.NextResultAsync();
                AlertHistory.Clear();
                int pending = 0;
                while (await r.ReadAsync())
                {
                    var status = r["Status"].ToString()!;
                    if (status == "Pending") pending++;
                    AlertHistory.Add(new AlertModel
                    {
                        AlrtID = r["Alrt_ID"].ToString()!,
                        AlertType = r["Alert_Type"].ToString()!,
                        AlertDate = Convert.ToDateTime(r["Alert_Date"]).ToString("MMM dd, yyyy"),
                        Message = r["Message"].ToString()!,
                        Status = status
                    });
                }
                AlertCount = pending.ToString();

                // Total unpaid
                using var cmd2 = new SqlCommand("SELECT dbo.fn_GetTotalUnpaidAmount(@HH)", conn);
                cmd2.Parameters.AddWithValue("@HH", SelectedHousehold.HouseholdID);
                var total = await cmd2.ExecuteScalarAsync();
                TotalUnpaid = "PHP " + Convert.ToDecimal(total).ToString("F2");
            }
            catch (Exception ex) { MessageBox.Show("Report error: " + ex.Message); }
        }
    }
}