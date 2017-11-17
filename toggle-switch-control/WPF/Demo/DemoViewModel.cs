using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace Demo
{
	public class DemoViewModel : INotifyPropertyChanged
	{
		private readonly DispatcherTimer _restartEventTimer = new DispatcherTimer();

		public DemoViewModel()
		{
			_restartEventTimer.Interval = TimeSpan.FromSeconds(1.5);
			_restartEventTimer.Tick += delegate { _restartEventTimer.Stop(); RestartToggleChecked = false; };
		}

		private bool _restartToggleChecked;
		public bool RestartToggleChecked
		{
			get
			{
				return _restartToggleChecked;
			}
			set
			{
				if (_restartToggleChecked != value)
				{
					_restartToggleChecked = value;
					InvokePropertyChanged("RestartToggleChecked");

					if (_restartToggleChecked)
					{
						_restartEventTimer.Start();
					}
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void InvokePropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
