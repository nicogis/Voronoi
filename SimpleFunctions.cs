namespace Voronoi
{
    using System;

    /// <summary>
    /// class Triangulation
    /// </summary>
    public partial class Triangulation
    {
        /// <summary>
        /// calculate center of circle and radius using three points
        /// </summary>
        /// <param name="p1">first point</param>
        /// <param name="p2">second point</param>
        /// <param name="p3">third point</param>
        /// <param name="circumCentre">center of circle</param>
        /// <param name="radius">value of radius</param>
        private static void CalculateCircumcircle(SimplePoint p1, SimplePoint p2, SimplePoint p3, out SimplePoint circumCentre, out double radius)
        {
            // Calculate the length of each side of the triangle
            double a = Distance(p2, p3); // side a is opposite point 1
            double b = Distance(p1, p3); // side b is opposite point 2 
            double c = Distance(p1, p2); // side c is opposite point 3

            // Calculate the radius of the circumcircle
            double area = Math.Abs(((double)((p1.X * (p2.Y - p3.Y)) + (p2.X * (p3.Y - p1.Y)) + (p3.X * (p1.Y - p2.Y)))) / 2);
            radius = a * b * c / (4 * area);

            // Define area coordinates to calculate the circumcentre
            double pp1 = Math.Pow(a, 2) * (Math.Pow(b, 2) + Math.Pow(c, 2) - Math.Pow(a, 2));
            double pp2 = Math.Pow(b, 2) * (Math.Pow(c, 2) + Math.Pow(a, 2) - Math.Pow(b, 2));
            double pp3 = Math.Pow(c, 2) * (Math.Pow(a, 2) + Math.Pow(b, 2) - Math.Pow(c, 2));

            // Normalise
            double t1 = pp1 / (pp1 + pp2 + pp3);
            double t2 = pp2 / (pp1 + pp2 + pp3);
            double t3 = pp3 / (pp1 + pp2 + pp3);

            // Convert to Cartesian
            double x = (t1 * p1.X) + (t2 * p2.X) + (t3 * p3.X);
            double y = (t1 * p1.Y) + (t2 * p2.Y) + (t3 * p3.Y);

            circumCentre = new SimplePoint(x, y);
        }
 
        /// <summary>
        /// Calculate the distance between two SimplePoints
        /// </summary>
        /// <param name="p1">first point</param>
        /// <param name="p2">second point</param>
        /// <returns>distance between two points</returns>
        private static double Distance(SimplePoint p1, SimplePoint p2)
        {
            double result = 0;
            result = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            return result;
        }
    }
}
