﻿using IIS_Visual.Models;
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
        public Needle _needle = new Needle();
        public Random _random = new Random();

        public int xLow = -5;
        public static int size = 60;
        public int xHigh = 5;
        public double yH = 5;
        public double yL = -5;
        public int numPoints = 5000;
        public List<double> zValues = new List<double>(size);
        public double delta = 1e-5;
        public double refCurrent;

        public MainWindow()
        {
            InitializeComponent();

            Axis zAxis = new Axis
            {
                Title = "Ось Z",
                MaxValue = _needle.Zo * 2, 
                MinValue = 0,
                Separator = new Separator { Step = 0.5 },
                Position = AxisPosition.LeftBottom
            };
            Axis yAxis = new Axis
            {
                Title = "Ось X",
                MinValue = 0,
                MaxValue = 50,
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

            for (int i = 0; i < size; i++)
                zValues.Add(_needle.Zo);

            refCurrent = MonteCarloDoubleIntegral(zValues[0], xLow, xHigh);

            seriesCollection[0].Values.AddRange(_surface.GetPoints());

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) }; // каждые X секунд добавляется новое значение на график
            _timer.Tick += TimerTick;
        }
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _timer.Start();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            (seriesCollection[1]).Values.Add(new ObservablePoint(_counterX, zValues[_counterZ]));

            _counterZ++;

            if (_counterX > 50)
            {
                //seriesCollection[1].Values.RemoveAt(0); // очищаем предыдущее значение или
                _timer.Stop(); // останавливаем таймер
            }
            else
            {
                double _currentJ = CalculateCurrents(_surface, _counterX);
                while (Math.Abs(1.0 - _currentJ / refCurrent) >= 0.10) // 10% диапазон доверия
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
            }
            _counterX++;
        }
        private double CalculateCurrent(double x, double y, double Zi, Surface surface)
        {
            double Z = Math.Sqrt(Zi + Math.Pow(x, 2) + Math.Pow(y, 2));
            double S1 = 3 / (surface.k * surface.phi0);
            double S2 = Z * (1 - (23 / (3 * surface.phi0 * surface.k * Z + 10 - 2 * surface.U * surface.k * Z))) + S1;
            double phi = surface.phi0 - ((surface.U * (S1 + S2)) / (2 * Z)) - (2.86 / (surface.k * (S2 - S1))) * Math.Log((S2 * (Z - S1)) / (S1 * (Z - S2)));
            return 1620 * surface.U * surface.Ef * Math.Exp(-1.025 * Z * Math.Sqrt(Math.Abs(phi)));
        }
        private double MonteCarloDoubleIntegral(double Zi, int xL, int xH)
        {
            double integral = 0.0;
            double x, y;
            for (int i = 0; i < numPoints; i++)
            {
                x = xL + (xH - xL) * _random.NextDouble();
                y = yL + (yH - yL) * _random.NextDouble();
                integral += CalculateCurrent(x, y, Zi, _surface); 
            }

            double area = (xH - xL) * (yH - yL); // площадь прямоугольника
            integral *= area / numPoints; // вычисление приближенного значения интеграла

            return integral;
        }
        private double CalculateCurrents(Surface surface, int _currentX) // todo
        {
            int currentSurface = surface.CurrentSurfacePart(_currentX);

            double It;
            switch (currentSurface)
            {
                case 0://   1-ый участок 0-10
                    It = MonteCarloDoubleIntegral(zValues[_counterZ], xLow, xHigh);
                    It += MonteCarloDoubleIntegral(Math.Sqrt(Math.Pow(zValues[_counterZ] - surface.h1, 2) + Math.Pow(surface.d1 - _currentX, 2)), 0, surface.h1);
                    break;
                case 1://   2-ой участок 10-30
                    It = MonteCarloDoubleIntegral(zValues[_counterZ] - surface.h1, xLow, xHigh);
                    break;
                case 2://   3-ий участок 30-42
                    It = MonteCarloDoubleIntegral(zValues[_counterZ], xLow, xHigh);
                    //todo
                    break;
                case 3://   4-ый участок 42-50
                    It = MonteCarloDoubleIntegral(zValues[_counterZ] - surface.h2, xLow, xHigh);
                    break;
                default:
                    It = 0;
                    throw new Exception("Неожиданный/необработанный случай, лол. Вот сиди и думай головой");
            }
            return It;
        }
    }
}