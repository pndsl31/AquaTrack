using AquaTrack.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AquaTrack.ViewModel
{
    public class BillingViewModel : ObservableObject
    {
        private const string ConnStr =
            @"Server=Amenoai;Database=AquaTrack;Trusted_Connection=True;TrustServerCertificate=True;";

        public Account CurrentUser { get; }
        public HouseholdModel HouseHold { get; }
        public MeterModel Meter { get; }
        public NavBarViewModel NavBar { get; }

        public ObservableCollection<BillModel> FilteredBills { get; } = new();
        private ObservableCollection<BillModel> _all = new();

        private BillModel? _selected;
        private string _filter = "All";
        private string _message = "";
        private bool _isSuccess;

        // Summary stats
        private decimal _totalUnpaid;
        private int _unpaidCount;
        private int _paidCount;

        public BillModel? SelectedBill
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
        public decimal TotalUnpaid
        {
            get => _totalUnpaid;
            set { _totalUnpaid = value; OnPropertyChanged(); }
        }
        public int UnpaidCount
        {
            get => _unpaidCount;
            set { _unpaidCount = value; OnPropertyChanged(); }
        }
        public int PaidCount
        {
            get => _paidCount;
            set { _paidCount = value; OnPropertyChanged(); }
        }

        public RelayCommand FilterAllCommand { get; }
        public RelayCommand FilterUnpaidCommand { get; }
        public RelayCommand FilterPaidCommand { get; }

        public BillingViewModel(Account user, HouseholdModel household, MeterModel meter, Window win)
        {
            CurrentUser = user;
            HouseHold = household;
            Meter = meter;
            NavBar = new NavBarViewModel(user, household, win, meter);

            FilterAllCommand = new RelayCommand(_ => SelectedFilter = "All");
            FilterUnpaidCommand = new RelayCommand(_ => SelectedFilter = "Unpaid");
            FilterPaidCommand = new RelayCommand(_ => SelectedFilter = "Paid");

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
                           b.Usage_ID, b.Household_ID,
                           u.Read_Date, u.Consumption, u.Usage_Status
                    FROM Bill b
                    INNER JOIN Usage u ON b.Usage_ID = u.Usage_ID
                    WHERE b.Household_ID = @HH
                    ORDER BY b.Due_Date DESC";

                using var cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@HH", HouseHold.HouseholdID);
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
                        HouseholdID = r["Household_ID"].ToString()!
                    });

                UnpaidCount = _all.Count(b => b.Status == "Unpaid");
                PaidCount = _all.Count(b => b.Status == "Paid");
                TotalUnpaid = _all.Where(b => b.Status == "Unpaid").Sum(b => b.AmountDue);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Billing load error: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyFilter()
        {
            FilteredBills.Clear();
            var list = SelectedFilter == "All"
                ? _all
                : new ObservableCollection<BillModel>(_all.Where(b => b.Status == SelectedFilter));
            foreach (var b in list)
                FilteredBills.Add(b);
        }
    }
}