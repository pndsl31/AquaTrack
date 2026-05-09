using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AquaTrack.Model;
using AquaTrack.View;
using Microsoft.Data.SqlClient;

namespace AquaTrack.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        public Account CurrentUser { get; set; }
        public HouseholdModel HouseHold { get; set; }
        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            CurrentUser = new Account();
            HouseHold = new HouseholdModel();
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

            string connectionString = @"Server=DESKTOP-6085EPQ;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT A.Account_Number, A.Password, H.Household_ID, H.Owner_Name, H.Email, H.Address, H.Registration_Date
                        FROM Accounts A
                        JOIN HouseHold H
                        ON A.Account_Number = H.Account_Number
                        WHERE A.Account_Number = @AccountNumber AND A.Password = @password;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@accountNumber", CurrentUser.AccountNumber);
                        command.Parameters.AddWithValue("@password", CurrentUser.Password);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                CurrentUser.AccountNumber = reader["Account_Number"]?.ToString() ?? "";
                                CurrentUser.Password = reader["Password"]?.ToString() ?? "";
                                HouseHold.HouseholdID = reader["Household_ID"]?.ToString() ?? "";
                                HouseHold.OwnerName = reader["Owner_Name"]?.ToString() ?? "";
                                HouseHold.Email = reader["Email"]?.ToString() ?? "";
                                HouseHold.Address = reader["Address"]?.ToString() ?? "";
                                HouseHold.RegistrationDate = reader["Registration_Date"]?.ToString() ?? "";

                                MessageBox.Show($"Welcome back, {HouseHold.OwnerName}!", "Login Successful",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                var dashboard = new DashboardWindow(CurrentUser, HouseHold);
                                dashboard.Show();
                                Application.Current.MainWindow.Close();
                            }
                            else
                            {
                                MessageBox.Show("Invalid Account Number or Password.", "Login Failed",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
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
