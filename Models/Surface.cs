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
            new ObservablePoint(35, 0), new ObservablePoint(40, 0), new ObservablePoint(49, 5), new ObservablePoint(55, 5)};
        public SolidColorBrush surfaceColor = Brushes.Gray;
        public double U = 0.010;
        public double Ef = 5.710;
        public double k = 1.0;
        public double phi0 = 4.50;
        public int d1 = 15, d2 = 5, h1 = 8, h2 = 5;
        public ChartValues<ObservablePoint> ReadSurfaceDataPointsFromFile()
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
            int partNumber = 0;
            for(var point = 0; point < surfaceDataPoints.Count; point++)
            {
                if (_currentX < surfaceDataPoints[point].X)
                {
                    partNumber = point;
                    break;
                }
            }
            switch (partNumber)
            {
                case 1:
                    partNumber = 0;
                    break;
                case 3:
                    partNumber = 1;
                    break;
                case 5:
                    partNumber = 2;
                    break;
                case 6:
                    partNumber = 2;
                    break;
                case 7:
                    partNumber = 3;
                    break;
            }
            return partNumber;
        }
    }
}
