using AquaTrack.Model;
using AquaTrack.ViewModel;
using System.Windows;

namespace AquaTrack.View
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow(Account user)
        {
            InitializeComponent();
            DataContext = new AdminDashboardViewModel(user, this);
        }
    }
}