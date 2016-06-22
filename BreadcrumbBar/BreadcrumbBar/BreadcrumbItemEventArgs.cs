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
    public class BreadcrumbItemEventArgs:RoutedEventArgs
    {
        public BreadcrumbItemEventArgs(BreadcrumbItem item, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Item = item;
        }

        public BreadcrumbItem Item { get; private set; }
    }

    public delegate void BreadcrumbItemEventHandler(object sender, BreadcrumbItemEventArgs e);

}
