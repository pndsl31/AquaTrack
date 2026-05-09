using AquaTrack.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AquaTrack.Model
{
    public class HouseholdModel : ObservableObject
    {
        private string _HouseholdID = "";
        private string _AccountNumber = "";
        private string _OwnerName = "";
        private string _Email = "";
        private string _Address = "";
        private string _RegistrationDate = "";

        public string HouseholdID
        {
            get => _HouseholdID;
            set { _HouseholdID = value; OnPropertyChanged(); }
        }

        public string AccountNumber
        {
            get => _AccountNumber;
            set { _AccountNumber = value; OnPropertyChanged(); }
        }

        public string OwnerName
        {
            get => _OwnerName;
            set { _OwnerName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _Email;
            set { _Email = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _Address;
            set { _Address = value; OnPropertyChanged(); }
        }

        public string RegistrationDate
        {
            get => _RegistrationDate;
            set { _RegistrationDate = value; OnPropertyChanged(); }
        }
    }
}

