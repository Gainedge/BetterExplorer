using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BExplorer.Shell
{
	/// <summary>
	/// Interaction logic for ToolTip.xaml
	/// </summary>
	public partial class ToolTip : Window, INotifyPropertyChanged
	{
		private String _Contents;
		public String Contents
		{
			get
			{
				return this._Contents;
			}
			set
			{
				this._Contents = value;
				RaisePropertyChanged("Contents");
			}
		}
		public ToolTip()
		{
			InitializeComponent();
			this.DataContext = this;
		}
		public ToolTip(String contents)
		{
			InitializeComponent();
			this.Contents = contents;
			this.DataContext = this;

		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			Hide();
		}
		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
			Hide();
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string propertyName)
		{
			var handlers = PropertyChanged;

			handlers(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
