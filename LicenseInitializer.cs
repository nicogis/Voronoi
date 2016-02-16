//-----------------------------------------------------------------------
// <copyright file="LicenseInitializer.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGIS.Voronoi
{
    using System;
    using ESRI.ArcGIS;

    /// <summary>
    /// class LicenseInitializer
    /// </summary>
    internal partial class LicenseInitializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseInitializer"/> class
        /// </summary>
        public LicenseInitializer()
        {
            this.ResolveBindingEvent += new EventHandler(this.BindingArcGISRuntime);
        }

        /// <summary>
        /// Binding ArcGISRuntime
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">object EventArgs</param>
        private void BindingArcGISRuntime(object sender, EventArgs e)
        {
            // TODO: Modify ArcGIS runtime binding code as needed
            if (!RuntimeManager.Bind(ProductCode.Engine))
            {
                // Failed to bind, announce and force exit
                Console.WriteLine("Invalid ArcGIS runtime binding. Application will shut down.");
                System.Environment.Exit(0);
            }
        }
    }
}