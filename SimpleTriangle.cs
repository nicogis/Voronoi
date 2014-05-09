namespace Voronoi
{
    /// <summary>
    /// class Triangulation
    /// </summary>
    public partial class Triangulation
    {
        /// <summary>
        /// Declare a simple triangle struct
        /// </summary>
        private struct SimpleTriangle
        {
            /// <summary>
            /// Index entries to each vertex
            /// </summary>
            public int A, B, C;

            /// <summary>
            /// x, y of the centre, and radius of the circle
            /// </summary>
            public SimplePoint CircumCentre;

            /// <summary>
            /// radius of triangle
            /// </summary>
            public double Radius;

            /// <summary>
            /// Initializes a new instance of the <see cref="SimpleTriangle"/> struct
            /// </summary>
            /// <param name="a">index vertex a</param>
            /// <param name="b">index vertex b</param>
            /// <param name="c">index vertex c</param>
            /// <param name="circumcentre">center of triangle</param>
            /// <param name="radius">radius of triangle</param>
            public SimpleTriangle(int a, int b, int c, SimplePoint circumcentre, double radius)
            {
                this.A = a;
                this.B = b;
                this.C = c;
                this.CircumCentre = circumcentre;
                this.Radius = radius;
            }
        }
    }
}
