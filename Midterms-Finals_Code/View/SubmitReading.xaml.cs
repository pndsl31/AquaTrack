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
        public SubmitReading(Account currentUser, HouseholdModel household, MeterModel meter)
        {
            InitializeComponent();
            DataContext = new ViewModel.SubmitReadingViewModel(currentUser, household, meter, this);
        }
    }
}
