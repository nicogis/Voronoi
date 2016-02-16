//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGIS.Voronoi
{
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.ADF;
    using ESRI.ArcGIS.esriSystem;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// class main
    /// </summary>
    public class Program
    {
        /// <summary>
        /// arcobjects License Initializer
        /// </summary>
        private static LicenseInitializer arcobjectsLicenseInitializer = new LicenseInitializer();
        
        /// <summary>
        /// main method
        /// </summary>
        /// <param name="args">array of args</param>
        [STAThread]
        private static void Main(string[] args)
        {
            // ESRI License Initializer generated code.
            if (!arcobjectsLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeEngine, esriLicenseProductCode.esriLicenseProductCodeBasic, esriLicenseProductCode.esriLicenseProductCodeStandard, esriLicenseProductCode.esriLicenseProductCodeAdvanced }, new esriLicenseExtensionCode[] { }))
            {
                System.Console.WriteLine(arcobjectsLicenseInitializer.LicenseMessage());
                System.Console.WriteLine("This application could not initialize with the correct ArcGIS license and will shutdown.");
                arcobjectsLicenseInitializer.ShutdownApplication();
                return;
            }

            IWorkspace workspace = Program.FileGdbWorkspaceFromPath(@"c:\temp\voronoi.gdb");
            IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;
            IFeatureClass featureClassPoints = featureWorkspace.OpenFeatureClass("Points");
            IFeatureClass featureClassPolygons = featureWorkspace.OpenFeatureClass("Polygons");

            List<IPoint> locations = new List<IPoint>();

            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureCursor cursor = featureClassPoints.Search(null, true);
                comReleaser.ManageLifetime(cursor);
                IFeature feature = null;
                while ((feature = cursor.NextFeature()) != null)
                {
                    IPoint p = feature.ShapeCopy as IPoint;
                    locations.Add(p);
                }
            }

            IList<IGeometry> thiessenPolygons = Triangulation.GeometryVoronoi(locations);
                    
            int idxId = featureClassPolygons.FindField("Id");
            int i = 0;
            foreach (IGeometry pg in thiessenPolygons)
            {
                i++;
                try
                {
                    IFeature feature = featureClassPolygons.CreateFeature();
                    feature.Shape = pg as IPolygon;
                    feature.set_Value(idxId, i);
                    feature.Store();
                }
                catch
                {
                }
            }
            
            // ESRI License Initializer generated code.
            // Do not make any call to ArcObjects after ShutDownApplication()
            arcobjectsLicenseInitializer.ShutdownApplication();
        }

        /// <summary>
        /// open file GDB
        /// </summary>
        /// <param name="path">path of file geodatabase</param>
        /// <returns>objects workspace</returns>
        private static IWorkspace FileGdbWorkspaceFromPath(string path)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            return workspaceFactory.OpenFromFile(path, 0);
        }
    }
}
