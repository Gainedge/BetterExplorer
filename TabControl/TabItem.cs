using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using BExplorer.Shell;
using System.Collections.Generic;
using System;

namespace Wpf.Controls
{
		[TemplatePart(Name = "PART_CloseButton", Type = typeof(ButtonBase))]
		public class TabItem : System.Windows.Controls.TabItem
		{
				static TabItem()
				{
						DefaultStyleKeyProperty.OverrideMetadata(typeof(Wpf.Controls.TabItem), new FrameworkPropertyMetadata(typeof(Wpf.Controls.TabItem)));
				}

				public List<String> SelectedItems = new List<String>();
				public NavigationLog log = new NavigationLog();
				public ShellItem ShellObject { get; set; }

				public ContextMenu mnu { get; set; }
				/// <summary>
				/// Provides a place to display an Icon on the Header and on the DropDown Context Menu
				/// </summary>
				public object Icon
				{
						get { return (object)GetValue(IconProperty); }
						set { SetValue(IconProperty, value); }
				}
				public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(TabItem), new UIPropertyMetadata(null));

				/// <summary>
				/// Allow the Header to be Deleted by the end user
				/// </summary>
				public bool AllowDelete
				{
						get { return (bool)GetValue(AllowDeleteProperty); }
						set { SetValue(AllowDeleteProperty, value); }
				}
				public static readonly DependencyProperty AllowDeleteProperty = DependencyProperty.Register("AllowDelete", typeof(bool), typeof(TabItem), new UIPropertyMetadata(true));

				/// <summary>
				/// OnApplyTemplate override
				/// </summary>
				public override void OnApplyTemplate()
				{
						base.OnApplyTemplate();

						// wire up the CloseButton's Click event if the button exists
						ButtonBase button = this.Template.FindName("PART_CloseButton", this) as ButtonBase;
						if (button != null)
						{
							button.PreviewMouseLeftButtonUp += (sender, e) =>
							{
								// get the parent tabcontrol 
								TabControl tc = Helper.FindParentControl<TabControl>(this);
								if (tc == null)
									return;
								// remove this tabitem from the parent tabcontrol
								tc.RemoveTabItem(this);
							};
						}

						this.MouseRightButtonUp += TabItem_MouseRightButtonUp;
				}

				void TabItem_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
				{
					if (this.mnu != null)
					{
						this.mnu.Placement = PlacementMode.Bottom;
						this.mnu.PlacementTarget = this;
						this.mnu.IsOpen = true;
					}
				}

				/// <summary>
				///     Used by the TabPanel for sizing
				/// </summary>
				internal Dimension Dimension { get; set; }

				/// <summary>
				/// OnMouseEnter, Create and Display a Tooltip
				/// </summary>
				/// <param name="e"></param>
				protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
				{
						base.OnMouseEnter(e);

						this.ToolTip = this.ShellObject.GetDisplayName(BExplorer.Shell.Interop.SIGDN.DESKTOPABSOLUTEEDITING);
						e.Handled = true;
				}

				/// <summary>
				/// OnMouseLeave, remove the tooltip
				/// </summary>
				/// <param name="e"></param>
				protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
				{
						base.OnMouseLeave(e);

						this.ToolTip = null;
						e.Handled = true;
				}
		}
}
