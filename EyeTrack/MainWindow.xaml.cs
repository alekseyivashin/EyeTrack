using EyeTrack.tracker;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void SearchDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.Tracker == null)
            {
                SearchDeviceButton.IsEnabled = false;
                var allTrackers = TrackerFinder.GetAllTrackers();
                if (allTrackers.Any())
                {
                    var tracker = allTrackers.First();
                    DeviceLabel.Content = DeviceLabel.Content + tracker.DeviceName;
                    App.Tracker = tracker;
                }
                else
                {
                    MessageBox.Show("Устройства не были найдены!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                SearchDeviceButton.IsEnabled = true;
            }

        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var personName = NameTextBox.Text;
            if (string.IsNullOrWhiteSpace(personName))
            {
                MessageBox.Show("Пожалуйста, введите имя!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var calibrationWindow = new CalibrationWindow(personName);
                calibrationWindow.Show();
            }

        }

        private void IdentifyButton_Click(object sender, RoutedEventArgs e)
        {
            var textWindow = new TextWindow("text1.txt");
            textWindow.Show();
        }

    }
}