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
    public class AdminBillingViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=DESKTOP-6085EPQ;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public AdminNavBarViewModel NavBar { get; }
        public ObservableCollection<BillModel> FilteredBills { get; } = new();

        private ObservableCollection<BillModel> _all = new();
        private BillModel? _selected;
        private string _filter = "All", _message = ""; bool _isSuccess;

        public BillModel? SelectedBill { get => _selected; set { _selected = value; OnPropertyChanged(); } }
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }
        public bool IsSuccess { get => _isSuccess; set { _isSuccess = value; OnPropertyChanged(); } }
        public string SelectedFilter
        {
            get => _filter;
            set { _filter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ICommand FilterAllCommand { get; }
        public ICommand FilterUnpaidCommand { get; }
        public ICommand FilterPaidCommand { get; }
        public ICommand PayBillCommand { get; }

        public AdminBillingViewModel(Account user, Window win)
        {
            CurrentUser = user;
            NavBar = new AdminNavBarViewModel(user, win);
            FilterAllCommand = new RelayCommand(_ => SelectedFilter = "All");
            FilterUnpaidCommand = new RelayCommand(_ => SelectedFilter = "Unpaid");
            FilterPaidCommand = new RelayCommand(_ => SelectedFilter = "Paid");
            PayBillCommand = new RelayCommand(async _ => await PayAsync());
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                const string q = @"
                    SELECT b.Bill_ID, b.Due_Date, b.Amount_Due, b.Status,
                           b.Usage_ID, b.Household_ID, h.Owner_Name,
                           u.Read_Date, u.Consumption
                    FROM Bill b
                    INNER JOIN HouseHold h ON b.Household_ID = h.Household_ID
                    INNER JOIN Usage     u ON b.Usage_ID     = u.Usage_ID
                    ORDER BY b.Due_Date DESC";
                using var cmd = new SqlCommand(q, conn);
                using var r = await cmd.ExecuteReaderAsync();
                _all.Clear();
                while (await r.ReadAsync())
                    _all.Add(new BillModel
                    {
                        BillID = r["Bill_ID"].ToString()!,
                        DueDate = Convert.ToDateTime(r["Due_Date"]).ToString("MMM dd, yyyy"),
                        AmountDue = Convert.ToDecimal(r["Amount_Due"]),
                        Status = r["Status"].ToString()!,
                        UsageID = r["Usage_ID"].ToString()!,
                        HouseholdID = r["Household_ID"].ToString()!,
                        OwnerName = r["Owner_Name"].ToString()!,
                        ReadDate = Convert.ToDateTime(r["Read_Date"]).ToString("MMM dd, yyyy"),
                        Consumption = Convert.ToDecimal(r["Consumption"])
                    });
                ApplyFilter();
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        private void ApplyFilter()
        {
            FilteredBills.Clear();
            var list = SelectedFilter == "All" ? _all : new ObservableCollection<BillModel>(_all.Where(b => b.Status == SelectedFilter));
            foreach (var b in list) FilteredBills.Add(b);
        }

        private async Task PayAsync()
        {
            if (SelectedBill == null) { Message = "⚠ Select a bill first."; IsSuccess = false; return; }
            if (SelectedBill.Status == "Paid") { Message = "⚠ Already paid."; IsSuccess = false; return; }
            try
            {
                using var conn = new SqlConnection(ConnStr);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("sp_PayBill", conn)
                { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Bill_ID", SelectedBill.BillID);
                await cmd.ExecuteNonQueryAsync();
                IsSuccess = true; Message = $"✔ {SelectedBill.BillID} marked as Paid.";
                await LoadAsync();
            }
            catch (Exception ex) { IsSuccess = false; Message = "⚠ " + ex.Message; }
        }
    }
}