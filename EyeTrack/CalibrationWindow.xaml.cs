using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Tobii.Research;

namespace EyeTrack
{
    public partial class CalibrationWindow
    {
        private readonly IEyeTracker _tracker;

        private Status _status = Status.NotStarted;

        private const int CircleDiameter = 10;
        private readonly Brush _redBrush = new SolidColorBrush(Colors.Red);
        private readonly Brush _greenBrush = new SolidColorBrush(Colors.Green);
        private readonly Brush _blueBrush = new SolidColorBrush(Colors.Blue);
        private readonly Brush _grayBrush = new SolidColorBrush(Colors.Gray);
        private Point? _lastLeftPoint = null;
        private Point? _lastRightPoint = null;

        public CalibrationWindow()
        {
            InitializeComponent();
            _tracker = App.Tracker;
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

        private async void Calibrate()
        {
            ClearSurface();
            // Create a calibration object.
            var calibration = new ScreenBasedCalibration(_tracker);
            // Enter calibration mode.
            await calibration.EnterCalibrationModeAsync();
            // Define the points on screen we should calibrate at.
            // The coordinates are normalized, i.e. (0.0f, 0.0f) is the upper left corner and (1.0f, 1.0f) is the lower right corner.
            var pointsToCalibrate = new[]
            {
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
                    }
                    else
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
            _status = Status.CalibrationCompleted;
            MessageLabel.Content = "Калибровка завершена. Нажмите \"Пробел\" для визуализации";
            MessageLabel.Visibility = Visibility.Visible;

            if (drawErrorCount > 0)
            {
                MessageBox.Show($"{drawErrorCount} points were not been drawn");
            }

            await calibration.LeaveCalibrationModeAsync();
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
                    }
                    else
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

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Space) return;
            switch (_status)
            {
                case Status.NotStarted:
                    MessageLabel.Visibility = Visibility.Hidden;
                    _status = Status.Calibration;
                    Calibrate();
                    break;
                case Status.CalibrationCompleted:
                    MessageLabel.Content = "Режим визуализации. Нажмите \"Пробел\", чтобы остановить";
                    _tracker.GazeDataReceived += EyeTracker_GazeDataReceived;
                    _status = Status.Visualization;
                    break;
                case Status.Visualization:
                    MessageLabel.Content = "Визуализация закончена. Нажмите \"Пробел\", чтобы выйти";
                    _tracker.GazeDataReceived -= EyeTracker_GazeDataReceived;
                    _status = Status.VisualizationCompleted;
                    break;
                case Status.VisualizationCompleted:
                    ClearSurface();
                    Close();
                    break;
            }

            e.Handled = true;
        }
    }

    internal enum Status
    {
        NotStarted,
        Calibration,
        CalibrationCompleted,
        Visualization,
        VisualizationCompleted
    }
}