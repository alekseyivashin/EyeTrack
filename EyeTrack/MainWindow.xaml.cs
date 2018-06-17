using EyeTrack.tracker;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tobii.Research;

namespace EyeTrack
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var data = new List<string>(GetTrackersNames());
            //Console.WriteLine(String.Join(", ", GetTrackersNames()));
            DevicesComboBox.ItemsSource = data;
        }

        private static List<string> GetTrackersNames() =>
            TrackerFinder.GetAllTrackers().Select(tracker => tracker.DeviceName).ToList();

        private void calibrateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTrackerName = DevicesComboBox.SelectedValue.ToString();
            var selectedTracker = TrackerFinder.GetByName(selectedTrackerName);
            var calibrationWindow = new CalibrationWindow(selectedTracker);
            calibrationWindow.Show();
        }
    }
}