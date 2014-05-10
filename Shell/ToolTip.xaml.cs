using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BExplorer.Shell
{
	/// <summary>
	/// Interaction logic for ToolTip.xaml
	/// </summary>
	public partial class ToolTip : Window, INotifyPropertyChanged
	{
		private String _Contents;
		private DispatcherTimer DelayTimer = new DispatcherTimer();

		public ShellItem CurrentItem { get; set; }

		public Int32 ItemIndex { get; set; }
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
			DelayTimer.Interval = TimeSpan.FromMilliseconds(400);
			DelayTimer.Tick += DelayTimer_Tick;
		}

		void DelayTimer_Tick(object sender, EventArgs e)
		{
			DelayTimer.Stop();
			this.Show();
		}

		public ToolTip(String contents)
		{
			InitializeComponent();
			this.Contents = contents;
			this.DataContext = this;

		}

		public void ShowTooltip()
		{
			if (!DelayTimer.IsEnabled)
				DelayTimer.Start();
		}

		public void HideTooltip()
		{
				DelayTimer.Stop();
				this.Hide();
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			DelayTimer.Stop();
			Hide();
		}
		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);
			DelayTimer.Stop();
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
