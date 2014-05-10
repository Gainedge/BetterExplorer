using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BetterExplorerControls {
	[Obsolete("Not used!", true)]
	public class DropDownList : ComboBox {
		#region Constructor

		static DropDownList() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownList),
				new System.Windows.FrameworkPropertyMetadata(typeof(DropDownList)));


		}


		#endregion

		#region Methods

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
		}

		#endregion

		#region Data

		#endregion

		#region Public Properties

		public static readonly DependencyProperty HeaderProperty =
			HeaderedContentControl.HeaderProperty.AddOwner(typeof(DropDownList));

		public object Header { get { return GetValue(HeaderProperty); } set { SetValue(HeaderProperty, value); } }


		public UIElement PlacementTarget {
			get { return (UIElement)GetValue(PlacementTargetProperty); }
			set { SetValue(PlacementTargetProperty, value); }
		}

		public static readonly DependencyProperty PlacementTargetProperty =
			Popup.PlacementTargetProperty.AddOwner(typeof(DropDownList));

		public PlacementMode Placement {
			get { return (PlacementMode)GetValue(PlacementProperty); }
			set { SetValue(PlacementProperty, value); }
		}

		public static readonly DependencyProperty PlacementProperty =
			Popup.PlacementProperty.AddOwner(typeof(DropDownList));

		public double HorizontalOffset {
			get { return (double)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}

		public static readonly DependencyProperty HorizontalOffsetProperty =
			Popup.HorizontalOffsetProperty.AddOwner(typeof(DropDownList));


		public double VerticalOffset {
			get { return (double)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}

		public static readonly DependencyProperty VerticalOffsetProperty =
		   Popup.VerticalOffsetProperty.AddOwner(typeof(DropDownList));


		public ControlTemplate HeaderButtonTemplate {
			get { return (ControlTemplate)GetValue(HeaderButtonTemplateProperty); }
			set { SetValue(HeaderButtonTemplateProperty, value); }
		}

		public static readonly DependencyProperty HeaderButtonTemplateProperty =
			DropDown.HeaderButtonTemplateProperty.AddOwner(typeof(DropDownList));

		#endregion

	}

	//public class DropDownMenu : Menu
	//{
	//    #region Constructor

	//    static DropDownMenu()
	//    {
	//        DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownMenu),
	//            new System.Windows.FrameworkPropertyMetadata(typeof(DropDownMenu)));


	//    }

	//    #endregion

	//    #region Methods

	//    public override void OnApplyTemplate()
	//    {
	//        base.OnApplyTemplate();
	//        //this.AddHandler(ComboBox.SelectionChangedEvent, (RoutedEventHandler)((o, e) =>
	//        //{
	//        //    this.SelectedIndex = -1;
	//        //}));
	//    }

	//    //protected override DependencyObject GetContainerForItemOverride()
	//    //{
	//    //    return new ComboBoxItem();
	//    //}

	//    #endregion

	//    #region Data

	//    #endregion

	//   #region Public Properties

	//    //public static readonly DependencyProperty HeaderProperty =
	//    //    HeaderedContentControl.HeaderProperty.AddOwner(typeof(DropDownList));

	//    //public object Header { get { return GetValue(HeaderProperty); } set { SetValue(HeaderProperty, value); } }


	//    //public UIElement PlacementTarget
	//    //{
	//    //    get { return (UIElement)GetValue(PlacementTargetProperty); }
	//    //    set { SetValue(PlacementTargetProperty, value); }
	//    //}

	//    //public static readonly DependencyProperty PlacementTargetProperty =
	//    //    Popup.PlacementTargetProperty.AddOwner(typeof(DropDownList));

	//    //public PlacementMode Placement
	//    //{
	//    //    get { return (PlacementMode)GetValue(PlacementProperty); }
	//    //    set { SetValue(PlacementProperty, value); }
	//    //}

	//    //public static readonly DependencyProperty PlacementProperty =
	//    //    Popup.PlacementProperty.AddOwner(typeof(DropDownList));

	//    //public double HorizontalOffset
	//    //{
	//    //    get { return (double)GetValue(HorizontalOffsetProperty); }
	//    //    set { SetValue(HorizontalOffsetProperty, value); }
	//    //}

	//    //public static readonly DependencyProperty HorizontalOffsetProperty =
	//    //    Popup.HorizontalOffsetProperty.AddOwner(typeof(DropDownList));


	//    //public double VerticalOffset
	//    //{
	//    //    get { return (double)GetValue(VerticalOffsetProperty); }
	//    //    set { SetValue(VerticalOffsetProperty, value); }
	//    //}

	//    //public static readonly DependencyProperty VerticalOffsetProperty =
	//    //   Popup.VerticalOffsetProperty.AddOwner(typeof(DropDownList));


	//    //public ControlTemplate HeaderButtonTemplate
	//    //{
	//    //    get { return (ControlTemplate)GetValue(HeaderButtonTemplateProperty); }
	//    //    set { SetValue(HeaderButtonTemplateProperty, value); }
	//    //}

	//    //public static readonly DependencyProperty HeaderButtonTemplateProperty =
	//    //    DropDown.HeaderButtonTemplateProperty.AddOwner(typeof(DropDownList));

	//    #endregion
	//}
}
