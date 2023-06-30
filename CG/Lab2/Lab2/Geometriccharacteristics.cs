using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2
{
    public static class Geometriccharacteristics
    {
        public static int GetArea(int[,] labels, int label)
        {
            int area = 0;
            for (int y = 0; y < labels.GetLength(1); y++)
            {
                for (int x = 0; x < labels.GetLength(0); x++)
                {
                    if (labels[x, y] == label)
                    {
                        area++;
                    }
                }
            }
            return area;
        }
        public static Tuple<int, int> GetCentroid(int[,] labels, int label)
        {
            int area = GetArea(labels, label);
            int cx = 0;
            int cy = 0;
            for (int y = 0; y < labels.GetLength(1); y++)
            {
                for (int x = 0; x < labels.GetLength(0); x++)
                {
                    if (labels[x, y] == label)
                    {
                        cx += x;
                        cy += y;
                    }
                }
            }
            cx /= area;
            cy /= area;
            return Tuple.Create(cx, cy);
        }

        public static int GetPerimeter(int[,] labels, int label)
        {
            int perimeter = 0;
            for (int y = 0; y < labels.GetLength(1); y++)
            {
                for (int x = 0; x < labels.GetLength(0); x++)
                {
                    if (labels[x, y] == label)
                    {
                        if (x == 0 || x == labels.GetLength(1) - 1 || y == 0 || y == labels.GetLength(0) - 1)
                        {
                            perimeter++;
                        }
                        else if (labels[x,y-1] != label || labels[x,y+1] != label || labels[x - 1,y] != label || labels[x+1,y] != label)
                        {
                            perimeter++;
                        }
                    }
                }
            }
            return perimeter;
        }
        public static double GetCompactness(int[,] labels, int label)
        {
            int area = GetArea(labels, label);
            int perimeter = GetPerimeter(labels, label);
            double compactness = 4 * Math.PI * area / (perimeter * perimeter);
            return compactness;
        }
        public static double GetOrientation(int[,] labels, int label)
        {
            var center = GetCentroid(labels, label);
            double cx = center.Item1;
            double cy = center.Item2;
            double ixx = 0;
            double iyy = 0;
            double ixy = 0;
            for (int y = 0; y < labels.GetLength(1); y++)
            {
                for (int x = 0; x < labels.GetLength(0); x++)
                {
                    if (labels[x, y] == label)
                    {
                        double dx = x - cx;
                        double dy = y - cy;
                        ixx += dx * dx;
                        iyy += dy * dy;
                        ixy += dx * dy;
                    }
                }
            }
            return 0.5 * Math.Atan2(2 * ixy, ixx - iyy);
        }
        public static double GetEccentricity(int[,] labels, int label)
        {
            int area = GetArea(labels, label);
            var center = GetCentroid(labels, label);
            double cx = center.Item1;
            double cy = center.Item2;
            double ixx = 0;
            double iyy = 0;
            double ixy = 0;
            for (int y = 0; y < labels.GetLength(1); y++)
            {
                for (int x = 0; x < labels.GetLength(0); x++)
                {
                    if (labels[x, y] == label)
                    {
                        double dx = x - cx;
                        double dy = y - cy;
                        ixx += dx * dx;
                        iyy += dy * dy;
                        ixy += dx * dy;
                    }
                }
            }
            double lambda1 = (ixx + iyy + Math.Sqrt((ixx - iyy) * (ixx - iyy) + 4 * ixy * ixy)) / (2 * area);
            double lambda2 = (ixx + iyy - Math.Sqrt((ixx - iyy) * (ixx - iyy) + 4 * ixy * ixy)) / (2 * area);
            double eccentricity = Math.Sqrt(1 - Math.Min(lambda1, lambda2) / Math.Max(lambda1, lambda2));
            return eccentricity;
        }
    }
}
