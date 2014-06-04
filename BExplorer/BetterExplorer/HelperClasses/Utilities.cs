﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BetterExplorer {
	public static class Utilities {

		static Utilities() {
			RegistryKey rk = Registry.CurrentUser;
			RegistryKey rks = rk.OpenSubKey(@"Software\BExplorer", true);
			if (rks == null)
				rks = rk.CreateSubKey(@"Software\BExplorer");
		}

		#region Registry

		public static void SetRegistryValue(string Name, object Value) {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				rks.SetValue(Name, Value);
			}
		}

		public static void SetRegistryValue(string Name, object Value, RegistryValueKind Kind) {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				rks.SetValue(Name, Value, Kind);

			}
		}

		public static Object GetRegistryValue(string Name, string DefaultValue) {
			using (RegistryKey rk = Registry.CurrentUser, rks = rk.OpenSubKey(@"Software\BExplorer", true)) {
				return rks.GetValue(Name, DefaultValue);
			}
		}

		#endregion

		#region File Extensions

		public static string RemoveExtensionsFromFile(string file, string ext) {
			return file.EndsWith(ext) ? file.Remove(file.LastIndexOf(ext), ext.Length) : file;
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public static string GetExtension(string file) {
			return file.Substring(file.LastIndexOf("."));
		}

		#endregion


		/// <summary>
		/// 
		/// </summary>
		/// <param name="header"></param>
		/// <param name="tag"></param>
		/// <param name="icon"></param>
		/// <param name="name"></param>
		/// <param name="focusable"></param>
		/// <param name="checkable"></param>
		/// <param name="isChecked"></param>
		/// <param name="GroupName"></param>
		/// <param name="onClick">Test</param>
		/// <returns></returns>	
		[System.Diagnostics.DebuggerStepThrough()]
		public static Fluent.MenuItem Build_MenuItem(Object header = null, Object tag = null, Object icon = null, string name = null, object ToolTip = null,
			bool focusable = true, bool checkable = false, bool isChecked = false, string GroupName = null, System.Windows.RoutedEventHandler onClick = null) {
			//TODO: Check if [MenuItem] are focusable by default

			var Item = new Fluent.MenuItem() {
				Name = name,
				Header = header,
				Tag = tag,
				Focusable = focusable,
				IsCheckable = checkable,
				IsChecked = isChecked,
				Icon = icon,
				GroupName = GroupName,
				ToolTip = ToolTip
			};

			if (onClick != null) Item.Click += onClick;
			return Item;
		} //TODO: Convert this into an extension


		[System.Diagnostics.DebuggerStepThrough()]
		public static string GetValueOnly(string property, string value) {
			return value.Substring(property.Length + 1);
		}

	}
}
