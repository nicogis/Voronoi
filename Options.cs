//-----------------------------------------------------------------------
// <copyright file="Options.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGIS.Voronoi
{
        using System.Collections.Generic;
        using CommandLine;
        using CommandLine.Text;

        /// <summary>
        /// class args parameter
        /// </summary>
        internal class Options
        {
            /// <summary>
            /// Gets or sets file
            /// </summary>
            [Option('f', "file", Required = true, MutuallyExclusiveSet = "voronoi", HelpText = @"Path and file geodatabase. For instance 'c:\temp\demo.gdb'")]
            public string PathAndFGDB
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets name of feature class point
            /// </summary>
            [Option('i', "points", Required = false, MutuallyExclusiveSet = "voronoi", HelpText = "Name of feature class points. Default ('Points')")]
            public string FeatureClassNameInput
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets from of distance
            /// </summary>
            [Option('o', "polygons", Required = false, MutuallyExclusiveSet = "voronoi", HelpText = "Name of feature class polygons output. Default ('Polygons')")]
            public string FeatureClassNameOutput
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets LastParserState
            /// </summary>
            [ParserState]
            public IParserState LastParserState
            {
                get;
                set;
            }

            /// <summary>
            /// help of application
            /// </summary>
            /// <returns>string of help</returns>
            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }

            /// <summary>
            /// message of errors
            /// </summary>
            /// <returns>string of message of errors</returns>
            public string GetUsageError()
            {
                var help = new HelpText();
                if (this.LastParserState.Errors.Count > 0)
                {
                    var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces
                    if (!string.IsNullOrEmpty(errors))
                    {
                        help.AddPreOptionsLine(string.Concat("\n", "Error(s):"));
                        help.AddPreOptionsLine(errors);
                    }
                }

                return help;
            }
        }
}
