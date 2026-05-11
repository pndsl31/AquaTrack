using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AquaTrack.ViewModel
{
    public class AlertsViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=Amenoai;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public HouseholdModel HouseHold { get; }
        public MeterModel Meter { get; }
        public NavBarViewModel NavBar { get; }

        public ObservableCollection<AlertModel> FilteredAlerts { get; } = new();
        private ObservableCollection<AlertModel> _all = new();

        private AlertModel? _selected;
        private string _filter = "All";
        private string _message = "";
        private bool _isSuccess;
        private int _pendingCount;
        private int _acknowledgedCount;
        private int _resolvedCount;

        public AlertModel? SelectedAlert
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(); }
        }
        public string SelectedFilter
        {
            get => _filter;
            set { _filter = value; OnPropertyChanged(); ApplyFilter(); }
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
        public int PendingCount
        {
            get => _pendingCount;
            set { _pendingCount = value; OnPropertyChanged(); }
        }
        public int AcknowledgedCount
        {
            get => _acknowledgedCount;
            set { _acknowledgedCount = value; OnPropertyChanged(); }
        }
        public int ResolvedCount
        {
            get => _resolvedCount;
            set { _resolvedCount = value; OnPropertyChanged(); }
        }

        public RelayCommand FilterAllCommand { get; }
        public RelayCommand FilterPendingCommand { get; }
        public RelayCommand FilterAcknowledgedCommand { get; }
        public RelayCommand FilterResolvedCommand { get; }
        public RelayCommand AcknowledgeCommand { get; }
        public RelayCommand ResolveCommand { get; }

        public AlertsViewModel(Account user, HouseholdModel household, MeterModel meter, Window win)
        {
            CurrentUser = user;
            HouseHold = household;
            Meter = meter;
            NavBar = new NavBarViewModel(user, household, win, meter);

            FilterAllCommand = new RelayCommand(_ => SelectedFilter = "All");
            FilterPendingCommand = new RelayCommand(_ => SelectedFilter = "Pending");
            FilterAcknowledgedCommand = new RelayCommand(_ => SelectedFilter = "Acknowledged");
            FilterResolvedCommand = new RelayCommand(_ => SelectedFilter = "Resolved");
            AcknowledgeCommand = new RelayCommand(async _ => await UpdateStatusAsync("Acknowledged"));
            ResolveCommand = new RelayCommand(async _ => await UpdateStatusAsync("Resolved"));

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();

                const string q = @"
                    SELECT Alrt_ID, Household_ID, Alert_Type, Alert_Date, Message, Status
                    FROM Alert
                    WHERE Household_ID = @HH
                    ORDER BY Alert_Date DESC";

                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
                using var r = await cmd.ExecuteReaderAsync();

                _all.Clear();
                while (await r.ReadAsync())
                    _all.Add(new AlertModel
                    {
                        AlrtID = r["Alrt_ID"].ToString()!,
                        HouseholdID = r["Household_ID"].ToString()!,
                        AlertType = r["Alert_Type"].ToString()!,
                        AlertDate = Convert.ToDateTime(r["Alert_Date"]).ToString("MMM dd, yyyy"),
                        Message = r["Message"].ToString()!,
                        Status = r["Status"].ToString()!
                    });

                PendingCount = _all.Count(a => a.Status == "Pending");
                AcknowledgedCount = _all.Count(a => a.Status == "Acknowledged");
                ResolvedCount = _all.Count(a => a.Status == "Resolved");
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Alert load error: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyFilter()
        {
            FilteredAlerts.Clear();
            var list = SelectedFilter == "All"
                ? _all
                : new ObservableCollection<AlertModel>(_all.Where(a => a.Status == SelectedFilter));
            foreach (var a in list)
                FilteredAlerts.Add(a);
        }

        private async Task UpdateStatusAsync(string newStatus)
        {
            if (SelectedAlert == null) { IsSuccess = false; Message = "⚠ Select an alert first."; return; }
            if (SelectedAlert.Status == "Resolved") { IsSuccess = false; Message = "⚠ Alert is already resolved."; return; }
            if (SelectedAlert.Status == newStatus) { IsSuccess = false; Message = $"⚠ Alert is already {newStatus}."; return; }

            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_ResolveAlert", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Alert_ID", SelectedAlert.AlrtID);
                cmd.Parameters.AddWithValue("@New_Status", newStatus);
                await cmd.ExecuteNonQueryAsync();

                IsSuccess = true;
                Message = $"✔ {SelectedAlert.AlrtID} → {newStatus}.";
                await LoadAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }
    }
}