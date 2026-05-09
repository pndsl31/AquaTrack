using AquaTrack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// <summary>
    /// Interaction logic for SubmitReading.xaml
    /// </summary>
    /// 

    public partial class SubmitReading : Window
    {
        public Account CurrentUser { get; set; }
        public HouseholdModel HouseHold { get; set; }
        public SubmitReading(Account currentUser, HouseholdModel household)
        {
            CurrentUser = currentUser;
            HouseHold = household;
            InitializeComponent();
        }
    }
}
