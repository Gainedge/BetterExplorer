using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BetterExplorerControls
{
    public class BreadcrumbOverflowPanel : ItemsControl
    {
        #region Constructor

        static BreadcrumbOverflowPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbOverflowPanel),
             new FrameworkPropertyMetadata(typeof(BreadcrumbOverflowPanel)));
        }

        #endregion

        #region Methods

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbItem(false) { HeaderTemplate = this.HeaderTemplate, 
                IconTemplate = this.IconTemplate, 
                ShowToggle = false 
            };
        }

        #endregion

        #region Data



        #endregion

        #region Public Properties

        public static readonly DependencyProperty HeaderTemplateProperty =
           DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(BreadcrumbOverflowPanel), new PropertyMetadata(null));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty IconTemplateProperty =
           DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(BreadcrumbOverflowPanel), new PropertyMetadata(null));

        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }

        #endregion
    }
}
