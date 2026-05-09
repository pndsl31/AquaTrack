using AquaTrack.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaTrack.Model
{
    public class AlertModel : ObservableObject
    {
        public string _alrtID = string.Empty;
        public string _alertType = string.Empty;
        public string _alertDate = string.Empty;
        public string _message = string.Empty;
        public string _status = string.Empty;
        public string _houseHoldID = string.Empty;

        public string AlrtID
        {
            get => _alrtID;
            set { _alrtID = value; OnPropertyChanged(); }
        }
        public string AlertType
        {
            get => _alertType;
            set { _alertType = value; OnPropertyChanged(); }
        }
        public string AlertDate
        {
            get => _alertDate;
            set { _alertDate = value; OnPropertyChanged(); }
        }
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
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
