using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
		private ShellItem _ShellItem;
		private DispatcherTimer DelayTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);

		public int Type { get; set; }
		public ShellItem CurrentItem
		{
			get
			{
				return _ShellItem;
			}

			set
			{
				_ShellItem = value;
				RaisePropertyChanged("CurrentItem");
			}
		}

		public Int32 ItemIndex { get; set; }
		public String Contents {get; set;}
		
		public ToolTip()
		{
			InitializeComponent();
			this.DataContext = this;
			DelayTimer.Interval = TimeSpan.FromMilliseconds(700);
			DelayTimer.Tick += DelayTimer_Tick;
		}

		void DelayTimer_Tick(object sender, EventArgs e)
		{
			DelayTimer.Stop();
			this.Show();
			Task.Run(() =>
			{
				Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (ThreadStart)(() =>
				{
					var tooltip = CurrentItem.ToolTipText;
					if (String.IsNullOrEmpty(tooltip) && Type == 1)
					{
						this.Hide();
						return;
					}
					Contents = Type == 0 ? String.Format("{0}\r\n{1}", CurrentItem.DisplayName, CurrentItem.ToolTipText) : CurrentItem.ToolTipText;
					RaisePropertyChanged("Contents");
				}));
			});
			
		}

		public ToolTip(String contents)
		{
			InitializeComponent();
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
			if (this.IsVisible)
				this.Hide();
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
