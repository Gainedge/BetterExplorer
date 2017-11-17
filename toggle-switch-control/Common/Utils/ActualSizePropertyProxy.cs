using System.ComponentModel;
using System.Windows;

namespace ToggleSwitch.Utils
{
	public class ActualSizePropertyProxy : FrameworkElement, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		#region ElementProperty (Dependancy Property)

		public static readonly DependencyProperty ElementProperty =
			DependencyProperty.Register("Element", typeof(FrameworkElement), typeof(ActualSizePropertyProxy),
										new PropertyMetadata(null, OnElementPropertyChanged));

		private static void OnElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            if (d != null)
            {
                ((ActualSizePropertyProxy)d).OnElementChanged(e);
            }
		}

		public FrameworkElement Element
		{
			get { return (FrameworkElement)GetValue(ElementProperty); }
			set { SetValue(ElementProperty, value); }
		}

		#endregion

		public double ActualHeightValue
		{
			get { return Element == null ? 0 : Element.ActualHeight; }
		}

		public double ActualWidthValue
		{
			get { return Element == null ? 0 : Element.ActualWidth; }
		}

		private void OnElementChanged(DependencyPropertyChangedEventArgs e)
		{
			var oldElement = (FrameworkElement)e.OldValue;
			var newElement = (FrameworkElement)e.NewValue;

			if (oldElement != null)
			{
				oldElement.SizeChanged -= ElementSizeChanged;
			}

            if (newElement != null)
            {
                newElement.SizeChanged += ElementSizeChanged;
            }

			NotifyPropertyChanged();
		}

		private void ElementSizeChanged(object sender, SizeChangedEventArgs e)
		{
			NotifyPropertyChanged();
		}

		private void NotifyPropertyChanged()
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("ActualWidthValue"));
				PropertyChanged(this, new PropertyChangedEventArgs("ActualHeightValue"));
			}
		}
	}
}