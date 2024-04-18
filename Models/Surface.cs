using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace IIS_Visual.Models
{
    public class Surface
    {
        public ChartValues<ObservablePoint> surfaceDataPoints = new ChartValues<ObservablePoint> {
            new ObservablePoint(0, 0), new ObservablePoint(20, 0), new ObservablePoint(20, 8), new ObservablePoint(35, 8),
            new ObservablePoint(35, 0), new ObservablePoint(40, 0), new ObservablePoint(48, 5), new ObservablePoint(55, 5)};
        private Dictionary<int, ChartValues<ObservablePoint>> surfaces = new Dictionary<int, ChartValues<ObservablePoint>>();
        private ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();
        public SolidColorBrush surfaceColor = Brushes.Gray;
        public double U = 0.010;
        public double Ef = 5.710;
        public double k = 1.0;
        public double phi0 = 4.50;
        public int d1 = 15, d2 = 5, h1 = 8, h2 = 5;

        public Surface()
        {
            for (int i = 0; i < surfaceDataPoints.Count - 1; i++)
            {
                surfaces.Add(i + 1, new ChartValues<ObservablePoint> { surfaceDataPoints[i], surfaceDataPoints[i + 1] });
            }

            foreach (var surfacePoints in surfaces.Values)
            {
                double startX = surfacePoints[0].X;
                double startZ = surfacePoints[0].Y;
                double endX = surfacePoints[1].X;
                double endZ = surfacePoints[1].Y;

                points.Add(new ObservablePoint(startX, startZ));

                if (startX == endX)
                {
                    if (startZ < endZ)
                    {
                        for (double z = startZ + 1; z < endZ; z++)
                        {
                            points.Add(new ObservablePoint(startX, z));
                        }
                    } 
                    else
                    {
                        for (double z = startZ - 1; z > endZ; z--)
                        {
                            points.Add(new ObservablePoint(startX, z));
                        }
                    }
                }
                else
                {
                    for (double x = startX + 1; x < endX; x++)
                    {
                        double z = InterpolateZ(startX, startZ, endX, endZ, x);
                        points.Add(new ObservablePoint(x, z));
                    }
                }
            }
        }
        public ChartValues<ObservablePoint> GetPoints()
        {
            return points;
        }
        public HashSet<int> CurrentSurfaces(double X, double Z, double R)
        {
            HashSet<int> result = new HashSet<int>();

            foreach (var point in points) 
            {
                double pointX = point.X;
                double pointZ = point.Y;

                double distance = Math.Sqrt(Math.Pow(pointX - X, 2) + Math.Pow(pointZ - Z, 2));

                if (distance <= R)
                {
                    int surface = DetermineSurface(point, surfaces);

                    result.Add(surface);
                }
            }

            return result;
        }
        private double InterpolateZ(double startX, double startZ, double endX, double endZ, double x)
        {
            double slope = (endZ - startZ) / (endX - startX);
            return startZ + slope * (x - startX);
        }
        private int DetermineSurface(ObservablePoint point, Dictionary<int, ChartValues<ObservablePoint>> surfaces)
        {
            foreach (var surface in surfaces)
            {
                if (IsPointOnSurface(point, surface.Value))
                {
                    return surface.Key;
                }
            }
            return 0;
        }
        private static bool IsPointOnSurface(ObservablePoint point, ChartValues<ObservablePoint> surfacePoints)
        {
            int polygonLength = surfacePoints.Count;
            double pointX = point.X;
            double pointZ = point.Y;
            bool inside = false;

            double vertex1X = surfacePoints[0].X;
            double vertex1Z = surfacePoints[0].Y;
            double vertex2X = surfacePoints[1].X;
            double vertex2Z = surfacePoints[1].Y;

            if (pointX == vertex2X && pointZ == vertex2Z) { return false; }

            if ((pointX < vertex2X && pointX >= vertex1X) || (pointZ < vertex2Z && pointZ >= vertex1Z) || (pointZ > vertex2Z && pointZ <= vertex1Z)) { return true; }

            return inside;
        }
    }
}
