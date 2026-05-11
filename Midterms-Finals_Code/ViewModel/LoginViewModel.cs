using AquaTrack.Model;
using AquaTrack.View;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        public Account CurrentUser { get; set; }
        public HouseholdModel HouseHold { get; set; }
        public MeterModel Meter { get; set; }
        public UsageModel Usage { get; set; }
        public BillModel Bill { get; set; }
        public AlertModel Alert { get; set; }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            CurrentUser = new Account();
            HouseHold = new HouseholdModel();
            Meter = new MeterModel();
            Usage = new UsageModel();
            Bill = new BillModel();
            Alert = new AlertModel();
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private async void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox != null)
            {
                CurrentUser.Password = passwordBox.Password;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.AccountNumber))
            {
                MessageBox.Show("Please enter your Account Number.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show("Please enter your Password.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = @"Server=Amenoai;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

            try
            {
                bool logged = false;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    if (CurrentUser.AccountNumber.ToUpper().Contains("ADM"))
                    {
                        string query2 = @"SELECT A.Account_Number, A.Password, A.IsActive FROM Accounts A WHERE A.Account_Number = @AccountNumber AND A.Password = @password;";

                        using (SqlCommand command = new SqlCommand(query2, connection))
                        {
                            command.Parameters.AddWithValue("@AccountNumber", CurrentUser.AccountNumber);
                            command.Parameters.AddWithValue("@password", CurrentUser.Password);
                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    CurrentUser.AccountNumber = reader["Account_Number"]?.ToString() ?? "";
                                    CurrentUser.Password = reader["Password"]?.ToString() ?? "";
                                    CurrentUser.IsActive = bool.Parse(reader["IsActive"]?.ToString() ?? "");
                                    if (CurrentUser.IsActive)
                                    {
                                        MessageBox.Show($"Welcome back, {CurrentUser.AccountNumber.ToUpper()}!", "Login Successful",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                        
                                        logged = true;
                                        var adminDashboard = new AdminDashboardWindow(CurrentUser);

                                        // Set as main window
                                        Application.Current.MainWindow = adminDashboard;

                                        // Show dashboard
                                        adminDashboard.Show();

                                        // Close login window
                                        foreach (Window window in Application.Current.Windows)
                                        {
                                            if (window is MainWindow)
                                            {
                                                window.Close();
                                                break;
                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                        MessageBox.Show(
                                        "Account Disabled.",
                                        "Login Failed",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    }
                                }
                            }
                        }
                    }
                    if (!logged)
                    {
                        string query = @"SELECT A.Account_Number, A.Password, A.IsActive, H.Household_ID, H.Owner_Name, H.Email, H.Address, H.Registration_Date, 
                        M.Meter_ID, M.Meter_Number, M.Location, M.Installation_Date, M.Status, M.Household_ID, U.Usage_ID, U.Read_Date, U.Consumption, 
                        U.Usage_Status, U.Meter_ID, B.Bill_ID, B.Due_Date, B.Amount_Due, B.Status, B.Household_ID, B.Usage_ID, AL.Alrt_ID, 
                        AL.Alert_Type, AL.Alert_Date, AL.Message, AL.Status, AL.Household_ID

                        FROM Accounts A
                        LEFT JOIN HouseHold H ON A.Account_Number = H.Account_Number
                        LEFT JOIN Meter M ON H.Household_ID = M.Household_ID
                        LEFT JOIN Usage U ON M.Meter_ID = U.Meter_ID
                        LEFT JOIN Bill B ON U.Usage_ID = B.Usage_ID
                        LEFT JOIN Alert AL ON B.Household_ID = AL.Household_ID
                        WHERE A.Account_Number = @AccountNumber AND A.Password = @password;";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@AccountNumber", CurrentUser.AccountNumber);
                            command.Parameters.AddWithValue("@password", CurrentUser.Password);
                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    CurrentUser.AccountNumber = reader["Account_Number"]?.ToString() ?? "";
                                    CurrentUser.Password = reader["Password"]?.ToString() ?? "";
                                    CurrentUser.IsActive = bool.Parse(reader["IsActive"]?.ToString() ?? "");
                                    HouseHold.HouseholdID = reader["Household_ID"]?.ToString() ?? "";
                                    HouseHold.OwnerName = reader["Owner_Name"]?.ToString() ?? "";
                                    HouseHold.Email = reader["Email"]?.ToString() ?? "";
                                    HouseHold.Address = reader["Address"]?.ToString() ?? "";
                                    HouseHold.RegistrationDate = reader["Registration_Date"]?.ToString() ?? "";
                                    Meter.MeterID = reader["Meter_ID"]?.ToString() ?? "";
                                    Meter.MeterNumber = reader["Meter_Number"]?.ToString() ?? "";
                                    Meter.Location = reader["Location"]?.ToString() ?? "";
                                    Meter.InstallationDate = reader["Installation_Date"]?.ToString() ?? "";
                                    Meter.Status = reader["Status"]?.ToString() ?? "";
                                    Meter.HouseholdID = reader["Household_ID"]?.ToString() ?? "";
                                    Usage.UsageID = reader["Usage_ID"]?.ToString() ?? "";
                                    Usage.ReadDate = reader["Read_Date"]?.ToString() ?? "";

                                    if (CurrentUser.IsActive)
                                    {
                                        MessageBox.Show($"Welcome back, {HouseHold.OwnerName}!", "Login Successful",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                                        var dashboard = new DashboardWindow(CurrentUser, HouseHold, Meter);

                                        // Set dashboard as the new main window
                                        Application.Current.MainWindow = dashboard;

                                        // Show dashboard
                                        dashboard.Show();

                                        // Close current login window safely
                                        foreach (Window window in Application.Current.Windows)
                                        {
                                            if (window is MainWindow)
                                            {
                                                window.Close();
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show(
                                        "Account Disabled.",
                                        "Login Failed",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(
                                        "Invalid Account Number or Password.",
                                        "Login Failed",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection failed: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
