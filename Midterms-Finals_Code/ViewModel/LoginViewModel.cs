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
        public ResidentModel CurrentResident { get; set; }
        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            CurrentResident = new ResidentModel();
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private async void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox != null)
            {
                CurrentResident.Password = passwordBox.Password;
            }

            if (string.IsNullOrWhiteSpace(CurrentResident.AccountNumber))
            {
                MessageBox.Show("Please enter your Account Number.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentResident.Password))
            {
                MessageBox.Show("Please enter your Password.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Database=aquatrack;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Application Name=""SQL Server Management Studio"";Command Timeout=0";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT AccountNumber, Password, FullName, Email,
                                            Address, ContactNumber, MeterID
                                     FROM Residents
                                     WHERE AccountNumber = @accountNumber
                                       AND Password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@accountNumber", CurrentResident.AccountNumber);
                        command.Parameters.AddWithValue("@password", CurrentResident.Password);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                CurrentResident.AccountNumber = reader["AccountNumber"]?.ToString() ?? "";
                                CurrentResident.FullName      = reader["FullName"]?.ToString() ?? "";
                                CurrentResident.Email         = reader["Email"]?.ToString() ?? "";
                                CurrentResident.Address       = reader["Address"]?.ToString() ?? "";
                                CurrentResident.ContactNumber = reader["ContactNumber"]?.ToString() ?? "";
                                CurrentResident.MeterID       = reader["MeterID"]?.ToString() ?? "";

                                MessageBox.Show($"Welcome back, {CurrentResident.FullName}!", "Login Successful",
                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                var dashboard = new DashboardWindow(CurrentResident);
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
