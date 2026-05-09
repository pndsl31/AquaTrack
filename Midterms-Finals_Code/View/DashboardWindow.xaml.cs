using AquaTrack.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AquaTrack.View
{
    public partial class DashboardWindow : Window
    {
        public Account CurrentUser { get; set; }
        public HouseholdModel HouseHold { get; set; }
        public DashboardWindow(Account currentUser, HouseholdModel household)
        {
            CurrentUser = currentUser;
            HouseHold = household;
            InitializeComponent();
            DataContext = new ViewModel.DashboardViewModel(currentUser, household, this);
        }
    }
}
