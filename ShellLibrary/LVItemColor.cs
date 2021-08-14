using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BExplorer.Shell.Annotations;

namespace BExplorer.Shell
{
	public class LVItemColor : INotifyPropertyChanged
	{
		public String ExtensionList { get; set; }
		public Color TextColor { get;  set; }

		public LVItemColor()
		{
			
			
		}

		public LVItemColor(String extensions, Color textColor)
		{
			this.ExtensionList = extensions;
			this.TextColor = textColor;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
