using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BExplorer.Shell;
using BExplorer.Shell._Plugin_Interfaces;
using Microsoft.Win32;
using Settings;

namespace Wpf.Controls {

	[TemplatePart(Name = "PART_CloseButton", Type = typeof(ButtonBase))]
	public class TabItem : System.Windows.Controls.TabItem {

		#region Properties
		public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(TabItem), new UIPropertyMetadata(null));
		public static readonly DependencyProperty AllowDeleteProperty = DependencyProperty.Register("AllowDelete", typeof(bool), typeof(TabItem), new UIPropertyMetadata(true));

		/// <summary>List of currently selected items (the strings are the parsed name of the actual ShellItem)</summary>
		public List<String> SelectedItems = new List<String>();

		/// <summary>The Navigation History (Log) of the tab</summary>
		public NavigationLog log;//= new NavigationLog();

		/// <summary>The current shell item the tab is on</summary>
		public IListItemEx ShellObject { get; set; }

		/// <summary>The ContextMenu for the tab's header in the TabControl</summary>
		public ContextMenu mnu { get; private set; }


		/// <summary>
		///     Used by the TabPanel for sizing
		/// </summary>
		internal Dimension Dimension { get; set; }

		/// <summary>
		/// Provides a place to display an Icon on the Header and on the DropDown Context Menu
		/// </summary>
		public object Icon
		{
			get { return GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		/// <summary>
		/// Allow the Header to be Deleted by the end user
		/// </summary>
		public bool AllowDelete
		{
			get { return (bool)GetValue(AllowDeleteProperty); }
			set { SetValue(AllowDeleteProperty, value); }
		}


		#endregion Properties

		static TabItem() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Wpf.Controls.TabItem), new FrameworkPropertyMetadata(typeof(Wpf.Controls.TabItem)));
		}

		public TabItem(IListItemEx ShellObject) {
			this.ShellObject = ShellObject;
			log = new NavigationLog(ShellObject);
		}


		/// <summary>
		/// OnApplyTemplate override
		/// </summary>
		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			// wire up the CloseButton's Click event if the button exists
			ButtonBase button = this.Template.FindName("PART_CloseButton", this) as ButtonBase;
			if (button != null) {
				button.PreviewMouseLeftButtonDown += (sender, e) => {
					// get the parent tabcontrol
					TabControl tc = Helper.FindParentControl<TabControl>(this);

					if (tc == null)
						return;
					// remove this tabitem from the parent tabcontrol
					tc.RemoveTabItem(this);
				};
			}
			this.PreviewMouseLeftButtonUp += TabItem_PreviewMouseLeftButtonUp;
			this.PreviewMouseDoubleClick += TabItem_PreviewMouseDoubleClick;
			this.MouseRightButtonUp += TabItem_MouseRightButtonUp;
		}

		void TabItem_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			e.Handled = true;
			TabControl tc = Helper.FindParentControl<TabControl>(this);
			if (tc == null) return;
			//tc.IsSelectionHandled = false;
			//tc.CurrentTabItem = this;
			tc.RaiseTabClick(this);
		}

		/// <summary>
		/// OnMouseEnter, Create and Display a Tooltip
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e) {
			base.OnMouseEnter(e);

			this.ToolTip = this.ShellObject.GetDisplayName(BExplorer.Shell.Interop.SIGDN.DESKTOPABSOLUTEEDITING).Replace("%20", " ").Replace("%3A", ":").Replace("%5C", @"\");
			e.Handled = true;
		}

		/// <summary>
		/// OnMouseLeave, remove the Tooltip
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e) {
			base.OnMouseLeave(e);

			this.ToolTip = null;
			e.Handled = true;
		}

		private void TabItem_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			e.Handled = true;
			TabControl tc = Helper.FindParentControl<TabControl>(this);
			if (tc == null) return;
			//tc.IsSelectionHandled = false;
		}

		private void TabItem_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			var tc = Helper.FindParentControl<TabControl>(this);
			if (tc == null) return;

			var firstItem = tc.Items.OfType<TabItem>().FirstOrDefault();
			this.mnu = new ContextMenu();

			Action<string, RoutedEventHandler> worker = (x, y) => {
				var item = new MenuItem { Header = x };
				item.Click += y;
				this.mnu.Items.Add(item);
			};

			worker("Close current tab", new RoutedEventHandler((owner, a) => tc.RemoveTabItem(this)));

			worker("Close all tabs", new RoutedEventHandler(
				(owner, a) => {
					foreach (TabItem tabItem in tc.Items.OfType<TabItem>().ToArray()) {
						tc.RemoveTabItem(tabItem, true, true);
					}
					tc.NewTab(BESettings.StartupLocation, true);
				}));

			worker("Close all other tabs", new RoutedEventHandler((owner, a) => tc.CloseAllTabsButThis(this)));

			this.mnu.Items.Add(new Separator());

			worker("New tab", new RoutedEventHandler((owner, a) => tc.NewTab()));
			worker("Clone tab", new RoutedEventHandler((owner, a) => tc.CloneTabItem(this)));

			this.mnu.Items.Add(new Separator());

			var miundocloser = new MenuItem();
			miundocloser.Header = "Undo close tab";
			miundocloser.IsEnabled = tc.ReopenableTabs.Count > 0;
			miundocloser.Tag = "UCTI";
			miundocloser.Click += new RoutedEventHandler((owner, a) => tc.ReOpenTab(tc.ReopenableTabs.Last()));

			this.mnu.Items.Add(miundocloser);
			this.mnu.Items.Add(new Separator());

			worker("Open in new window", new RoutedEventHandler(
				(owner, a) => {
					System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, this.ShellObject.ParsingName + " /nw");
					tc.RemoveTabItem(this);
				}));

			if (this.mnu != null && this.mnu.Items.Count > 0) {
				this.mnu.Placement = PlacementMode.Bottom;
				this.mnu.PlacementTarget = this;
				this.mnu.IsOpen = true;
			}
		}
	}
}