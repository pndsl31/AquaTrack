using AquaTrack.Model;
using AquaTrack.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AquaTrack.ViewModel
{
    internal class NavBarViewModel : ObservableObject
    {
        private Window _currentWindow;
        Account CurrentUser { get; set; }
        HouseholdModel HouseHold { get; set; }
        public MeterModel Meter { get; set; }
        public ICommand GoToSubmitMeterCommand { get; set; }
        //public ICommand GoToSubjectsCommand { get; set; }
        //public ICommand GoToProfileCommand { get; set; }
        public ICommand GoToMainDashboardCommand { get; set; }
        public ICommand LogoutCommand { get; set; }



        public NavBarViewModel(Account currentUser, HouseholdModel household, Window currentWindow, MeterModel meter)
        {
            CurrentUser = currentUser;
            HouseHold = household;
            Meter = meter;
            _currentWindow = currentWindow;
            //GoToGradesCommand = new RelayCommand(GoToGrades);
            //GoToSubjectsCommand = new RelayCommand(GoToSubjects);
            //GoToProfileCommand = new RelayCommand(GoToProfile);
            GoToMainDashboardCommand = new RelayCommand(GoToMainDashboard);
            GoToSubmitMeterCommand = new RelayCommand(GoToSubmitMeter);
            LogoutCommand = new RelayCommand(Logout);
        }

        public void GoToSubmitMeter(object? parameter)
        {
            var win = new SubmitReading(CurrentUser, HouseHold);
            //win.DataContext = new GradesWindowVM(CurrentUser);
            win.Show();

            _currentWindow.Close();
        }

        //public void GoToSubjects(object? parameter)
        //{
        //    var win = new SubjectsWindow(CurrentUser);
        //    //win.DataContext = new SubjectsViewModel(CurrentUser);
        //    win.Show();

        //    _currentWindow.Close();
        //}

        //public void GoToProfile(object? parameter)
        //{
        //    var win = new ProfileWindow(CurrentUser);
        //    //win.DataContext = new ProfileViewModel(CurrentUser);
        //    win.Show();

        //    _currentWindow.Close();
        //}

        public void GoToMainDashboard(object? parameter)
        {
            var win = new DashboardWindow(CurrentUser, HouseHold, Meter);
            //win.DataContext = new Window1(CurrentUser);
            win.Show();

            _currentWindow.Close();
        }

        public void Logout(object? parameter)
        {
            var win = new MainWindow();
            win.Show();

            _currentWindow.Close();
        }
    }
}