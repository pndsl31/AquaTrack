using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class HouseholdManagementViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=Amenoai;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }
        public ObservableCollection<HouseholdModel> Households { get; } = new();
        public ObservableCollection<string> UnlinkedAccounts { get; } = new();

        private HouseholdModel? _selected;
        public HouseholdModel? SelectedHousehold
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        private string _hhID = "", _accNum = "", _owner = "", _email = "", _address = "";
        private string _message = ""; bool _isSuccess;

        public string HouseholdID { get => _hhID; set { _hhID = value; OnPropertyChanged(); } }
        public string AccNum { get => _accNum; set { _accNum = value; OnPropertyChanged(); } }
        public string OwnerName { get => _owner; set { _owner = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string Address { get => _address; set { _address = value; OnPropertyChanged(); } }
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
        public bool IsSuccess { get => _isSuccess; set { _isSuccess = value; OnPropertyChanged(); } }

        public ICommand RegisterCommand { get; }
        public ICommand DisableCommand { get; }
        public ICommand EnableCommand { get; }

        public HouseholdManagementViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
            DisableCommand = new RelayCommand(async _ => await DisableAsync());
            EnableCommand = new RelayCommand(async _ => await EnableAsync());
            _ = LoadAllAsync();
        }

        private async Task LoadAllAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                // Load households
                const string hq = @"SELECT * FROM HouseHold ORDER BY Household_ID";
                using (var cmd = new SqlCommand(hq, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    Households.Clear();
                    while (await r.ReadAsync())
                        Households.Add(new HouseholdModel
                        {
                            HouseholdID = r["Household_ID"].ToString()!,
                            AccountNumber = r["Account_Number"].ToString()!,
                            OwnerName = r["Owner_Name"].ToString()!,
                            Email = r["Email"].ToString()!,
                            Address = r["Address"].ToString()!,
                            RegistrationDate = r["Registration_Date"].ToString()!,
                            IsActive = bool.Parse(r["IsActive"].ToString()!),
                        });
                }

                // Load accounts not yet linked to any household
                const string aq = @"
                    SELECT Account_Number FROM Accounts
                    WHERE Account_Number NOT IN (SELECT Account_Number FROM HouseHold)
                    AND IsActive = 1
                    ORDER BY Account_Number";
                using (var cmd = new SqlCommand(aq, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    UnlinkedAccounts.Clear();
                    while (await r.ReadAsync())
                        UnlinkedAccounts.Add(r["Account_Number"].ToString()!);
                }
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private async Task<bool> CanAssignHouseholdAsync(string householdId)
        {
            using var conn = new SqlConnection(ConnStr);
            await conn.OpenAsync();

            const string sql = @"
            SELECT Owner_Name
            FROM HouseHold
            WHERE Household_ID = @HouseholdID";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@HouseholdID", householdId);

            var result = await cmd.ExecuteScalarAsync();

            if (result == null || result == DBNull.Value)
            {
                return true;
            }

            string owner = result.ToString()!;

            // ❌ Household exists but already has an owner
            if (!string.IsNullOrWhiteSpace(owner))
            {
                IsSuccess = false;
                Message = "⚠ This household already has an owner assigned.";
                return false;
            }

            // ✅ Household exists and is unassigned
            return true;
        }

        private async Task RegisterAsync()
        {
            Message = "";
            if (string.IsNullOrWhiteSpace(HouseholdID) || string.IsNullOrWhiteSpace(AccNum)
             || string.IsNullOrWhiteSpace(OwnerName) || string.IsNullOrWhiteSpace(Email)
             || string.IsNullOrWhiteSpace(Address))
            { IsSuccess = false; Message = "⚠ Fill in all fields."; return; }

            try
            {
                if (!await CanAssignHouseholdAsync(HouseholdID))
                    return;
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_AddHousehold", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Household_ID", HouseholdID.Trim());
                cmd.Parameters.AddWithValue("@Account_Number", AccNum);
                cmd.Parameters.AddWithValue("@Owner_Name", OwnerName.Trim());
                cmd.Parameters.AddWithValue("@Email", Email.Trim());
                cmd.Parameters.AddWithValue("@Address", Address.Trim());
                cmd.Parameters.AddWithValue("@Registration_Date", DateTime.Today);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true; Message = $"✔ Household {HouseholdID} registered.";
                HouseholdID = AccNum = OwnerName = Email = Address = "";
                await LoadAllAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }

        private async Task DisableAsync()
        {
            if (SelectedHousehold == null) { Message = "⚠ Select a household first."; IsSuccess = false; return; }
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_DisableAccount", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Account_Number", SelectedHousehold.AccountNumber);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true; Message = $"✔ Household {SelectedHousehold.HouseholdID} disabled.";
                await LoadAllAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }
        private async Task EnableAsync()
        {
            if (SelectedHousehold == null) { Message = "⚠ Select a household first."; IsSuccess = false; return; }
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_EnableAccount", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Account_Number", SelectedHousehold.AccountNumber);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true; Message = $"✔ Household {SelectedHousehold.HouseholdID} enabled.";
                await LoadAllAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }
    }
}