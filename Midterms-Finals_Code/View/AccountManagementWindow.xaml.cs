using AquaTrack.Model;
using AquaTrack.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AquaTrack.View
{
    public partial class AccountManagementWindow : Window
    {
        private AccountManagementViewModel _vm = null!;

        public AccountManagementWindow(Account user)
        {
            InitializeComponent();
            _vm = new AccountManagementViewModel(user, this);
            DataContext = _vm;
        }

        // PasswordBox can't bind directly — grab value on button click
        private void PwBox_Changed(object s, RoutedEventArgs e)
            => _vm.NewPassword = ((PasswordBox)s).Password;
        private void ConfirmBox_Changed(object s, RoutedEventArgs e)
            => _vm.NewConfirm = ((PasswordBox)s).Password;
    }
}