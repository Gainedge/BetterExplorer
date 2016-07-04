using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
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
			try {
				var perceivedType = PerceivedType.Unknown;
				if (obj != null) {
					var perceivedTypeProp = obj.GetPropertyValue(SystemProperties.PerceivedType, typeof(PerceivedType));
					if (perceivedTypeProp != null && perceivedTypeProp.VarType != VarEnum.VT_EMPTY && perceivedTypeProp.VarType != VarEnum.VT_ERROR) {
						perceivedType = (PerceivedType) obj.GetPropertyValue(SystemProperties.PerceivedType, typeof(PerceivedType)).Value;
					}
				}
				if (perceivedType ==
				    PerceivedType.Image && obj != null && !obj.IsFolder) {
					return ((FrameworkElement)container).FindResource("FSImageTooltip") as DataTemplate;
				} else if (obj != null && obj.IsFileSystem) {
					return ((FrameworkElement)container).FindResource("FSTooltip") as DataTemplate;
				} else {
					return ((FrameworkElement)container).FindResource("FSTooltip") as DataTemplate;
				}
			}
			catch {
				return ((FrameworkElement)container).FindResource("FSTooltip") as DataTemplate;
			}
		}
	}
}
