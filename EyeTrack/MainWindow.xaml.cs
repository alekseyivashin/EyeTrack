using System;
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
            this.DataContext = this;
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
                    MessageBox.Show("Устройства не были найдены!", "Confirmation", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                SearchDeviceButton.IsEnabled = true;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var calibrationWindow = new CalibrationWindow();
            calibrationWindow.Show();
        }

        private void ShowTextWindow(int textIndex)
        {
            var personName = NameTextBox.Text;
            App.Name = personName;
            if (string.IsNullOrWhiteSpace(personName))
            {
                MessageBox.Show("Пожалуйста, введите имя!", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var textWindow = new TextWindow(textIndex);
                textWindow.Show();
            }
        }

        private void Text1Button_Click(object sender, RoutedEventArgs e)
        {
            ShowTextWindow(1);
        }

        private void Text2Button_Click(object sender, RoutedEventArgs e)
        {
            ShowTextWindow(2);
        }

        private void Text3Button_Click(object sender, RoutedEventArgs e)
        {
            ShowTextWindow(3);
        }

        private void Text4Button_Click(object sender, RoutedEventArgs e)
        {
            ShowTextWindow(4);
        }

        private void Text5Button_Click(object sender, RoutedEventArgs e)
        {
            ShowTextWindow(5);
        }

        private void Text6Button_Click(object sender, RoutedEventArgs e)
        {
            ShowTextWindow(6);
        }
    }
}