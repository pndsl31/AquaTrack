using AquaTrack.ViewModel;

namespace AquaTrack.Model
{
    public class ResidentModel : ObservableObject
    {
        private string _accountNumber = "";
        private string _password = "";
        private string _fullName = "";
        private string _email = "";
        private string _address = "";
        private string _contactNumber = "";
        private string _meterID = "";

        public string AccountNumber
        {
            get => _accountNumber;
            set { _accountNumber = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public string ContactNumber
        {
            get => _contactNumber;
            set { _contactNumber = value; OnPropertyChanged(); }
        }

        public string MeterID
        {
            get => _meterID;
            set { _meterID = value; OnPropertyChanged(); }
        }
    }
}
