using AquaTrack.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaTrack.Model
{
    public class BillModel : ObservableObject
    {
        public string _billID = string.Empty;
        public string _dueDate = string.Empty;
        public decimal _amountDue = 0;
        public string _status = string.Empty;
        public string _houseHoldID = string.Empty;
        public string _usageID = string.Empty;
        private decimal _consumption = 0;

        public string BillID
        {
            get=> _billID;
            set {  _billID = value; OnPropertyChanged(); }
        }
        public string UsageID
        {
            get => _usageID;
            set { _usageID = value; OnPropertyChanged(); }
        }
        public string DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }
        public decimal AmountDue
        {
            get => _amountDue;
            set { _amountDue = value; OnPropertyChanged(); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }
        public string HouseholdID
        {
            get => _houseHoldID;
            set { _houseHoldID = value; OnPropertyChanged(); }
        }
        public decimal Consumption
        {
            get => _consumption;
            set { _consumption = value; OnPropertyChanged(); }
        }
    }
}
