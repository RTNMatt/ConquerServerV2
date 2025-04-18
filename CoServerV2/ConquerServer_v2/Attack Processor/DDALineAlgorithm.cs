using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Core;

namespace ConquerServer_v2.Attack_Processor
{
    public struct Point
    {
        public int X;
        public int Y;
        public Point(int x, int y) { X = x; Y = y; }
    }

    public class DDALineAlgorithm
    {
        private static void DDALineEx(int x0, int y0, int x1, int y1, List<Point> vctPoint)
        {
            if ((x0 != x1) || (y0 != y1))
            {
                int dx = x1 - x0;
                int dy = y1 - y0;
                int abs_dx = Math.Abs(dx);
                int abs_dy = Math.Abs(dy);
                if (abs_dx > abs_dy)
                {
                    int _0_5 = abs_dx * ((dy > 0) ? 1 : -1);
                    int numerator = dy * 2;
                    int denominator = abs_dx * 2;
                    if (dx > 0)
                    {
                        for (int i = 1; i <= abs_dx; i++)
                        {
                            Point point;
                            point.X = x0 + i;
                            point.Y = y0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                    else if (dx < 0)
                    {
                        for (int i = 1; i <= abs_dx; i++)
                        {
                            Point point;
                            point.X = x0 - i;
                            point.Y = y0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                }
                else
                {
                    int _0_5 = abs_dy * ((dx > 0) ? 1 : -1);
                    int numerator = dx * 2;
                    int denominator = abs_dy * 2;
                    if (dy > 0)
                    {
                        for (int i = 1; i <= abs_dy; i++)
                        {
                            Point point;
                            point.Y = y0 + i;
                            point.X = x0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                    else if (dy < 0)
                    {
                        for (int i = 1; i <= abs_dy; i++)
                        {
                            Point point;
                            point.Y = y0 - i;
                            point.X = x0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                }
            }
        }

        public static Point[] Line(int x0, int y0, int x1, int y1, int nRange)
        {
            double dist = Kernel.GetE2DDistance(x0, y0, x1, y1);
            List<Point> vctPoint = new List<Point>();
            if (dist <= nRange)
                vctPoint.Add(new Point(x1, y1));
            if ((x0 != x1) || (y0 != y1))
            {
                double scale = ((double)(1 * nRange)) / dist;
                x1 = ((int)(0.5 + (scale * (x1 - x0)))) + x0;
                y1 = ((int)(0.5 + (scale * (y1 - y0)))) + y0;
                DDALineEx(x0, y0, x1, y1, vctPoint);
            }
            return vctPoint.ToArray();
        }
    }
}
