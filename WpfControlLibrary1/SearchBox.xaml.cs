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

namespace BetterExplorerControls
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public SearchBox()
        {
            InitializeComponent();
            ShowFilterMenu();
        }

        public static readonly RoutedEvent BeginSearchEvent = EventManager.RegisterRoutedEvent(
        "BeginSearch", RoutingStrategy.Direct, typeof(SearchEventHandler), typeof(SearchBox));

        // Provide CLR accessors for the event

        public delegate void SearchEventHandler(object sender, SearchRoutedEventArgs e);

        public event SearchEventHandler BeginSearch
        {
            add { AddHandler(BeginSearchEvent, value); }
            remove { RemoveHandler(BeginSearchEvent, value); }
        }

        // This method raises the Tap event
        void RaiseBeginSearchEvent()
        {
            SearchRoutedEventArgs newEventArgs = new SearchRoutedEventArgs(CompileTerms(), SearchBox.BeginSearchEvent);
            RaiseEvent(newEventArgs);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event SearchEventHandler RequestCriteriaChange;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnCriteriaChangeRequested(SearchRoutedEventArgs e)
        {
            if (RequestCriteriaChange != null)
                RequestCriteriaChange(this, e);
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event EventHandler FiltersCleared;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnFiltersCleared(EventArgs e)
        {
            if (FiltersCleared != null)
                FiltersCleared(this, e);
        }

        public string FullSearchTerms
        {
            get
            {
                return CompileTerms();
            }
        }

        private string CompileTerms()
        {
            string full = "";

            full += TextBoxSearchTerms;
            if (ksc != ":null:")
            {
                full += " " + KindCondition;
            }
            if (useesc == true)
            {
                full += " " + esc;
            }
            if (usessc == true)
            {
                full += " " + ssc;
            }
            if (useasc == true)
            {
                full += " " + asc;
            }
            if (usedsc == true)
            {
                full += " " + dsc;
            }
            if (usemsc == true)
            {
                full += " " + msc;
            }
            if (useusc == true)
            {
                full += " " + usc;
            }

            return full;
        }

        public bool FiltersMenuShown
        {
            get
            {
                if (this.SFilters.Visibility == System.Windows.Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private string cfs = "Clear All Filters";

        public string ClearFiltersTitle
        {
            get
            {
                return cfs;
            }
            set
            {
                cfs = value;
            }
        }

        public void SetUpFiltersMenu()
        {
            SFilters.Items.Clear();

            Fluent.MenuItem cfd = new Fluent.MenuItem();
            cfd.Header = cfs;
            cfd.Click += new RoutedEventHandler(cfd_Click);
            SFilters.Items.Add(cfd);
            
            SFilters.Items.Add(new Separator());

            if (useesc == true)
            {
                Fluent.MenuItem a = new Fluent.MenuItem();
                a.Header = esc;
                a.Click += new RoutedEventHandler(a_Click);
                SFilters.Items.Add(a);
            }
            if (usessc == true)
            {
                Fluent.MenuItem a = new Fluent.MenuItem();
                a.Header = ssc;
                a.Click += new RoutedEventHandler(a_Click);
                SFilters.Items.Add(a);
            }
            if (useasc == true)
            {
                Fluent.MenuItem a = new Fluent.MenuItem();
                a.Header = asc;
                a.Click += new RoutedEventHandler(a_Click);
                SFilters.Items.Add(a);
            }
            if (usedsc == true)
            {
                Fluent.MenuItem a = new Fluent.MenuItem();
                a.Header = dsc;
                a.Click += new RoutedEventHandler(a_Click);
                SFilters.Items.Add(a);
            }
            if (usemsc == true)
            {
                Fluent.MenuItem a = new Fluent.MenuItem();
                a.Header = msc;
                a.Click += new RoutedEventHandler(a_Click);
                SFilters.Items.Add(a);
            }
            if (useusc == true)
            {
                Fluent.MenuItem a = new Fluent.MenuItem();
                a.Header = usc;
                a.Click += new RoutedEventHandler(a_Click);
                SFilters.Items.Add(a);
            }
        }

        void cfd_Click(object sender, RoutedEventArgs e)
        {
            ssc = "size:";
            asc = "author:";
            esc = "ext:";
            dsc = "date:";
            msc = "modified:";
            usc = "subject:";

            useasc = false;
            usedsc = false;
            useesc = false;
            usemsc = false;
            usessc = false;
            useusc = false;

            OnFiltersCleared(EventArgs.Empty);
            SetUpFiltersMenu();
            ShowFilterMenu();
        }

        void a_Click(object sender, RoutedEventArgs e)
        {
            OnCriteriaChangeRequested(new SearchRoutedEventArgs((string)((Fluent.MenuItem)sender).Header, e.RoutedEvent));
        }

        private bool autouse = true;
        public bool AutomaticallySetUseValues
        {
            get
            {
                return autouse;
            }
            set
            {
                autouse = value;
            }
        }

        private void ShowFilterMenu()
        {
            if (useasc == false && usessc == false && useesc == false && useusc == false && usemsc == false && usedsc == false)
            {
                this.SFilters.Visibility = System.Windows.Visibility.Collapsed;
                this.SearchCriteriatext.Margin = new Thickness(0, 0, 24, 0);
            }
            else
            {
                this.SFilters.Visibility = System.Windows.Visibility.Visible;
                this.SearchCriteriatext.Margin = new Thickness(0, 0, 54, 0);
            }
        }

        private string ksc = ":null:";

        public string KindCondition
        {
            get
            {
                return ksc;
            }
            set
            {
                ksc = value;
            }
        }

        private string esc = "ext:";
        private bool useesc = false;

        public string ExtensionCondition
        {
            get
            {
                return esc;
            }
            set
            {
                esc = value;
                if (autouse == true)
                {
                    if (esc.Length > 4)
                    {
                        useesc = true;
                    }
                    else
                    {
                        useesc = false;
                    }
                    ShowFilterMenu();
                }
            }
        }

        public bool UseExtensionCondition
        {
            get
            {
                return useesc;
            }
            set
            {
                useesc = value;
                ShowFilterMenu();
            }
        }

        private string ssc = "size:";
        private bool usessc = false;

        public string SizeCondition
        {
            get
            {
                return ssc;
            }
            set
            {
                ssc = value;
                if (autouse == true)
                {
                    if (ssc.Length > 5)
                    {
                        usessc = true;
                    }
                    else
                    {
                        usessc = false;
                    }
                    ShowFilterMenu();
                }
            }
        }

        public bool UseSizeCondition
        {
            get
            {
                return usessc;
            }
            set
            {
                usessc = value;
                ShowFilterMenu();
            }
        }

        private string asc = "author:";
        private bool useasc = false;

        public string AuthorCondition
        {
            get
            {
                return asc;
            }
            set
            {
                asc = value;
                if (autouse == true)
                {
                    if (asc.Length > 7)
                    {
                        useasc = true;
                    }
                    else
                    {
                        useasc = false;
                    }
                    ShowFilterMenu();
                }
            }
        }

        public bool UseAuthorCondition
        {
            get
            {
                return useasc;
            }
            set
            {
                useasc = value;
                ShowFilterMenu();
            }
        }

        private string dsc = "date:";
        private bool usedsc = false;

        public string DateCondition
        {
            get
            {
                return dsc;
            }
            set
            {
                dsc = value;
                if (autouse == true)
                {
                    if (dsc.Length > 5)
                    {
                        usedsc = true;
                    }
                    else
                    {
                        usedsc = false;
                    }
                    ShowFilterMenu();
                }
            }
        }

        public bool UseDateCondition
        {
            get
            {
                return usedsc;
            }
            set
            {
                usedsc = value;
                ShowFilterMenu();
            }
        }

        private string msc = "modified:";
        private bool usemsc = false;

        public string ModifiedCondition
        {
            get
            {
                return msc;
            }
            set
            {
                msc = value;
                if (autouse == true)
                {
                    if (msc.Length > 9)
                    {
                        usemsc = true;
                    }
                    else
                    {
                        usemsc = false;
                    }
                    ShowFilterMenu();
                }
            }
        }

        public bool UseModifiedCondition
        {
            get
            {
                return usemsc;
            }
            set
            {
                usemsc = value;
                ShowFilterMenu();
            }
        }

        private string usc = "subject:";
        private bool useusc = false;

        public string SubjectCondition
        {
            get
            {
                return usc;
            }
            set
            {
                usc = value;
                if (autouse == true)
                {
                    if (usc.Length > 8)
                    {
                        useusc = true;
                    }
                    else
                    {
                        useusc = false;
                    }
                    ShowFilterMenu();
                }
            }
        }

        public bool UseSubjectCondition
        {
            get
            {
                return useusc;
            }
            set
            {
                useusc = value;
                ShowFilterMenu();
            }
        }

        public string TextBoxSearchTerms
        {
            get
            {
                return SearchCriteriatext.Text;
            }
        }

        private void textBlock1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lblDefault.Visibility = System.Windows.Visibility.Hidden;
            this.Focus();
            SearchCriteriatext.IsEnabled = true;
            SearchCriteriatext.Focus();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchCriteriatext.Text.Length == 0)
            {
                lblDefault.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lblDefault.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void textBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchCriteriatext.Text.Length == 0)
            {
                lblDefault.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void textBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusManager.SetIsFocusScope(this, true);
        }

        private void textBox1_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (SearchCriteriatext.Text.Length == 0)
            {
                lblDefault.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void SStartEnd_Click(object sender, RoutedEventArgs e)
        {
            RaiseBeginSearchEvent();
        }

        private void SFilters_DropDownOpened(object sender, EventArgs e)
        {
            SetUpFiltersMenu();
        }
    }
}

public class SearchRoutedEventArgs : RoutedEventArgs
{
    private string st;

    public SearchRoutedEventArgs(string terms)
    {
        st = terms;
    }

    public SearchRoutedEventArgs(RoutedEvent routedevent)
    {
        st = "";
        base.RoutedEvent = routedevent;
    }

    public SearchRoutedEventArgs(string terms, RoutedEvent routedevent)
    {
        st = terms;
        base.RoutedEvent = routedevent;
    }

    public SearchRoutedEventArgs(string terms, RoutedEvent routedevent, string source)
    {
        st = terms;
        base.RoutedEvent = routedevent;
        base.Source = source;
    }

    public string SearchTerms
    {
        get
        {
            return st;
        }
        set
        {
            st = value;
        }
    }

}
