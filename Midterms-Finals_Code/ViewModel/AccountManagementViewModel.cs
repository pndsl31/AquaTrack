using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class AccountManagementViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-6085EPQ;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public Account new_Account {get; set;}
        public AdminNavBarViewModel NavBar { get; }

        public ObservableCollection<Account> Accounts { get; } = new();

        private Account? _selected;

        public Account? SelectedAccount
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }

        private string _newAccountNumber = "";
        private string _newPassword = "";
        private string _newConfirm = "";
        private string _message = "";
        private bool _isSuccess;

        public string NewAccountNumber { get => _newAccountNumber; set { _newAccountNumber = value; OnPropertyChanged(); } }
        public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(); } }
        public string NewConfirm { get => _newConfirm; set { _newConfirm = value; OnPropertyChanged(); } }
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
        public bool IsSuccess { get => _isSuccess; set { _isSuccess = value; OnPropertyChanged(); } }

        public ICommand UpdatePasswordCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand DisableCommand { get; }
        public ICommand EnableCommand { get; }

        public AccountManagementViewModel(Account user, Window win)
        {
            new_Account = new Account();
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            AddAccountCommand = new RelayCommand(async _ => await AddAccountAsync());
            DisableCommand = new RelayCommand(async _ => await ToggleAccountAsync(false));
            EnableCommand = new RelayCommand(async _ => await ToggleAccountAsync(true));
            UpdatePasswordCommand = new RelayCommand(async _ => await UpdatePasswordAsync());
            _ = LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = @"
                    SELECT a.Account_Number, a.IsActive,
                           ISNULL(h.Household_ID,'None')       AS LinkedHousehold,
                           ISNULL(h.Owner_Name,'No Household') AS LinkedOwner
                    FROM Accounts a
                    LEFT JOIN HouseHold h ON a.Account_Number = h.Account_Number
                    ORDER BY a.Account_Number";
                using var cmd = new SqlCommand(q, conn);
                using var r = await cmd.ExecuteReaderAsync();
                Accounts.Clear();
                while (await r.ReadAsync())
                    Accounts.Add(new Account
                    {
                        AccountNumber = r["Account_Number"].ToString()!,
                        IsActive = bool.Parse(r["IsActive"].ToString()!),
                        LinkedHousehold = r["linkedhousehold"].ToString()!,
                        LinkedOwner = r["linkedowner"].ToString()!
                    });
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private async Task AddAccountAsync()
        {
            Message = "";

            if (string.IsNullOrWhiteSpace(NewAccountNumber) || string.IsNullOrWhiteSpace(NewPassword))
            {
                IsSuccess = false;
                Message = "⚠ Fill in all fields.";
                return;
            }

            if (NewPassword != NewConfirm)
            {
                IsSuccess = false;
                Message = "⚠ Passwords do not match.";
                return;
            }

            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                // 🔴 CHECK DUPLICATE FIRST
                using (var checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Accounts WHERE Account_Number = @Account", conn))
                {
                    checkCmd.Parameters.AddWithValue("@Account", NewAccountNumber.Trim());
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        IsSuccess = false;
                        Message = "⚠ Account number already exists.";
                        return;
                    }
                }

                // INSERT ONLY IF NOT EXISTS
                using var cmd = new SqlCommand("sp_AddAccount", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Account_Number", NewAccountNumber.Trim());
                cmd.Parameters.AddWithValue("@HashedPassword", NewPassword);

                await cmd.ExecuteNonQueryAsync();

                IsSuccess = true;
                Message = $"✔ Account {NewAccountNumber} created.";

                NewAccountNumber = NewPassword = NewConfirm = "";
                await LoadAccountsAsync();
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                Message = "⚠ " + ex.Message;
            }
        }

        private async Task UpdatePasswordAsync()
        {
            Message = "";

            if (SelectedAccount == null)
            {
                IsSuccess = false;
                Message = "⚠ Select an account.";
                return;
            }

            if (string.IsNullOrWhiteSpace(UpdatePassword) || string.IsNullOrWhiteSpace(UpdateConfirm))
            {
                IsSuccess = false;
                Message = "⚠ Fill password fields.";
                return;
            }

            if (UpdatePassword != UpdateConfirm)
            {
                IsSuccess = false;
                Message = "⚠ Passwords do not match.";
                return;
            }

            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                using var cmd = new SqlCommand(@"
            UPDATE Accounts
            SET Password = @Password
            WHERE Account_Number = @Account", conn);

                cmd.Parameters.AddWithValue("@Password", UpdatePassword);
                cmd.Parameters.AddWithValue("@Account", SelectedAccount.AccountNumber);

                await cmd.ExecuteNonQueryAsync();

                IsSuccess = true;
                Message = $"✔ Password updated for {SelectedAccount.AccountNumber}";

                UpdatePassword = "";
                UpdateConfirm = "";
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                Message = "⚠ " + ex.Message;
            }
        }
        private async Task ToggleAccountAsync(bool enable)
        {
            if (SelectedAccount == null) { Message = "⚠ Select an account first."; IsSuccess = false; return; }
            string sp = enable ? "sp_EnableAccount" : "sp_DisableAccount";
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand(sp, conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Account_Number", SelectedAccount.AccountNumber);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true;
                Message = $"✔ Account {SelectedAccount.AccountNumber} {(enable ? "enabled" : "disabled")}.";
                await LoadAccountsAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }

        private string _updatePassword = "";
        private string _updateConfirm = "";

        public string UpdatePassword
        {
            get => _updatePassword;
            set { _updatePassword = value; OnPropertyChanged(); }
        }

        public string UpdateConfirm
        {
            get => _updateConfirm;
            set { _updateConfirm = value; OnPropertyChanged(); }
        }
        

    }
}