using AquaTrack.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaTrack.Model
{
    public class MeterModel : ObservableObject
    {
        private string _meterID = string.Empty;
        private string _meterNumber = string.Empty;
        private string _location = string.Empty;
        private string _installationDate = string.Empty;
        private string _status = string.Empty;
        private string _HouseholdID = string.Empty;

        public string MeterID
        {
            get => _meterID;
            set { _meterID = value; OnPropertyChanged(); }
        }

        public string MeterNumber
        {
            get => _meterNumber;
            set { _meterNumber = value; OnPropertyChanged(); }
        }

        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(); }
        }

        public string InstallationDate
        {
            get => _installationDate;
            set { _installationDate = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string HouseholdID
        {
            get => _HouseholdID;
            set { _HouseholdID = value; OnPropertyChanged(); }
        }

    }
}
