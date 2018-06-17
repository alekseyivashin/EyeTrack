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
using Tobii.Research;

namespace EyeTrack
{
    /// <summary>
    /// Логика взаимодействия для CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private IEyeTracker _tracker;
        
        private const int CircleDiameter = 10;
        private readonly Brush _redBrush = new SolidColorBrush(Colors.Red);
        private readonly Brush _greenBrush = new SolidColorBrush(Colors.Green);
        private readonly Brush _blueBrush = new SolidColorBrush(Colors.Blue);
        private readonly Brush _grayBrush = new SolidColorBrush(Colors.Gray);
        private Point? _lastLeftPoint = null;
        private Point? _lastRightPoint = null;

        public CalibrationWindow(IEyeTracker tracker)
        {
            _tracker = tracker;
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DrawCircle(Point point)
        {
            DrawCircle(point, _redBrush);
        }

        private void DrawCircle(Point point, Brush brush)
        {
            const int width = CircleDiameter;
            const int height = CircleDiameter;
            var ellipse = new Ellipse
            {
                Width = width,
                Height = height,
                Fill = brush
            };
            var left = point.X - (width / 2.0);
            var top = point.Y - (height / 2.0);
            ellipse.Margin = new Thickness(left, top, 0, 0);
            PaintSurface.Children.Add(ellipse);
        }

        private void ClearSurface()
        {
            PaintSurface.Children.Clear();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ActionsPanel.Visibility = Visibility.Hidden;
            LegendPanel.Visibility = Visibility.Hidden;
            Calibrate();
        }

        private async void Calibrate()
        {
            ClearSurface();
            // Create a calibration object.
            var calibration = new ScreenBasedCalibration(_tracker);
            // Enter calibration mode.
            await calibration.EnterCalibrationModeAsync();
            // Define the points on screen we should calibrate at.
            // The coordinates are normalized, i.e. (0.0f, 0.0f) is the upper left corner and (1.0f, 1.0f) is the lower right corner.
            var pointsToCalibrate = new NormalizedPoint2D[] {
                new NormalizedPoint2D(0.5f, 0.5f),
                new NormalizedPoint2D(0.2f, 0.5f),
                new NormalizedPoint2D(0.8f, 0.5f),
                new NormalizedPoint2D(0.5f, 0.2f),
                new NormalizedPoint2D(0.5f, 0.8f),
            };
            // Collect data.
            foreach (var point in pointsToCalibrate)
            {
                // Show an image on screen where you want to calibrate.

                DrawCircle(ToPoint(point));
                // Wait a little for user to focus.
                System.Threading.Thread.Sleep(1000);
                // Collect data.
                var status = await calibration.CollectDataAsync(point);
                if (status != CalibrationStatus.Success)
                {
                    // Try again if it didn't go well the first time.
                    // Not all eye tracker models will fail at this point, but instead fail on ComputeAndApply.
                    await calibration.CollectDataAsync(point);
                }
                ClearSurface();
            }
            // Compute and apply the calibration.
            var calibrationResult = await calibration.ComputeAndApplyAsync();
            var drawErrorCount = 0;

            foreach (var point in calibrationResult.CalibrationPoints)
            {
                var pointPosition = ToPoint(point.PositionOnDisplayArea);
                DrawCircle(pointPosition, _grayBrush);
                foreach (var sample in point.CalibrationSamples)
                {
                    var leftPosition = sample.LeftEye.PositionOnDisplayArea;
                    var rightPosition = sample.RightEye.PositionOnDisplayArea;
                    var leftValidity = sample.LeftEye.Validity == CalibrationEyeValidity.ValidAndUsed;
                    var rightValidity = sample.RightEye.Validity == CalibrationEyeValidity.ValidAndUsed;
                    var leftPoint = ToPoint(leftPosition);
                    var rightPoint = ToPoint(rightPosition);

                    if (double.IsNaN(leftPoint.X) || double.IsNaN(leftPoint.Y))
                    {
                        drawErrorCount++;
                    } else
                    {
                        DrawLine(pointPosition, leftPoint, leftValidity ? _blueBrush : _redBrush);
                    }

                    if (double.IsNaN(rightPoint.X) || double.IsNaN(rightPoint.Y))
                    {
                        drawErrorCount++;
                    }
                    else
                    {
                        DrawLine(pointPosition, rightPoint, rightValidity ? _greenBrush : _redBrush);
                    }
                }
            }
            LegendPanel.Visibility = Visibility.Visible;
            ActionsPanel.Visibility = Visibility.Visible;

            if (drawErrorCount > 0)
            {
                MessageBox.Show($"{drawErrorCount} points were not been drawn");
            }
            //WriteLog("Compute and apply returned {0} and collected at {1} points.",
            //    calibrationResult.Status, calibrationResult.CalibrationPoints.Count);
            // Analyze the data and maybe remove points that weren't good.
            //calibration.DiscardData(new NormalizedPoint2D(0.1f, 0.1f));
            // Redo collection at the discarded point.
            //WriteLog("Show point on screen at ({0}, {1})", 0.1f, 0.1f);
            //await calibration.CollectDataAsync(new NormalizedPoint2D(0.1f, 0.1f));
            // Compute and apply again.
            //calibrationResult = await calibration.ComputeAndApplyAsync();
            //WriteLog("Second compute and apply returned {0} and collected at {1} points.",
            //calibrationResult.Status, calibrationResult.CalibrationPoints.Count);
            // See that you're happy with the result.
            // The calibration is done. Leave calibration mode.
            await calibration.LeaveCalibrationModeAsync();
            //WriteLog("Calibration done");
            //TrackBox tb = eyeTracker.GetTrackBox();
            //WriteLog("trackBox: backLowerLeft: {0}; backLowerRight: {1}; backUpperLeft: {2}; backUpperRight: {3}", FormatPoint3D(tb.BackLowerLeft), FormatPoint3D(tb.BackLowerRight), FormatPoint3D(tb.BackUpperLeft), FormatPoint3D(tb.BackUpperRight));
            //WriteLog("trackBox: frontLowerLeft: {0}; FrontLowerRight: {1}; FrontUpperLeft: {2}; FrontUpperRight: {3}", FormatPoint3D(tb.FrontLowerLeft), FormatPoint3D(tb.FrontLowerRight), FormatPoint3D(tb.FrontUpperLeft), FormatPoint3D(tb.FrontUpperRight));
        }

        private Point ToPoint(NormalizedPoint2D point)
        {
            return new Point(PositionToWidth(point.X), PositionToHeight(point.Y));
        }

        private double PositionToWidth(double position)
        {
            return this.Width * position;
        }

        private double PositionToHeight(double position)
        {
            return this.Height * position;
        }

        private void DrawLine(Point fromPoint, Point toPoint, Brush brush)
        {
            var line = new Line()
            {
                Stroke = brush,
                X1 = fromPoint.X,
                Y1 = fromPoint.Y,
                X2 = toPoint.X,
                Y2 = toPoint.Y
            };

            PaintSurface.Children.Add(line);
        }

        private void VisualizeButton_Click(object sender, RoutedEventArgs e)
        {
            ActionsPanel.Visibility = Visibility.Hidden;
            VisualizationPanel.Visibility = Visibility.Visible;
            _tracker.GazeDataReceived += EyeTracker_GazeDataReceived;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearSurface();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ActionsPanel.Visibility = Visibility.Visible;
            VisualizationPanel.Visibility = Visibility.Hidden;
            _tracker.GazeDataReceived -= EyeTracker_GazeDataReceived;
        }

        private void EyeTracker_GazeDataReceived(object sender, GazeDataEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var gazePoint = e.LeftEye.GazePoint;
                if (gazePoint.Validity == Validity.Valid)
                {
                    if (_lastLeftPoint != null)
                    {
                        var tmp = ToPoint(gazePoint.PositionOnDisplayArea);
                        DrawLine(_lastLeftPoint.Value, tmp, _blueBrush);
                        _lastLeftPoint = tmp;
                    } else
                    {
                        _lastLeftPoint = ToPoint(gazePoint.PositionOnDisplayArea);
                    }
                }

                gazePoint = e.RightEye.GazePoint;
                if (gazePoint.Validity == Validity.Valid)
                {
                    if (_lastRightPoint != null)
                    {
                        var tmp = ToPoint(gazePoint.PositionOnDisplayArea);
                        DrawLine(_lastRightPoint.Value, tmp, _greenBrush);
                        _lastRightPoint = tmp;
                    }
                    else
                    {
                        _lastRightPoint = ToPoint(gazePoint.PositionOnDisplayArea);
                    }
                }
            }));
        }
    }
}
