namespace Voronoi
{
    using System;

    /// <summary>
    /// class Triangulation
    /// </summary>
    public partial class Triangulation
    {
        /// <summary>
        /// Define a simple point structure
        /// </summary>
        private struct SimplePoint : IComparable
        {
            /// <summary>
            /// coordinate X and Y
            /// </summary>
            public double X, Y;

            /// <summary>
            /// Initializes a new instance of the <see cref="SimplePoint"/> struct
            /// </summary>
            /// <param name="x">coordinate X</param>
            /// <param name="y">coordinate Y</param>
            public SimplePoint(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            /// <summary>
            /// Implement IComparable CompareTo method to enable sorting
            /// </summary>
            /// <param name="obj">object simple point</param>
            /// <returns>value of compare</returns>
            int IComparable.CompareTo(object obj)
            {
                SimplePoint other = (SimplePoint)obj;
                if (this.X > other.X)
                {
                    return 1;
                }
                else if (this.X < other.X)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
