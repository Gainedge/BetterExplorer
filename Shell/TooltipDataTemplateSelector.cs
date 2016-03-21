using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BExplorer.Shell.Interop;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {
	public class TooltipDataTemplateSelector : DataTemplateSelector {
		public override DataTemplate SelectTemplate(object item, DependencyObject container) {
			var obj = item as IListItemEx;
			if (obj != null &&
					((PerceivedType)obj.GetPropertyValue(SystemProperties.PerceivedType, typeof(PerceivedType)).Value) ==
					PerceivedType.Image) {
				return ((FrameworkElement)container).FindResource("FSImageTooltip") as DataTemplate;
			} else if (obj != null && obj.IsFileSystem) {
				return ((FrameworkElement)container).FindResource("FSTooltip") as DataTemplate;
			} else {
				return ((FrameworkElement)container).FindResource("FSTooltip") as DataTemplate;
			}
		}
	}
}
