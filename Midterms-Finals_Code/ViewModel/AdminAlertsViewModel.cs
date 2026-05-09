using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    public class AdminAlertsViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-6085EPQ;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }
        public ObservableCollection<AlertModel> FilteredAlerts { get; } = new();

        private ObservableCollection<AlertModel> _all = new();
        private AlertModel? _selected;
        private string _filter = "All", _message = ""; bool _isSuccess;

        public AlertModel? SelectedAlert { get => _selected; set { _selected = value; OnPropertyChanged(); } }
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
        public bool IsSuccess { get => _isSuccess; set { _isSuccess = value; OnPropertyChanged(); } }
        public string SelectedFilter
        {
            get => _filter;
            set { _filter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ICommand FilterAllCommand { get; }
        public ICommand FilterPendingCommand { get; }
        public ICommand FilterAcknowledgedCommand { get; }
        public ICommand FilterResolvedCommand { get; }
        public ICommand AcknowledgeCommand { get; }
        public ICommand ResolveCommand { get; }

        public AdminAlertsViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            FilterAllCommand = new RelayCommand(_ => SelectedFilter = "All");
            FilterPendingCommand = new RelayCommand(_ => SelectedFilter = "Pending");
            FilterAcknowledgedCommand = new RelayCommand(_ => SelectedFilter = "Acknowledged");
            FilterResolvedCommand = new RelayCommand(_ => SelectedFilter = "Resolved");
            AcknowledgeCommand = new RelayCommand(async _ => await UpdateAlertAsync("Acknowledged"));
            ResolveCommand = new RelayCommand(async _ => await UpdateAlertAsync("Resolved"));
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = @"
                    SELECT a.Alrt_ID, a.Houshold_ID, a.Alert_Type, a.Alert_Date, a.Message, a.Status,
                           h.Owner_Name
                    FROM Alert a
                    INNER JOIN HouseHold h ON a.Houshold_ID = h.Household_ID
                    ORDER BY a.Alert_Date DESC";
                using var cmd = new SqlCommand(q, conn);
                using var r = await cmd.ExecuteReaderAsync();
                _all.Clear();
                while (await r.ReadAsync())
                    _all.Add(new AlertModel
                    {
                        AlrtID = r["Alrt_ID"].ToString()!,
                        HouseholdID = r["Houshold_ID"].ToString()!,
                        AlertType = r["Alert_Type"].ToString()!,
                        AlertDate = Convert.ToDateTime(r["Alert_Date"]).ToString("MMM dd, yyyy"),
                        Message = r["Message"].ToString()!,
                        Status = r["Status"].ToString()!,
                        OwnerName = r["Owner_Name"].ToString()!
                    });
                ApplyFilter();
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void ApplyFilter()
        {
            FilteredAlerts.Clear();
            var list = SelectedFilter == "All" ? _all : new ObservableCollection<AlertModel>(_all.Where(a => a.Status == SelectedFilter));
            foreach (var a in list) FilteredAlerts.Add(a);
        }

        private async Task UpdateAlertAsync(string newStatus)
        {
            if (SelectedAlert == null) { Message = "⚠ Select an alert first."; IsSuccess = false; return; }
            if (SelectedAlert.Status == "Resolved") { Message = "⚠ Already resolved."; IsSuccess = false; return; }
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ResolveAlert", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Alert_ID", SelectedAlert.AlrtID);
                cmd.Parameters.AddWithValue("@New_Status", newStatus);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true; Message = $"✔ {SelectedAlert.AlrtID} → {newStatus}.";
                await LoadAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }
    }
}