using System;

namespace IIS_Visual.Models
{
    public class Needle
    {
        public double Zo;
        public double step = 0.10;
        public double radius;
        public Needle(double Z0, double offset) 
        {
            Zo = Z0;
            radius = Zo + offset;
        }
    }
}
