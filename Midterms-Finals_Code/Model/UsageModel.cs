using AquaTrack.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaTrack.Model
{
    public class UsageModel : ObservableObject
    {
        public string _usageID = String.Empty;
        public string _readDate = String.Empty;
        public string _consumption = String.Empty;
        public string _usageStatus = String.Empty;
        public string _meterID = String.Empty;

        public string UsageID
        {
            get => _usageID;
            set { _usageID = value; OnPropertyChanged(); }
        }

        public string ReadDate
        {
            get => _readDate;
            set { _readDate = value; OnPropertyChanged(); }
        }

        public string Consumption
        {
            get => _consumption;
            set { _consumption = value; OnPropertyChanged(); }
        }
        public string UsageStatus
        {
            get => _usageStatus;
            set { _usageStatus = value; OnPropertyChanged(); }
        }
        public string MeterID
        {
            get => _meterID;
            set { _meterID = value; OnPropertyChanged(); }
        }
    }
}
