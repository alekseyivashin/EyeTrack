using EyeTrack.tracker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Tobii.Research;

namespace EyeTrack
{
    public partial class TextWindow : Window
    {
        private readonly IEyeTracker _tracker;
        private readonly string _outputFileName;
        private List<GazeDataEventArgs> _gazeList = new List<GazeDataEventArgs>();

        private ReadingStatus _status = ReadingStatus.NotStarted;

        public TextWindow(int textIndex)
        {
            InitializeComponent();
            _tracker = App.Tracker;

            TextBox.Visibility = Visibility.Hidden;
            TextBox.Text = TextUtils.GetText(textIndex - 1);

            Directory.CreateDirectory("output");
            _outputFileName = $"output/{App.Name}_{textIndex}_{DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond}.json";
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

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Space) return;
            switch (_status)
            {
                case ReadingStatus.NotStarted:
                    MessageLabel.Visibility = Visibility.Hidden;
                    TextBox.Visibility = Visibility.Visible;
                    _status = ReadingStatus.Reading;
                    //_tracker.GazeDataReceived += EyeTracker_GazeDataReceived;
                    break;
                case ReadingStatus.Reading:
                    //_tracker.GazeDataReceived -= EyeTracker_GazeDataReceived;
                    MessageLabel.Content = "Чтение закончено. Нажмите \"Пробел\", чтобы выйти";
                    MessageLabel.Visibility = Visibility.Visible;
                    TextBox.Visibility = Visibility.Hidden;
                    _status = ReadingStatus.ReadingCompleted;
                    break;
                case ReadingStatus.ReadingCompleted:
                    //SaveListToFile();
                    ClearList();
                    Close();
                    break;
            }

            e.Handled = true;
        }
    }

    internal enum ReadingStatus
    {
        NotStarted,
        Reading,
        ReadingCompleted,
    }
}