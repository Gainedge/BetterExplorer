using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BetterExplorerControls
{


    public class Breadcrumb : BreadcrumbBase
    {
        #region Constructor

     

        public Breadcrumb()
        {
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();          

            //Update Suggestions when text changed.
            tbox.AddHandler(TextBox.TextChangedEvent, (RoutedEventHandler)((o, e) =>
                {
                    if (tbox.IsEnabled)
                    {
                        var suggestSource = SuggestSource;
                        var hierarchyHelper = HierarchyHelper;
                        string text = tbox.Text;
                        object data = RootItem;
                        Task.Run(async () =>
                        {
                            return await suggestSource.SuggestAsync(data, text, hierarchyHelper);
                        }).ContinueWith(
                        (pTask) =>
                        {
                            if (!pTask.IsFaulted)
                                this.SetValue(SuggestionsProperty, pTask.Result);
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }));

            this.AddValueChanged(ValuePathProperty, (o, e) =>
                {
                    Breadcrumb.OnHierarchyHelperPropChanged(this, 
                        new DependencyPropertyChangedEventArgs(ValuePathProperty, null, ValuePath));
                });

            this.AddValueChanged(RootItemProperty, OnRootItemChanged);
            OnRootItemChanged(this, EventArgs.Empty);
        }

        public override void Select(object value)
        {
            base.Select(value);
            if (bcore != null && value != null)
            {
                var hierarchy = HierarchyHelper.GetHierarchy(value, true).Reverse().ToList();
                this.SetValue(ItemsSourceProperty, hierarchy);
                SelectedPathValue = HierarchyHelper.GetPath(value);
                bcore.SetValue(BreadcrumbCore.ShowDropDownProperty, SelectedPathValue != "");
            }
        }

        public void OnRootItemChanged(object sender, EventArgs args)
        {
            if (RootItem != null)
            {
                this.Items.Clear();
                this.SetValue(RootItemsSourceProperty, this.HierarchyHelper.List(RootItem));
                //bcore.RootItems = tbox.RootItems = this.HierarchyHelper.List(RootItem);
                //bcore.ShowDropDown = false;
            }
        }

        public static void OnSelectedPathValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            if (bread.bcore != null &&
                (e.NewValue == null || !e.NewValue.Equals(bread.HierarchyHelper.GetPath(bread.SelectedValue))))
                bread.Select(bread.HierarchyHelper.GetItem(bread.RootItem, e.NewValue as string));
        }

       

        public static void OnHierarchyHelperPropChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bread = sender as Breadcrumb;
            //If HierarchyHelper changed, update Parent/Value/Subentries path, vice versa.
            if (!bread._updatingHierarchyHelper)
            {
                bread._updatingHierarchyHelper = true;
                try
                {
                    if (e.Property.Equals(HierarchyHelperProperty))
                    {
                        if (bread.HierarchyHelper.ParentPath != bread.ParentPath)
                            bread.ParentPath = bread.HierarchyHelper.ParentPath;

                        if (bread.HierarchyHelper.ValuePath != bread.ValuePath)
                            bread.ValuePath = bread.HierarchyHelper.ValuePath;

                        if (bread.HierarchyHelper.SubentriesPath != bread.SubentriesPath)
                            bread.SubentriesPath = bread.HierarchyHelper.SubentriesPath;
                    }
                    else
                    {

                        bread.HierarchyHelper = new PathHierarchyHelper(bread.ParentPath, bread.ValuePath, bread.SubentriesPath);
                    }
                }
                finally
                {
                    bread._updatingHierarchyHelper = false;
                }
            }
        }

        #endregion

        #region Data

        bool _updatingHierarchyHelper = false;


        #endregion

        #region Public Properties

        #region SelectedValue, SelectedPathValue

        

        /// <summary>
        /// Path value of the SelectedValue object, bindable.
        /// </summary>
        public string SelectedPathValue
        {
            get { return (string)GetValue(SelectedPathValueProperty); }
            set { SetValue(SelectedPathValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedPathValueProperty =
            DependencyProperty.Register("SelectedPathValue", typeof(string),
            typeof(Breadcrumb), new UIPropertyMetadata(null, OnSelectedPathValueChanged));

        #endregion

        public static readonly DependencyProperty RootItemProperty =
         DependencyProperty.Register("RootItem", typeof(object), typeof(Breadcrumb),
         new PropertyMetadata(null));

        /// <summary>
        /// Root item of the breadcrumbnail
        /// </summary>
        public object RootItem
        {
            get { return (object)GetValue(RootItemProperty); }
            set { SetValue(RootItemProperty, value); }
        }

        #region HierarchyHelper, ParentPath, ValuePath, SubEntriesPath, SuggestSource

        /// <summary>
        /// Uses to navigate the hierarchy, one can also set the ParentPath/ValuePath and SubEntriesPath instead.
        /// </summary>
        public IHierarchyHelper HierarchyHelper
        {
            get { return (IHierarchyHelper)GetValue(HierarchyHelperProperty); }
            set { SetValue(HierarchyHelperProperty, value); }
        }

        public static readonly DependencyProperty HierarchyHelperProperty =
            DependencyProperty.Register("HierarchyHelper", typeof(IHierarchyHelper),
            typeof(Breadcrumb), new PropertyMetadata(new PathHierarchyHelper("Parent", "Value", "SubEntries"), OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access parent.
        /// </summary>
        public string ParentPath
        {
            get { return (string)GetValue(ParentPathProperty); }
            set { SetValue(ParentPathProperty, value); }
        }

        public static readonly DependencyProperty ParentPathProperty =
            DependencyProperty.Register("Parent", typeof(string),
            typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// The path of view model to access sub entries.
        /// </summary>
        public string SubentriesPath
        {
            get { return (string)GetValue(SubentriesPathProperty); }
            set { SetValue(SubentriesPathProperty, value); }
        }

        public static readonly DependencyProperty SubentriesPathProperty =
           DependencyProperty.Register("SubentriesPath", typeof(string),
            typeof(Breadcrumb), new PropertyMetadata(OnHierarchyHelperPropChanged));

        /// <summary>
        /// Uses by SuggestBox to suggest options.
        /// </summary>
        public ISuggestSource SuggestSource
        {
            get { return (ISuggestSource)GetValue(SuggestSourceProperty); }
            set { SetValue(SuggestSourceProperty, value); }
        }

        public static readonly DependencyProperty SuggestSourceProperty =
            DependencyProperty.Register("SuggestSource", typeof(ISuggestSource),
            typeof(Breadcrumb), new UIPropertyMetadata(new AutoSuggestSource()));

        #endregion


        #endregion
    }
}
