using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
//###################################################################################
// Odyssey.Controls
// (c) Copyright 2008 Thomas Gerber
// All rights reserved.
//
//  THERE IS NO WARRANTY FOR THE PROGRAM, TO THE EXTENT PERMITTED BY
// APPLICABLE LAW.  EXCEPT WHEN OTHERWISE STATED IN WRITING THE COPYRIGHT
// HOLDERS AND/OR OTHER PARTIES PROVIDE THE PROGRAM "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE.  THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE OF THE PROGRAM
// IS WITH YOU.  SHOULD THE PROGRAM PROVE DEFECTIVE, YOU ASSUME THE COST OF
// ALL NECESSARY SERVICING, REPAIR OR CORRECTION.
//###################################################################################


namespace Odyssey.Controls
{
    /// <summary>
    /// RoutedEventArgs to convert the display path to edit path and vice verca.
    /// </summary>
    public class PathConversionEventArgs:RoutedEventArgs
    {
        /// <summary>
        /// Specifies what property to convert.
        /// </summary>
        public enum ConversionMode
        {
            /// <summary>
            /// Convert the display path to edit path.
            /// </summary>
            DisplayToEdit,

            /// <summary>
            /// convert the edit path to display path.
            /// </summary>
            EditToDisplay,
        }

        /// <summary>
        /// Gets or sets the display path.
        /// </summary>
        public string DisplayPath { get; set; }

        /// <summary>
        /// Gets or sets the edit path.
        /// </summary>
        public string EditPath { get; set; }

        /// <summary>
        /// Specifies what path property to convert.
        /// </summary>
        public ConversionMode Mode { get; private set; }

        /// <summary>
        /// Gets the root object of the breadcrumb bar.
        /// </summary>
        public object Root { get; private set; }

        /// <summary>
        /// Creates a new PathConversionEventArgs class.
        /// </summary>
        /// <param name="mode">The conversion mode.</param>
        /// <param name="path">The initial values for DisplayPath and EditPath.</param>
        /// <param name="root">The root object.</param>
        /// <param name="routedEvent"></param>
        public PathConversionEventArgs(ConversionMode mode, string path, object root, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Mode = mode;
            DisplayPath = EditPath = path;
            Root = root;
        }
    }

    public delegate void PathConversionEventHandler(object sender, PathConversionEventArgs e);

}
