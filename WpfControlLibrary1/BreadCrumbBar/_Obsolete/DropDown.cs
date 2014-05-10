using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BetterExplorerControls {
	/// <summary>
	/// Display a ToggleButton and when it's clicked, show it's content as a dropdown.
	/// </summary>
	[Obsolete("Not used!", true)]
	public class DropDown : HeaderedContentControl {
		#region Constructor
		static DropDown() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDown),
				new System.Windows.FrameworkPropertyMetadata(typeof(DropDown)));
		}

		public DropDown() {

		}

		#endregion

		#region Methods

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			var popup = this.Template.FindName("PART_Popup", this);
			if (popup is Popup) {
				_popup = (Popup)this.Template.FindName("PART_Popup", this);
				_content = (ContentPresenter)this.Template.FindName("PART_Content", this);

				_popup.AddHandler(Popup.LostFocusEvent,
				   new RoutedEventHandler((o, e) => {
					   //(o as DropDownControl).                   
					   //IsDropDownOpen = false;
				   }));
			}
		}

		private static void OnIsDropDownOpenChanged(object sender, DependencyPropertyChangedEventArgs args) {
			DropDown ddc = (DropDown)sender;
			if (ddc._popup != null) {
				ddc._popup.IsOpen = (bool)args.NewValue;
			}
			//if (ddc._content != null)
			//{
			//    ddc._content.Focus();
			//}
			//if (((bool)args.NewValue) && ddc._dropDownGrid != null)
			//{
			//    //Setfocu
			//    //ddc._dropDownGrid.
			//    //Debug.WriteLine(ddc._dropDownGrid.IsFocused);
			//}
		}

		#endregion

		#region Data

		Popup _popup = null;
		ContentPresenter _content = null;

		#endregion

		#region DependencyProperties

		public bool IsDropDownOpen {
			get { return (bool)GetValue(IsDropDownOpenProperty); }
			set { SetValue(IsDropDownOpenProperty, value); }
		}

		public static readonly DependencyProperty IsDropDownOpenProperty =
			DependencyProperty.Register("IsDropDownOpen", typeof(bool),
			typeof(DropDown), new UIPropertyMetadata(false,
				new PropertyChangedCallback(OnIsDropDownOpenChanged)));


		public bool IsDropDownAlignLeft {
			get { return (bool)GetValue(IsDropDownAlignLeftProperty); }
			set { SetValue(IsDropDownAlignLeftProperty, value); }
		}

		public static readonly DependencyProperty IsDropDownAlignLeftProperty =
			DependencyProperty.Register("IsDropDownAlignLeft", typeof(bool),
			typeof(DropDown), new UIPropertyMetadata(false));


		public UIElement PlacementTarget {
			get { return (UIElement)GetValue(PlacementTargetProperty); }
			set { SetValue(PlacementTargetProperty, value); }
		}

		public static readonly DependencyProperty PlacementTargetProperty =
			Popup.PlacementTargetProperty.AddOwner(typeof(DropDown));

		public PlacementMode Placement {
			get { return (PlacementMode)GetValue(PlacementProperty); }
			set { SetValue(PlacementProperty, value); }
		}

		public static readonly DependencyProperty PlacementProperty =
			Popup.PlacementProperty.AddOwner(typeof(DropDown));



		public ControlTemplate HeaderButtonTemplate {
			get { return (ControlTemplate)GetValue(HeaderButtonTemplateProperty); }
			set { SetValue(HeaderButtonTemplateProperty, value); }
		}

		public static readonly DependencyProperty HeaderButtonTemplateProperty =
			DependencyProperty.Register("HeaderButtonTemplate", typeof(ControlTemplate), typeof(DropDown));


		public double HorizontalOffset {
			get { return (double)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}

		public static readonly DependencyProperty HorizontalOffsetProperty =
			Popup.HorizontalOffsetProperty.AddOwner(typeof(DropDown));


		public double VerticalOffset {
			get { return (double)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}

		public static readonly DependencyProperty VerticalOffsetProperty =
		   Popup.VerticalOffsetProperty.AddOwner(typeof(DropDown));

		//IsHeaderEnabled
		//IsDropDownOpen

		#endregion
	}
}
