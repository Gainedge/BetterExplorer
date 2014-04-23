using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;

namespace BetterExplorerControls
{
	public class WatermarkedTextBox : TextBox
	{

		#region Dependency Properties
		public static DependencyProperty WatermarkProperty = DependencyProperty.Register("Watermark",
																																						 typeof(string),
																																						 typeof(WatermarkedTextBox),
																																						 new PropertyMetadata(new PropertyChangedCallback(OnWatermarkChanged)));
		#endregion

		#region Public Member
		protected new Brush Foreground
		{
			get { return base.Foreground; }
			set { base.Foreground = value; }
		}

		public string Watermark
		{
			get { return (string)GetValue(WatermarkProperty); }
			set { SetValue(WatermarkProperty, value); }
		}

		public Boolean IsWatermarkShown {
			get
			{
				return this._isWatermarked;
			}
		}
		#endregion

		#region Private Member
		private bool _isWatermarked;
		private Binding _textBinding;
		#endregion

		#region Initializer
		public WatermarkedTextBox()
		{
			Loaded += (s, ea) => ShowWatermark();
		}
		#endregion

		#region Events
		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
			HideWatermark();
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			ShowWatermark();
		}

		private static void OnWatermarkChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
		{
			var tbw = sender as WatermarkedTextBox;
			if (tbw == null) return;
			tbw.ShowWatermark();
		}
		#endregion

		#region Private Methods

		private void ShowWatermark()
		{
			if (string.IsNullOrEmpty(base.Text))
			{
				_isWatermarked = true;
				base.Foreground = new SolidColorBrush(Colors.Gray);
				base.FontStyle = FontStyles.Italic;
				var bindingExpression = GetBindingExpression(TextProperty);
				_textBinding = bindingExpression == null ? null : bindingExpression.ParentBinding;
				if (bindingExpression != null)
					bindingExpression.UpdateSource();
				BindingOperations.ClearBinding(this, TextProperty);
				base.Text = Watermark;
			}
		}

		private void HideWatermark()
		{
			if (_isWatermarked)
			{
				_isWatermarked = false;
				ClearValue(ForegroundProperty);
				base.FontStyle = FontStyles.Normal;
				base.Text = "";
				BindingOperations.ClearBinding(this, TextProperty);
			}
		}

		#endregion

	}
}
