using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.WindowsAPICodePack.Shell;
using Fluent;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Threading;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for BreadcrumbBarItem.xaml
    /// </summary>
    public partial class BreadcrumbBarItem : UserControl
    {
        string path = "";
        Fluent.ContextMenu DropDownMenu = new Fluent.ContextMenu();
        bool children = true;
        ShellObject so;

        public bool HasDropDownMenu
        {
            get
            {
                return children;
            }
        }

        public ShellObject ShellObject
        {
            get
            {
                return so;
            }
            set
            {
                LoadDirectory(value);
            }
        }

        public BreadcrumbBarItem()
        {
            InitializeComponent();
            DropDownMenu.Opened += new RoutedEventHandler(DropDownMenu_Opened);
            DropDownMenu.Closed += new RoutedEventHandler(DropDownMenu_Closed);
        }

        void DropDownMenu_Closed(object sender, RoutedEventArgs e)
        {
            OnMenuClosed(EventArgs.Empty);
            expandArrow.Visibility = System.Windows.Visibility.Visible;
            ddArrow.Visibility = System.Windows.Visibility.Hidden;
            HideSelectedColors();
        }

        void DropDownMenu_Opened(object sender, RoutedEventArgs e)
        {
            OnMenuOpened(EventArgs.Empty);
            expandArrow.Visibility = System.Windows.Visibility.Hidden;
            ddArrow.Visibility = System.Windows.Visibility.Visible;
            ShowSelectedColors();
        }

        public void LoadDirectory(ShellObject obj)
        {
            obj.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
            obj.Thumbnail.CurrentSize = new Size(16, 16);
            this.PathImage.Source = obj.Thumbnail.BitmapSource;
            this.pathName.Text = obj.GetDisplayName(DisplayNameType.Default);
            this.so = obj;
            path = obj.ParsingName;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)(() =>
            {




                if (obj.ParsingName == KnownFolders.Network.ParsingName || obj.ParsingName.StartsWith(@"\\"))
                {
                    SetChildren(true);
                    grid1.Visibility = System.Windows.Visibility.Visible;
                    MenuBorder.Visibility = System.Windows.Visibility.Visible;

                }
                else
                {
                    try
                    {
                        ShellContainer con = (ShellContainer)obj;
                        List<ShellObject> joe = new List<ShellObject>();
                        foreach (ShellObject item in con)
                        {
                            if (item.IsFolder == true)
                            {
                                if (item.ParsingName.ToLower().EndsWith(".zip") == false && item.ParsingName.ToLower().EndsWith(".cab") == false)
                                {
                                    joe.Add(item);
                                }
                            }
                        }
                        SetChildren(joe.Count > 0);
                    }
                    catch
                    {
                        SetChildren(false);
                    }
                    
                }
            }));

        }

        public void SetChildren(bool isON)
        {
            expandArrow.Visibility = isON ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            grid1.Visibility = isON ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            children = isON;
        }

        public delegate void PathEventHandler(object sender, PathEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event PathEventHandler NavigateRequested;
        //public event EventHandler MouseDoubleClick;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnNavigateRequested(PathEventArgs e)
        {
            if (NavigateRequested != null)
                NavigateRequested(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler MenuOpened;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnMenuOpened(EventArgs e)
        {
            if (MenuOpened != null)
                MenuOpened(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler MenuClosed;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnMenuClosed(EventArgs e)
        {
            if (MenuClosed != null)
                MenuClosed(this, e);
        }
        private string IncludeTrailingBackslash(string Path)
        {
            string CharToInsert = "";
            if (Path[Path.Length - 1] != Char.Parse(@"\"))
            {
                CharToInsert = @"\";
            }
            return Path + CharToInsert;
        }

        private void MenuItemClicked(object sender, RoutedEventArgs e)
        {
            Fluent.MenuItem pan = (Fluent.MenuItem)sender;
            OnNavigateRequested(new PathEventArgs(pan.Tag as ShellObject));
        }

        private void ShowSelectedColors()
        {
            this.SelectionBackground.Visibility = System.Windows.Visibility.Visible;
            this.PathBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 127, 177));

            if (HasDropDownMenu == true)
            {
                this.MenuBackground.Visibility = System.Windows.Visibility.Visible;
                this.MenuBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 127, 177));
            }
        }

        private void HideSelectedColors()
        {
            this.SelectionBackground.Visibility = System.Windows.Visibility.Hidden;
            this.PathBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(1, 60, 127, 177));

            if (HasDropDownMenu == true)
            {
                this.MenuBackground.Visibility = System.Windows.Visibility.Hidden;
                this.MenuBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(1, 60, 127, 177));
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {

            ShowSelectedColors();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            HideSelectedColors();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ShellContainer con = (ShellContainer)this.ShellObject;
            List<ShellObject> joe = new List<ShellObject>();
            foreach (ShellObject item in con)
            {
                if (item.IsFolder)
                {
                    if (!item.ParsingName.EndsWith(".zip"))
                    {
                        joe.Add(item);
                    }
                }
            }

            joe.Sort(delegate(ShellObject j1, ShellObject j2) { return j1.GetDisplayName(DisplayNameType.Default).CompareTo(j2.GetDisplayName(DisplayNameType.Default)); });
            DropDownMenu.Items.Clear();
            foreach (ShellObject thing in joe)
            {
                Fluent.MenuItem pan = new Fluent.MenuItem();
                thing.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                thing.Thumbnail.CurrentSize = new Size(16, 16);
                pan.Icon = thing.Thumbnail.BitmapSource;
                pan.Header = thing.GetDisplayName(DisplayNameType.Default);
                pan.Height = 23;
                pan.Tag = thing;
                pan.Click += new RoutedEventHandler(MenuItemClicked);
                this.DropDownMenu.Items.Add(pan);
            }
            if (DropDownMenu.Items.Count > 0)
            {
                DropDownMenu.Placement = PlacementMode.Bottom;
                DropDownMenu.PlacementTarget = Base;
                DropDownMenu.IsOpen = true;
            }

        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            OnNavigateRequested(new PathEventArgs(this.ShellObject));
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }



        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
            //e.Handled = true;
            //if (e.ClickCount == 2)
            //{
            //    if (MouseDoubleClick != null)
            //    {
            //        MouseDoubleClick(this, EventArgs.Empty);
            //    }
                
            //}
            
        }

        private void btnDropDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void btnDropDown_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }



    }
}

public class PathEventArgs
{
    string _path;
    ShellObject _obj;

    public PathEventArgs(ShellObject obj = null)
    {
        _obj = obj;
        _path = obj.ParsingName;
    }

    public string Path
    {
        get
        {
            return _path;
        }
    }

    public ShellObject ShellObject
    {
        get
        {
            return _obj;
        }
    }

}

