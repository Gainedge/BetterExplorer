using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls
{
    public class BreadcrumbExpander : DropDownList
    {
        #region Constructor

        static BreadcrumbExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbExpander),
                new FrameworkPropertyMetadata(typeof(BreadcrumbExpander)));
        }

        #endregion

        #region Methods


        #endregion

        #region Data

        #endregion

        #region Public Properties

        //public static readonly DependencyProperty BreadcrumbTreeProperty =
        //        DependencyProperty.Register("BreadcrumbTree", typeof(BreadcrumbTree), typeof(BreadcrumbTree));

        //public BreadcrumbTree BreadcrumbTree
        //{
        //    get { return (BreadcrumbTree)GetValue(BreadcrumbTreeProperty); }
        //    set { SetValue(BreadcrumbTreeProperty, value); }
        //}

        #endregion
    }
}
