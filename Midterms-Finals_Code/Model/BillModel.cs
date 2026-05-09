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
        public string _amountDue = string.Empty;
        public string _status = string.Empty;
        public string _houseHoldID = string.Empty;

        public string BillID
        {
            get=> _billID;
            set {  _billID = value; OnPropertyChanged(); }
        }
        public string DueDate
        {
            get => _dueDate;
            set { _dueDate = value; OnPropertyChanged(); }
        }
        public string AmountDue
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
    }
}
