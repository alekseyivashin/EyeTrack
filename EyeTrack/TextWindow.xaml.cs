using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Tobii.Research;

namespace EyeTrack
{
    /// <summary>
    /// Логика взаимодействия для TextWindow.xaml
    /// </summary>
    public partial class TextWindow : Window
    {
        
        private readonly IEyeTracker _tracker;
        private readonly string _outputFileName;
        private List<GazeDataEventArgs> _gaseList = new List<GazeDataEventArgs>();
        private int _gazeCount = 0;
    
        public TextWindow(string fileName)
        {
            InitializeComponent();
            
            _tracker = App.Tracker;
            
            var text = ReadFile(fileName);
            TextBox.Text = text;

            _outputFileName = App.Name + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond + ".txt";
        }

        private static string ReadFile(string fileName)
        {
            try
            {   // Open the text file using a stream reader.
                using (var sr = new StreamReader(fileName))
                {
                    // Read the stream to a string, and write the string to the console.
                    var text = sr.ReadToEnd();
                    return text;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@"The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            _tracker.GazeDataReceived += EyeTracker_GazeDataReceived;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            _tracker.GazeDataReceived -= EyeTracker_GazeDataReceived;
            SaveListToFile();
            ClearList();
            Close();
        }
        
        private void EyeTracker_GazeDataReceived(object sender, GazeDataEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _gaseList.Add(e);
                _gazeCount++;
            }));
        }

        private void ClearList()
        {
            _gaseList = new List<GazeDataEventArgs>();
        }

        private void SaveListToFile()
        {
            var json = JsonConvert.SerializeObject(_gaseList, Formatting.Indented);
            File.WriteAllText(_outputFileName, json);
        }
    }
}
