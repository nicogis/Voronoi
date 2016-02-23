//-----------------------------------------------------------------------
// <copyright file="Helper.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGIS.Voronoi
{
    using System;
    using ESRI.ArcGIS.DataSourcesGDB;
    using ESRI.ArcGIS.Geodatabase;
    using ESRI.ArcGIS.Geometry;

    /// <summary>
    /// class of helper
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// open file GDB
        /// </summary>
        /// <param name="path">path of file geodatabase</param>
        /// <returns>objects workspace</returns>
        internal static IWorkspace FileGdbWorkspaceFromPath(string path)
        {
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            return workspaceFactory.OpenFromFile(path, 0);
        }

        /// <summary>
        /// check exists file geodatabase
        /// </summary>
        /// <param name="pathFileName">path and name of geodatabase</param>
        /// <returns>true if exists</returns>
        internal static bool ExistsFileGdb(string pathFileName)
        {
            IWorkspaceFactory2 wsf = new FileGDBWorkspaceFactoryClass() as IWorkspaceFactory2;
            return wsf.IsWorkspace(pathFileName);
        }

        /// <summary>
        /// get spatial reference of feature class
        /// </summary>
        /// <param name="featureClass">object feature class</param>
        /// <returns>object spatial reference</returns>
        internal static ISpatialReference GetSpatialReference(IFeatureClass featureClass)
        {
            ISpatialReference spatialReference = null;
            if (featureClass != null)
            {
                IGeoDataset geoDataset = featureClass as IGeoDataset;
                spatialReference = geoDataset.SpatialReference;
            }

            return spatialReference;
        }
    }
}
