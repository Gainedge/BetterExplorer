using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Shell;
using Fluent;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;

namespace BetterExplorer
{
    /// <summary>
    /// ========================================
    /// .NET Framework 4.0 Custom Control
    /// ========================================
    ///
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CloseableTabItemDemo"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CloseableTabItemDemo;assembly=CloseableTabItemDemo"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file. Note that Intellisense in the
    /// XML editor does not currently work on custom controls and its child elements.
    ///
    ///     <MyNamespace:ClosableTabItem/>
    ///
    /// </summary>
    public class ClosableTabItem : TabItem
    {
        static ClosableTabItem()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosableTabItem),
                new FrameworkPropertyMetadata(typeof(ClosableTabItem)));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source == this || !this.IsSelected)
                return;

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e.Source == this || !this.IsSelected)
            {
                //base.OnMouseLeftButtonDown(e); // OR just this.Focus(); OR this.IsSeleded = true;
                this.RaiseEvent(new RoutedEventArgs(TabSelectedEvent, this));
                this.IsSelected = true;
            }

            base.OnMouseLeftButtonUp(e);
        }

        public static readonly RoutedEvent CloseTabEvent =
            EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ClosableTabItem));

        public static readonly RoutedEvent TabSelectedEvent =
            EventManager.RegisterRoutedEvent("TabSelected", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ClosableTabItem));

        public event RoutedEventHandler CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }

        public event RoutedEventHandler TabSelected
        {
            add { AddHandler(TabSelectedEvent, value); }
            remove { RemoveHandler(TabSelectedEvent, value); }
        }

        public static readonly DependencyProperty IconProperty =
                                DependencyProperty.Register("TabIcon", typeof(ImageSource), typeof(ClosableTabItem),
                                new FrameworkPropertyMetadata(null,
                                      FrameworkPropertyMetadataOptions.AffectsRender |
                                      FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>The Content.</value>
        public ImageSource TabIcon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        int _Index = -1;
        public int Index
        {
            get
            {
                return _Index;
            }
            set
            {
                _Index = value;
            }
        }

        ShellObject _Path;
        public ShellObject Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                this.PathText = _Path.ParsingName;
            }
        }

        public string PathText
        {
          get;
          set;
        }
        bool _IsNavigate = true;
        public bool IsNavigate
        {
            get
            {
                return _IsNavigate;
            }
            set
            {
                _IsNavigate = value;
            }
        }

        public String[] SelectedItems { get; set; }
        public Fluent.ContextMenu mnu = null;
        private System.Windows.Controls.Button CloseButton;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            System.Windows.Controls.Button closeButton = base.GetTemplateChild("PART_Close") as System.Windows.Controls.Button;
            if (closeButton != null)
            {
                CloseButton = closeButton;
                closeButton.PreviewMouseLeftButtonDown += closeButton_PreviewMouseDown; // Click += new System.Windows.RoutedEventHandler(closeButton_Click);
            }
            this.MouseRightButtonUp += new MouseButtonEventHandler(CloseableTabItem_MouseRightButtonUp);
        }

        void closeButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
        }



        void CloseableTabItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mnu != null)
            {
                mnu.Placement = PlacementMode.Bottom;
                mnu.PlacementTarget = this;
                mnu.IsOpen = true;
            }
            
        }


        /// <summary>
        /// Contains information about the current location, as well as previously opened locations.
        /// </summary>
        public NavigationLog log = new NavigationLog();
    }

    /// <summary>
    /// Takes care of the looks department :)
    /// </summary>
    public class EyeCandy
    {
        #region Image dependency property

        /// <summary>
        /// An attached dependency property which provides an
        /// <see cref="ImageSource" /> for arbitrary WPF elements.
        /// </summary>
        public static readonly DependencyProperty ImageProperty;

        /// <summary>
        /// Gets the <see cref="ImageProperty"/> for a given
        /// <see cref="DependencyObject"/>, which provides an
        /// <see cref="ImageSource" /> for arbitrary WPF elements.
        /// </summary>
        public static ImageSource GetImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageProperty);
        }

        /// <summary>
        /// Sets the attached <see cref="ImageProperty"/> for a given
        /// <see cref="DependencyObject"/>, which provides an
        /// <see cref="ImageSource" /> for arbitrary WPF elements.
        /// </summary>
        public static void SetImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageProperty, value);
        }

        #endregion

        static EyeCandy()
        {
            //register attached dependency property
            var metadata = new FrameworkPropertyMetadata((ImageSource)null);
            ImageProperty = DependencyProperty.RegisterAttached("Image",
                                                                typeof(ImageSource),
                                                                typeof(EyeCandy), metadata);
        }
    }
}
