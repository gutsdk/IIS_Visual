using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace IIS_Visual.Models
{
    public class Surface
    {
        private ChartValues<ObservablePoint> surfaceDataPoints = new ChartValues<ObservablePoint> {
            new ObservablePoint(0, 0), new ObservablePoint(20, 0), new ObservablePoint(20, 8), new ObservablePoint(35, 8),
            new ObservablePoint(35, 0), new ObservablePoint(40, 0), new ObservablePoint(48, 5), new ObservablePoint(55, 5)};
        public SolidColorBrush surfaceColor = Brushes.Gray;
        public double U = 0.010;
        public double Ef = 5.710;
        public double k = 1.0;
        public double phi0 = 4.50;
        public int d1 = 15, d2 = 5, h1 = 8, h2 = 5;
        public ChartValues<ObservablePoint> ReadSurfaceDataPointsFromFile() // если надо сделать универсальную реализацию
        {
            // необходимо реализовать функцию открытия файла .csv в котором хранятся значения поверхности 
            return surfaceDataPoints;// по умолчанию значения
        }
        public ChartValues<ObservablePoint> GetPoints()
        {
            return surfaceDataPoints;
        }
        public int CurrentSurfacePart(int _currentX)
        {
            int part = 0;
            for (int i = 0; i < surfaceDataPoints.Count - 1; i++)
            {
                double x1 = surfaceDataPoints[i].X;
                double y1 = surfaceDataPoints[i].Y;

                double x2 = surfaceDataPoints[i + 1].X;
                double y2 = surfaceDataPoints[i + 1].Y;

                if (y2 > y1)
                {
                // Если есть возвышение, то проверяем, находится ли точка выше линии между опорными точками
                    double interpolatedY = y1 + (_currentX - x1) * (y2 - y1) / (x2 - x1);
                    if (_currentX >= x1 && _currentX <= x2 && interpolatedY <= y2)
                    {
                        part = i + 1;     
                    }
                }
                else if (y2 < y1)
                {
                    // Если есть спуск, то проверяем, находится ли точка ниже линии между опорными точками
                    double interpolatedY = y1 + (_currentX - x1) * (y2 - y1) / (x2 - x1);
                    if (_currentX >= x1 && _currentX <= x2 && interpolatedY >= y2)
                    {
                        part = i + 1;
                    }
                }
                else
                {
                    // Если y1 == y2, это плоский участок, и мы проверяем, принадлежит ли точка этому участку
                    if (_currentX >= x1 && _currentX <= x2)
                    {
                        part = i + 1;
                    }
                }
            }

            switch (part)
            {
                case 1:
                    part = 1;
                    break;
                case 2:
                    part = 1;
                    break;
                case 3:
                    part = 2;
                    break;
                case 4:
                    part = 2;
                    break;
                case 5:
                    part = 3;
                    break;
                case 6:
                    part = 3;
                    break;
                case 7:
                    part = 4;
                    break;
            }

            return part;
        }
    }
}
