using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BetterExplorerControls {

	public class WatermarkedTextBox : TextBox {

		#region Dependency Properties

		public static DependencyProperty WatermarkProperty =
			DependencyProperty.Register("Watermark", typeof(string), typeof(WatermarkedTextBox), new PropertyMetadata(new PropertyChangedCallback(OnWatermarkChanged)));

		#endregion Dependency Properties

		#region Public Member

		public Boolean IsWatermarkShown { get { return this._isWatermarked; } }

		public string Watermark {
			get { return (string)GetValue(WatermarkProperty); }
			set { SetValue(WatermarkProperty, value); }
		}

		protected new Brush Foreground {
			get { return base.Foreground; }
			set { base.Foreground = value; }
		}

		#endregion Public Member

		#region Private Member

		private bool _isWatermarked;
		private Binding _textBinding;

		#endregion Private Member

		#region Initializer

		public WatermarkedTextBox() {
			this.SetResourceReference(StyleProperty, typeof(TextBox));
			Loaded += (s, ea) => ShowWatermark();
		}

		#endregion Initializer

		#region Events

		protected override void OnGotFocus(RoutedEventArgs e) {
			base.OnGotFocus(e);
			HideWatermark();
		}

		protected override void OnLostFocus(RoutedEventArgs e) {
			base.OnLostFocus(e);
			ShowWatermark();
		}

		private static void OnWatermarkChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea) {
			var tbw = sender as WatermarkedTextBox;
			if (tbw == null) return;
			tbw.ShowWatermark();
		}

		#endregion Events

		#region Private Methods

		private void ShowWatermark() {
			if (string.IsNullOrEmpty(base.Text)) {
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

		private void HideWatermark() {
			if (_isWatermarked) {
				_isWatermarked = false;
				ClearValue(ForegroundProperty);
				base.FontStyle = FontStyles.Normal;
				base.Text = "";
				BindingOperations.ClearBinding(this, TextProperty);
			}
		}

		#endregion Private Methods
	}
}