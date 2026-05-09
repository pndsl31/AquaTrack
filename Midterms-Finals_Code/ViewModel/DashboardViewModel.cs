using AquaTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AquaTrack.ViewModel
{
    internal class DashboardViewModel : ObservableObject
    {
        public Account CurrentUser { get; set; }
        public HouseholdModel HouseHold { get; set; }
        public MeterModel Meter { get; set; }

        public NavBarViewModel NavBar { get; set; }

        public DashboardViewModel(Account currentUser, HouseholdModel household, Window currentWindow, MeterModel meter)
        {
            CurrentUser = currentUser;
            HouseHold = household;
            NavBar = new NavBarViewModel(currentUser, household, currentWindow, meter);
            Meter = meter;
        }
    }
}
