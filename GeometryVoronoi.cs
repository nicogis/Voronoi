namespace Voronoi
{
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// triangulation class
    /// </summary>
    public partial class Triangulation
    {
        /// <summary>
        /// Calculate diagram voronoi from list of points
        /// </summary>
        /// <param name="points">list of points</param>
        /// <returns>list of polygons</returns>
        public static IList<IGeometry> GeometryVoronoi(List<IPoint> points)
        {
            // Check valid input
            if (points.Count < 3)
            {
                throw new ArgumentException("Input must be a MultiPoint containing at least three points");
            }

            // Initialise a list of vertices
            List<SimplePoint> vertices = new List<SimplePoint>();

            // Add all the original supplied points
            for (int i = 0; i < points.Count; i++)
            {
                SimplePoint point = new SimplePoint(points[i].X, points[i].Y);

                // MultiPoints can contain the same point twice, but this messes up Delaunay
                if (!vertices.Contains(point))
                {
                    vertices.Add(point);
                }
            }

            // Important - count the number of points in the array as some duplicate points
            // may have been removed
            int numPoints = vertices.Count;

            // Check valid input
            if (numPoints < 3)
            {
                throw new ArgumentException("Input must be a list of points containing at least three points");
            }

            // Important! Sort the list so that points sweep from left - right
            vertices.Sort();

            IPointCollection pointCollection = new MultipointClass();
            foreach (SimplePoint p in vertices)
            {
                pointCollection.AddPoint(new PointClass() { X = p.X, Y = p.Y });
            }

            // Calculate the "supertriangle" that encompasses the pointset
            IEnvelope envelope = (pointCollection as IGeometry).Envelope;

            // Width
            double dx = envelope.Width;

            // Height 
            double dy = envelope.Height;

            // Maximum dimension
            double dmax = (dx > dy) ? dx : dy;

            // Centre
            double avgx = ((envelope.XMax - envelope.XMin) / 2) + envelope.XMin;
            double avgy = ((envelope.YMax - envelope.YMin) / 2) + envelope.YMin;

            // Create the points at corners of the supertriangle
            SimplePoint a = new SimplePoint(avgx - (2 * dmax), avgy - dmax);
            SimplePoint b = new SimplePoint(avgx + (2 * dmax), avgy - dmax);
            SimplePoint c = new SimplePoint(avgx, avgy + (2 * dmax));

            // Add the supertriangle vertices to the end of the vertex array
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            double radius;
            SimplePoint circumcentre;
            Triangulation.CalculateCircumcircle(a, b, c, out circumcentre, out radius);

            // Create a triangle from the vertices
            SimpleTriangle superTriangle = new SimpleTriangle(numPoints, numPoints + 1, numPoints + 2, circumcentre, radius);

            // Add the supertriangle to the list of triangles
            List<SimpleTriangle> triangles = new List<SimpleTriangle>();
            triangles.Add(superTriangle);

            List<SimpleTriangle> completedTriangles = new List<SimpleTriangle>();

            // Loop through each point
            for (int i = 0; i < numPoints; i++)
            {
                // Initialise the edge buffer
                List<int[]> edges = new List<int[]>();

                // Loop through each triangle
                for (int j = triangles.Count - 1; j >= 0; j--)
                {
                    // If the point lies within the circumcircle of this triangle
                    if (Distance(triangles[j].CircumCentre, vertices[i]) < triangles[j].Radius)
                    {
                        // Add the triangle edges to the edge buffer
                        edges.Add(new int[] { triangles[j].A, triangles[j].B });
                        edges.Add(new int[] { triangles[j].B, triangles[j].C });
                        edges.Add(new int[] { triangles[j].C, triangles[j].A });

                        // Remove this triangle from the list
                        triangles.RemoveAt(j);
                    }
                    else if (vertices[i].X > triangles[j].CircumCentre.X + triangles[j].Radius)
                    {
                        // If this triangle is complete
                        {
                            completedTriangles.Add(triangles[j]);
                        }

                        triangles.RemoveAt(j);
                    }
                }

                // Remove duplicate edges
                for (int j = edges.Count - 1; j > 0; j--)
                {
                    for (int k = j - 1; k >= 0; k--)
                    {
                        // Compare if this edge match in either direction
                        if (edges[j][0].Equals(edges[k][1]) && edges[j][1].Equals(edges[k][0]))
                        {
                            // Remove both duplicates
                            edges.RemoveAt(j);
                            edges.RemoveAt(k);

                            // We've removed an item from lower down the list than where j is now, so update j
                            j--;
                            break;
                        }
                    }
                }

                // Create new triangles for the current point
                for (int j = 0; j < edges.Count; j++)
                {
                    Triangulation.CalculateCircumcircle(vertices[edges[j][0]], vertices[edges[j][1]], vertices[i], out circumcentre, out radius);
                    SimpleTriangle t = new SimpleTriangle(edges[j][0], edges[j][1], i, circumcentre, radius);
                    triangles.Add(t);
                }
            }

            // We've finished triangulation. Move any remaining triangles onto the completed list
            completedTriangles.AddRange(triangles);

            IList<IGeometry> voronoiPolygon = new List<IGeometry>();
            for (var i = 0; i < vertices.Count; i++)
            {
                // Initiliase a new geometry to hold the voronoi polygon
                IPointCollection mp = new MultipointClass();

                // Look through each triangle
                foreach (SimpleTriangle tri in completedTriangles)
                {
                    // If the triangle intersects this point
                    if (tri.A == i || tri.B == i || tri.C == i)
                    {
                        mp.AddPoint(new PointClass() { X = tri.CircumCentre.X, Y = tri.CircumCentre.Y });
                    }
                }

                // Create the voronoi polygon from the convex hull of the circumcentres of intersecting triangles
                ITopologicalOperator topologicalOperator = mp as ITopologicalOperator;
                IGeometry polygon = topologicalOperator.ConvexHull();
                topologicalOperator = polygon as ITopologicalOperator;
                IGeometry result = topologicalOperator.Intersect(envelope, esriGeometryDimension.esriGeometry2Dimension);
                if ((result != null) && (!result.IsEmpty))
                {
                    voronoiPolygon.Add(result);
                }
            }

            return voronoiPolygon;
        }
    }
}