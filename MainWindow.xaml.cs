using IIS_Visual.Models;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace IIS_Visual
{
    public partial class MainWindow : Window
    {
        public SeriesCollection seriesCollection { get; set; }
        private readonly DispatcherTimer _timer;
        private int _counterX = 0;
        private int _counterZ = 0;
        public Surface _surface = new Surface();
        public Needle _needle = new Needle(2);
        public Random _random = new Random();

        public double xMax = 10;
        public double yMax = 10;
        public int numPoints = 10000;
        public int size = 100;
        public List<double> zValues;
        public double refCurrent;
        public ObservablePoint lastPoint;
        public MainWindow()
        {
            InitializeComponent();
            lastPoint = _surface.GetPoints().Last();

            Axis zAxis = new Axis
            {
                Title = "Ось Z",
                MaxValue = 30, 
                MinValue = 0,
                Separator = new Separator { Step = 1 },
                Position = AxisPosition.LeftBottom
            };
            Axis yAxis = new Axis
            {
                Title = "Ось X",
                MinValue = 0,
                MaxValue = lastPoint.X,
                Separator = new Separator { Step = 1 },
                Position = AxisPosition.LeftBottom
            };

            Chart_1.AxisY.Add(zAxis);
            Chart_1.AxisX.Add(yAxis);

            seriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Поверхность",
                    PointGeometry = DefaultGeometries.None,
                    Values = new ChartValues<ObservablePoint> { },
                    LineSmoothness = 0,
                    Fill = _surface.surfaceColor,
                    Stroke = _surface.surfaceColor
                },
                new LineSeries
                {
                    Title = "Игла",
                    PointGeometry = DefaultGeometries.Circle,
                    Values = new ChartValues<ObservablePoint> { },
                    Fill = Brushes.Transparent
                }
            };

            DataContext = this;

            zValues = new List<double>(new double[size]);
            zValues[_counterZ] = _needle.Zo;

            refCurrent = MonteCarloDoubleIntegral(_needle.Zo, -xMax, xMax);

            seriesCollection[0].Values.AddRange(_surface.GetPoints());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) }; // каждые X миллисекунд добавляется новое значение на график
            _timer.Tick += TimerTick;
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _timer.Start();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            if (_counterX > lastPoint.X)
            {
                _timer.Stop(); // останавливаем таймер
            }
            else
            {
                double _currentJ = CalculateCurrents(_surface, _counterX);
                while (Math.Abs(1.0 - _currentJ / refCurrent) >= 0.050) // n% диапазон доверия
                {
                    if (_currentJ > refCurrent)
                    {
                        zValues[_counterZ]= zValues[_counterZ] + _needle.step;
                    }
                    else 
                    {
                        zValues[_counterZ] = zValues[_counterZ] - _needle.step;
                    }
                    _currentJ = CalculateCurrents(_surface, _counterX);
                }
                (seriesCollection[1]).Values.Add(new ObservablePoint(_counterX, zValues[_counterZ]));
            }
            zValues[_counterZ + 1] = zValues[_counterZ];
            _counterX++; _counterZ++;
        }
        private double CalculateCurrent(double x, double y, double Zi, Surface surface)
        {
            double Z = Math.Sqrt(Zi + Math.Pow(x, 2) + Math.Pow(y, 2));
            double S1 = 3 / (surface.k * surface.phi0);
            double S2 = Zi * (1 - (23 / (3 * surface.phi0 * surface.k * Zi + 10 - 2 * surface.U * surface.k * Zi))) + S1;
            double phi = surface.phi0 - ((surface.U * (S1 + S2)) / (2 * Zi)) - (2.86 / (surface.k * (S2 - S1))) * Math.Log((S2 * (Zi - S1)) / (S1 * (Zi - S2)));
            return 1620 * surface.U * surface.Ef * Math.Exp(-1.0250 * Z * Math.Sqrt(phi));
        }
        private double MonteCarloDoubleIntegral(double Zi, double xL, double xH)
        {
            double integral = 0.0;
            double x, y;
            for (int i = 0; i < numPoints; i++)
            {
                x = xL + (xH - xL) * _random.NextDouble();
                y = -yMax + (yMax - (-yMax)) * _random.NextDouble();
                integral += CalculateCurrent(x, y, Zi, _surface); 
            }

            double area = (xH - xL) * (yMax - (-yMax)); // площадь прямоугольника
            integral *= area / numPoints; // вычисление приближенного значения интеграла

            return integral;
        }
        private double CalculateCurrents(Surface surface, int _currentX) //todo
        {
            try
            {
                double It = 0;
                var set = _surface.CurrentSurfaces(_currentX, zValues[_counterZ], _needle.radius);
                
                foreach (var surfaceIndex in set)
                {
                    switch (surfaceIndex)
                    {
                        case 1:
                            double I1 = MonteCarloDoubleIntegral(zValues[_counterZ], -xMax, xMax);
                            It += I1;
                            break;
                        case 2:
                            if (_currentX <= _surface.surfaceDataPoints[surfaceIndex].X)
                            {
                                double I2 = MonteCarloDoubleIntegral(Math.Sqrt(Math.Pow((zValues[_counterZ] - _surface.h1) + _surface.h1 * 0.5, 2)
                                + Math.Pow(_currentX - _surface.surfaceDataPoints[surfaceIndex - 1].X, 2)), 0, _surface.h1);
                                It += I2;
                            }
                            break;
                        case 3:
                            if (_currentX >= _surface.surfaceDataPoints[surfaceIndex - 1].X)
                            {
                                double I3 = MonteCarloDoubleIntegral(zValues[_counterZ] - _surface.h1, -xMax, xMax);
                                It += I3;
                            }
                            else
                            {
                                double I3 = MonteCarloDoubleIntegral(Math.Sqrt(Math.Pow((zValues[_counterZ] - _surface.h1), 2)
                                + Math.Pow(_currentX - _surface.surfaceDataPoints[surfaceIndex - 1].X, 2)), -xMax, xMax);
                                It += I3;
                            }
                            break;
                        case 4:
                            if (_currentX >= _surface.surfaceDataPoints[surfaceIndex].X)
                            {
                                double I4 = MonteCarloDoubleIntegral(Math.Sqrt(Math.Pow((zValues[_counterZ] - _surface.h1) + _surface.h1 * 0.5, 2)
                                + Math.Pow(_currentX - _surface.surfaceDataPoints[surfaceIndex - 1].X, 2)), 0, _surface.h1);
                                It += I4;
                            }
                            break;
                        case 5:
                            double I5 = MonteCarloDoubleIntegral(zValues[_counterZ], -xMax, xMax);
                            It += I5;
                            break;
                        case 6:
                            double I6 = MonteCarloDoubleIntegral(Math.Sqrt(Math.Pow((zValues[_counterZ] - _surface.h2) + _surface.h2 * 0.5, 2)
                                + Math.Pow(_currentX - _surface.surfaceDataPoints[surfaceIndex].X, 2)), -xMax, xMax);
                            It += I6;
                            break;
                        case 7:
                            double I7 = MonteCarloDoubleIntegral(zValues[_counterZ] - _surface.h2, -xMax, xMax);
                            It += I7;
                            break;
                    }
                }

                return It;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return 0;
            }
        }
    }
}