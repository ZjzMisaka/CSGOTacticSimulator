using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOTacticsSimulator.Helper
{
    static public class VectorHelper
    {
        static public double GetDistance(Point startPoint, Point endPoint)
        {
            return Math.Sqrt(Math.Pow((startPoint.X - endPoint.X), 2) + Math.Pow((startPoint.Y - endPoint.Y), 2));
        }

        //static public Point GetDirection(Point startPoint, Point endPoint)
        //{
        //    return new Point(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
        //}

        static public Point GetUnitVector(Point startPoint, Point endPoint)
        {
            double dist = GetDistance(startPoint, endPoint);
            return new Point((endPoint.X - startPoint.X) / dist, (endPoint.Y - startPoint.Y) / dist);
        }

        static public Point Multiply(Point point, double multiplier)
        {
            return new Point(point.X * multiplier, point.Y * multiplier);
        }

        static public Point Add(Point point, Point addend)
        {
            return new Point(point.X + addend.X, point.Y + addend.Y);
        }

        static public Point Cast(string pointStr)
        {
            pointStr = pointStr.Replace("(", "").Replace(")", "");
            return new Point(Double.Parse(pointStr.Split(',')[0]), Double.Parse(pointStr.Split(',')[1]));
        }
    }
}
