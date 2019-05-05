using EyeTrack.tracker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Tobii.Research;

namespace EyeTrack
{
    public partial class TextWindow : Window
    {
        private readonly IEyeTracker _tracker;
        private readonly string _outputFileName;
        private List<GazeDataEventArgs> _gazeList = new List<GazeDataEventArgs>();

        public TextWindow(int textIndex)
        {
            InitializeComponent();
            _tracker = App.Tracker;

            textIndex--;
            TextBox.Text = TextUtils.GetText(textIndex);

            Directory.CreateDirectory("output");
            _outputFileName = $"output/{App.Name}_{textIndex}_{DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond}.json";
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
            Application.Current.Dispatcher.Invoke(new Action(() => { _gazeList.Add(e); }));
        }

        private void ClearList()
        {
            _gazeList = new List<GazeDataEventArgs>();
        }

        private void SaveListToFile()
        {
            var json = JsonConvert.SerializeObject(_gazeList, Formatting.Indented);
            File.WriteAllText(_outputFileName, json);
        }
    }
}