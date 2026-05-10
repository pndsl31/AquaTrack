using AquaTrack.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaTrack.Model
{
    public class Account : ObservableObject
    {
        private string _accountNumber = "";
        private string _password = "";
        private bool _isActive = true;
        private string _linkedHousehold = "";
        private string _linkedOwner = "";

        public string AccountNumber
        {
            get => _accountNumber;
            set { _accountNumber = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }
        public string LinkedHousehold
        {
            get => _linkedHousehold;
            set { _linkedHousehold = value; OnPropertyChanged(); }
        }
        public string LinkedOwner
        {
            get => _linkedOwner;
            set { _linkedOwner = value; OnPropertyChanged(); }
        }
        public string StatusText
        {
            get
            {
                return IsActive ? "Active" : "Disabled";
            }
        }
    }
}
