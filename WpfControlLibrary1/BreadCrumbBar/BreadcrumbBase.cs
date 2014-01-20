using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BetterExplorerControls
{
    public class BreadcrumbBase : ItemsControl
    {
        #region Constructor

        static BreadcrumbBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBase),
                new FrameworkPropertyMetadata(typeof(BreadcrumbBase)));
        }
        
        #endregion

        #region Methods

        public virtual void Select(object value)
        {
            SelectedValue = value;
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            bcore = this.Template.FindName("PART_BreadcrumbCore", this) as BreadcrumbCore;
            tbox = this.Template.FindName("PART_TextBox", this) as SuggestBoxBase;
            toggle = this.Template.FindName("PART_Toggle", this) as ToggleButton;

            #region BreadcrumbCore related handlers
            //When Breadcrumb select a value, update it.
            AddHandler(BreadcrumbCore.SelectedValueChangedEvent, (RoutedEventHandler)((o, e) =>
            {
                Select(bcore.SelectedValue);
            }));
            #endregion

            #region SuggestBox related handlers.
            //When click empty space, switch to text box
            AddHandler(Breadcrumb.MouseDownEvent, (RoutedEventHandler)((o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, false); //Hide Breadcrumb
            }));

            //When text box is visible, call SelectAll
            toggle.AddValueChanged(ToggleButton.IsCheckedProperty,
                (o, e) =>
                {
                    tbox.Focus();
                    tbox.SelectAll();
                });

           

            //When changed selected (path) value, hide textbox.
            AddHandler(SuggestBox.ValueChangedEvent, (RoutedEventHandler)((o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, true); //Show Breadcrumb
            }));
            this.AddValueChanged(Breadcrumb.SelectedPathValueProperty, (o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, true); //Show Breadcrumb
            });
            this.AddValueChanged(Breadcrumb.SelectedValueProperty, (o, e) =>
            {
                toggle.SetValue(ToggleButton.IsCheckedProperty, true); //Show Breadcrumb
            });

            #endregion
           
        }

        public static void OnSelectedValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as BreadcrumbBase;
            if (bread.bcore != null && (e.NewValue == null || !e.NewValue.Equals(e.OldValue)))
                bread.Select(e.NewValue);
        }



        
        #endregion

        #region Data

        protected BreadcrumbCore bcore;
        protected SuggestBoxBase tbox;
        protected ToggleButton toggle;
        
        #endregion

        #region Public Properties

        public SuggestBoxBase PART_SuggestBox { get { return tbox; } }
        public BreadcrumbCore PART_BreadcrumbCore { get { return bcore; } }
        public ToggleButton PART_Toggle { get { return toggle; } }

        /// <summary>
        /// Selected value object, it's path is retrieved from HierarchyHelper.GetPath(), not bindable at this time
        /// </summary>
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object),
            typeof(BreadcrumbBase), new UIPropertyMetadata(null, OnSelectedValueChanged));


        #region ProgressBar related - IsIndeterminate, IsProgressbarVisible, ProgressBarValue
        /// <summary>
        /// Toggle whether the progress bar is indertminate
        /// </summary>
        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool),
            typeof(BreadcrumbBase), new UIPropertyMetadata(true));

        /// <summary>
        /// Toggle whether Progressbar visible
        /// </summary>
        public bool IsProgressbarVisible
        {
            get { return (bool)GetValue(IsProgressbarVisibleProperty); }
            set { SetValue(IsProgressbarVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsProgressbarVisibleProperty =
            DependencyProperty.Register("IsProgressbarVisible", typeof(bool),
            typeof(BreadcrumbBase), new UIPropertyMetadata(false));

        /// <summary>
        /// Value of Progressbar.
        /// </summary>
        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(int),
            typeof(BreadcrumbBase), new UIPropertyMetadata(0));

        #endregion

        #region IsBreadcrumbVisible, DropDownHeight, DropDownWidth

        /// <summary>
        /// Toggle whether Breadcrumb (or SuggestBox) visible
        /// </summary>
        public bool IsBreadcrumbVisible
        {
            get { return (bool)GetValue(IsBreadcrumbVisibleProperty); }
            set { SetValue(IsBreadcrumbVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsBreadcrumbVisibleProperty =
            DependencyProperty.Register("IsBreadcrumbVisible", typeof(bool),
            typeof(BreadcrumbBase), new UIPropertyMetadata(true));

        public static readonly DependencyProperty DropDownHeightProperty =
              BreadcrumbCore.DropDownHeightProperty.AddOwner(typeof(BreadcrumbBase));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </summary>
        public double DropDownHeight
        {
            get { return (double)GetValue(DropDownHeightProperty); }
            set { SetValue(DropDownHeightProperty, value); }
        }

        public static readonly DependencyProperty DropDownWidthProperty =
            BreadcrumbCore.DropDownWidthProperty.AddOwner(typeof(BreadcrumbBase));

        /// <summary>
        /// Is current dropdown (combobox) opened, this apply to the first &lt;&lt; button only
        /// </summary>
        public double DropDownWidth
        {
            get { return (double)GetValue(DropDownWidthProperty); }
            set { SetValue(DropDownWidthProperty, value); }
        }

        #endregion

        #region Header/Icon Template
        public static readonly DependencyProperty HeaderTemplateProperty =
                    BreadcrumbCore.HeaderTemplateProperty.AddOwner(typeof(BreadcrumbBase));

        /// <summary>
        /// DataTemplate define the header text, (see also IconTemplate)
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly DependencyProperty IconTemplateProperty =
           DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(BreadcrumbBase), new PropertyMetadata(null));

        /// <summary>
        /// DataTemplate define the icon.
        /// </summary>
        public DataTemplate IconTemplate
        {
            get { return (DataTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }
        
        #endregion

        public static readonly DependencyProperty RootItemsSourceProperty =
         BreadcrumbCore.RootItemsSourceProperty.AddOwner(typeof(BreadcrumbBase));

        /// <summary>
        /// RootItemsSource - Items to be shown in BreadcrumbCore.
        /// ItemsSource - The Hierarchy for of current selected item.
        /// </summary>
        public IEnumerable RootItemsSource
        {
            get { return (IEnumerable)GetValue(RootItemsSourceProperty); }
            set { SetValue(RootItemsSourceProperty, value); }
        }


        public static readonly DependencyProperty ValuePathProperty =
           DependencyProperty.Register("ValuePath", typeof(string), typeof(BreadcrumbBase), 
           new PropertyMetadata("Value"));

        /// <summary>
        /// Used by suggest box to obtain value
        /// </summary>
        public string ValuePath
        {
            get { return (string)GetValue(ValuePathProperty); }
            set { SetValue(ValuePathProperty, value); }
        }

        public static readonly DependencyProperty SuggestionsProperty =
            SuggestBox.SuggestionsProperty.AddOwner(typeof(BreadcrumbBase));

        /// <summary>
        /// Suggestions shown on the SuggestionBox
        /// </summary>
        public IList<object> Suggestions
        {
            get { return (IList<object>)GetValue(SuggestionsProperty); }
            set { SetValue(SuggestionsProperty, value); }
        }


        public static readonly DependencyProperty TextProperty =
        SuggestBox.TextProperty.AddOwner(typeof(BreadcrumbBase));

        /// <summary>
        /// Text shown on the SuggestionBox
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }





        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(object), typeof(BreadcrumbBase));

        /// <summary>
        /// Buttons shown in the right side of the Breadcrumb
        /// </summary>
        public object Buttons
        {
            get { return GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }


        #endregion
    }
}
