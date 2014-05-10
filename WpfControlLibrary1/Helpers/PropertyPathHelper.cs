using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace BetterExplorerControls {
	//Thomas Levesque - http://stackoverflow.com/questions/3577802/wpf-getting-a-property-value-from-a-binding-path
	public static class PropertyPathHelper {
		internal static Dictionary<Tuple<Type, string>, PropertyInfo> _cacheDic
				= new Dictionary<Tuple<Type, string>, PropertyInfo>();
		public static object GetValueFromPropertyInfo(object obj, string[] propPaths) {
			var current = obj;
			foreach (var ppath in propPaths) {
				if (current == null)
					return null;

				Type type = current.GetType();
				var key = new Tuple<Type, string>(type, ppath);

				PropertyInfo pInfo = null;
				lock (_cacheDic) {
					if (!(_cacheDic.ContainsKey(key))) {
						pInfo = type.GetProperty(ppath);
						_cacheDic.Add(key, pInfo);
					}
					pInfo = _cacheDic[key];
				}

				if (pInfo == null)
					return null;
				current = pInfo.GetValue(current);
			}
			return current;
		}

		public static object GetValueFromPropertyInfo(object obj, string propertyPath) {
			return GetValueFromPropertyInfo(obj, propertyPath.Split('.'));
		}

		public static object GetValue(object obj, string propertyPath) {
			Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
			if (dispatcher == null)
				return GetValueFromPropertyInfo(obj, propertyPath);

			Binding binding = new Binding(propertyPath);
			binding.Mode = BindingMode.OneTime;
			binding.Source = obj;
			BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
			return _dummy.GetValue(Dummy.ValueProperty);


		}

		public static object GetValue(object obj, BindingBase binding) {
			return GetValue(obj, (binding as Binding).Path.Path);
		}



		private static readonly Dummy _dummy = new Dummy();

		private class Dummy : DependencyObject {
			public static readonly DependencyProperty ValueProperty =
					DependencyProperty.Register("Value", typeof(object), typeof(Dummy), new UIPropertyMetadata(null));
		}
	}
}
