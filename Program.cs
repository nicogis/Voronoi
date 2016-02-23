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
    using System.Text;
    using CommandLine;
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
        /// name field of identifier polygon
        /// </summary>
        private static string nameFieldIdOutput = "Id";

        /// <summary>
        /// main method
        /// </summary>
        /// <param name="args">array of args</param>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                // ESRI License Initializer generated code.
                if (!arcobjectsLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeEngine, esriLicenseProductCode.esriLicenseProductCodeBasic, esriLicenseProductCode.esriLicenseProductCodeStandard, esriLicenseProductCode.esriLicenseProductCodeAdvanced }, new esriLicenseExtensionCode[] { }))
                {
                    Console.WriteLine(arcobjectsLicenseInitializer.LicenseMessage());
                    Console.WriteLine("This application could not initialize with the correct ArcGIS license and will shutdown.");
                    arcobjectsLicenseInitializer.ShutdownApplication();
                    return;
                }

                var options = new Options();
                Parser parser = new Parser();

                if (args.Length == 0 || args[0] == "-h" || args[0].Trim().ToLowerInvariant() == "help")
                {
                    Console.WriteLine(options.GetUsage());
                    Console.WriteLine("Press any key to continue ...");
                    Console.ReadKey();
                    return;
                }

                if (parser.ParseArguments(args, options))
                {
                    if (!Helper.ExistsFileGdb(options.PathAndFGDB))
                    {
                        Console.WriteLine("Filegeodatabase '{0}' not exists!", options.PathAndFGDB);
                        Console.WriteLine("Press any key to continue ...");
                        Console.ReadKey();
                        return;
                    }

                    IWorkspace workspace = Helper.FileGdbWorkspaceFromPath(options.PathAndFGDB);
                    IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;

                    string inputFeatureClassName = options.FeatureClassNameInput ?? "Points";

                    IWorkspace2 workspace2 = workspace as IWorkspace2;
                    if (!workspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, inputFeatureClassName))
                    {
                        Console.WriteLine("FeatureClass '{0}' not exists in filegeodatabase '{1}'!", inputFeatureClassName, options.PathAndFGDB);
                        Console.WriteLine("Press any key to continue ...");
                        Console.ReadKey();
                        return;
                    }

                    IFeatureClass featureClassPoints = featureWorkspace.OpenFeatureClass(inputFeatureClassName);
                    if (featureClassPoints.ShapeType != esriGeometryType.esriGeometryPoint)
                    {
                        Console.WriteLine("FeatureClass '{0}' is not type point!", inputFeatureClassName);
                        Console.WriteLine("Press any key to continue ...");
                        Console.ReadKey();
                        return;
                    }

                    string outputFeatureClassName = options.FeatureClassNameOutput ?? "Polygons";
                    if (workspace2.get_NameExists(esriDatasetType.esriDTFeatureClass, outputFeatureClassName))
                    {
                        Console.Write("\nThe featureClass '{0}' exists. You want to overwrite it? Y/N ", outputFeatureClassName);
                        while (true)
                        {
                            ConsoleKeyInfo answer = Console.ReadKey(true);

                            if (answer.Key == ConsoleKey.N)
                            {
                                return;
                            }
                            else if (answer.Key == ConsoleKey.Y)
                            {
                                Console.Write("\n");
                                break;
                            }
                            else
                            {
                                Console.Write("\nPlease select a valid option (Y/N)!");
                            }
                        }

                        IDataset outputDataset = (IDataset)featureWorkspace.OpenFeatureClass(outputFeatureClassName);
                        if (outputDataset.CanDelete())
                        {
                            outputDataset.Delete();
                        }
                        else
                        {
                            Console.WriteLine("FeatureClass '{0}' couldn't be deleted", outputFeatureClassName);
                            Console.WriteLine("Press any key to continue ...");
                            Console.ReadKey();
                            return;
                        }
                    }

                    IFeatureClass featureClassPolygons = Program.CreateFeatureClassOutput(workspace, Helper.GetSpatialReference(featureClassPoints), outputFeatureClassName);

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

                    int idxId = featureClassPolygons.FindField(Program.nameFieldIdOutput);
                    int i = 0;
                    StringBuilder errorGeneratePolygon = new StringBuilder();
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
                        catch (Exception ex)
                        {
                            errorGeneratePolygon.AppendLine(string.Format("Polygon id: {0} - Error: Message[{1}]\r\nSource[{2}]\r\nTrace[{3}]", i, ex.Message, ex.Source, ex.StackTrace));
                            errorGeneratePolygon.AppendLine();
                        }
                    }

                    string errorResult = errorGeneratePolygon.ToString();
                    if (!string.IsNullOrEmpty(errorResult))
                    {
                        Console.WriteLine(errorResult);
                        Console.WriteLine("Create polygons voronoi with errors!");
                    }
                    else
                    {
                        Console.WriteLine("Create polygons voronoi with success!");
                    }

                    Console.WriteLine("Press any key to continue ...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine(options.GetUsageError());
                    Console.WriteLine("Press any key to continue ...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Message[{0}]\r\nSource[{1}]\r\nTrace[{2}]", ex.Message, ex.Source, ex.StackTrace);
                Console.WriteLine("Press any key to continue ...");
                Console.ReadKey();
            }
            finally
            {
                try
                {
                    // ESRI License Initializer generated code.
                    // Do not make any call to ArcObjects after ShutDownApplication()
                    arcobjectsLicenseInitializer.ShutdownApplication();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// create feature class of output
        /// </summary>
        /// <param name="workspace">object workspace</param>
        /// <param name="spatialReference">spatial reference of feature class of output</param>
        /// <param name="nameFeatureClass">name of feature class</param>
        /// <returns>object feature class</returns>
        private static IFeatureClass CreateFeatureClassOutput(IWorkspace workspace, ISpatialReference spatialReference, string nameFeatureClass)
        {
            IFeatureClassDescription featureClassDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription objectClassDescription = (IObjectClassDescription)featureClassDescription;

            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

            // Create the fields collection.
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            IField oidField = new FieldClass();
            IFieldEdit oidFieldEdit = (IFieldEdit)oidField;
            oidFieldEdit.Name_2 = "OBJECTID";
            oidFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            fieldsEdit.AddField(oidField);

            // Create the Shape field.
            IField shapeField = new Field();
            IFieldEdit shapeFieldEdit = (IFieldEdit)shapeField;

            // Set up the geometry definition for the Shape field.
            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;

            // By setting the grid size to 0, you're allowing ArcGIS to determine the appropriate grid sizes for the feature class. 
            // If in a personal geodatabase, the grid size will be 1000. If in a file or ArcSDE geodatabase, the grid size
            // will be based on the initial loading or inserting of features.
            geometryDefEdit.HasM_2 = false;
            geometryDefEdit.HasZ_2 = false;

            geometryDefEdit.SpatialReference_2 = spatialReference;

            // Set standard field properties.
            shapeFieldEdit.Name_2 = featureClassDescription.ShapeFieldName;
            shapeFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shapeFieldEdit.GeometryDef_2 = geometryDef;
            shapeFieldEdit.IsNullable_2 = true;
            shapeFieldEdit.Required_2 = true;
            fieldsEdit.AddField(shapeField);

            IField idField = new FieldClass();
            IFieldEdit idIsolaFieldEdit = (IFieldEdit)idField;
            idIsolaFieldEdit.Name_2 = Program.nameFieldIdOutput;
            idIsolaFieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField(idField);

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;
            fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

            return featureWorkspace.CreateFeatureClass(nameFeatureClass, fields, objectClassDescription.InstanceCLSID, objectClassDescription.ClassExtensionCLSID, esriFeatureType.esriFTSimple, featureClassDescription.ShapeFieldName, string.Empty);
        }
    }
}
