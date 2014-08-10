using BExplorer.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BExplorer.Shell {
	//TODO: Document this!
	public class AssociationItem {
		public ShellItem Owner { get; private set; }
		public String DisplayName { get; set; }
		public String ApplicationPath { get; set; }

		public BitmapSource Icon {
			get {
				var item = new ShellItem(this.ApplicationPath.ToShellParsingName());
				return item.Thumbnail.SmallBitmapSource;
			}
		}

		public AssociationItem(ShellItem item) {
			this.Owner = item;
		}


		public void Invoke() {
			//TODO: Test these changes!!
			RegistryKey root = Registry.ClassesRoot;
			var applications = root.OpenSubKey("Applications", RegistryKeyPermissionCheck.ReadSubTree);
			var currentApplication = applications.OpenSubKey(Path.GetFileName(this.ApplicationPath), RegistryKeyPermissionCheck.ReadSubTree);

			Action OnFail = () => {
				Process.Start(this.ApplicationPath, "\"" + this.Owner.ParsingName + "\"");
				applications.Close();
				root.Close();
			};

			if (currentApplication == null) {
				OnFail();
				return;
			}

			var command = currentApplication.OpenSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadSubTree);
			if (command == null) {
				command = currentApplication.OpenSubKey(@"shell\edit\command", RegistryKeyPermissionCheck.ReadSubTree);
			}

			if (command == null) {
				OnFail();
				return;
			}

			var commandValue = command.GetValue("").ToString();
			var argsArray = Shell32.CommandLineToArgs(commandValue).ToList();
			var executable = argsArray[0];
			argsArray.RemoveAt(0);
			for (int i = 0; i < argsArray.Count; i++) {
				if (argsArray[i] != "%1" && argsArray[i] != "%L" && argsArray[i].ToLowerInvariant().Contains("photoviewer.dll")) {
					argsArray[i] = "\"" + argsArray[i].Replace(",", "") + "\",";
				}
				else if ((argsArray[i] == "%1" || argsArray[i] == "%L") && !argsArray[i].ToLowerInvariant().Contains("photoviewer.dll")) {
					argsArray[i] = "\"" + argsArray[i] + "\"";
				}
			}

			var args = String.Join(" ", argsArray).Replace("%1", this.Owner.ParsingName).Replace("%L", this.Owner.ParsingName).Replace(@"\\", @"\");
			commandValue = Environment.ExpandEnvironmentVariables(commandValue);
			Process.Start(executable, args);

			command.Close();
			currentApplication.Close();
			applications.Close();
			root.Close();
		}


		/*
		public void Invoke() {
			RegistryKey root = Registry.ClassesRoot;
			var applications = root.OpenSubKey("Applications", RegistryKeyPermissionCheck.ReadSubTree);
			var currentApplication = applications.OpenSubKey(Path.GetFileName(this.ApplicationPath), RegistryKeyPermissionCheck.ReadSubTree);

			if (currentApplication == null) {
				Process.Start(this.ApplicationPath, "\"" + this.Owner.ParsingName + "\"");
			}
			else {
				var commandKey = currentApplication.OpenSubKey(@"shell\open\command", RegistryKeyPermissionCheck.ReadSubTree);
				if (commandKey != null) {
					var commandValue = commandKey.GetValue("").ToString();
					var argsArray = Shell32.CommandLineToArgs(commandValue).ToList();
					var executable = argsArray[0];
					argsArray.RemoveAt(0);
					for (int i = 0; i < argsArray.Count; i++) {
						if (argsArray[i] != "%1" && argsArray[i] != "%L" && argsArray[i].ToLowerInvariant().Contains("photoviewer.dll")) {
							argsArray[i] = "\"" + argsArray[i].Replace(",", "") + "\",";
						}
						else if ((argsArray[i] == "%1" || argsArray[i] == "%L") && !argsArray[i].ToLowerInvariant().Contains("photoviewer.dll")) {
							argsArray[i] = "\"" + argsArray[i] + "\"";
						}
					}
					var args = String.Join(" ", argsArray).Replace("%1", this.Owner.ParsingName).Replace("%L", this.Owner.ParsingName).Replace(@"\\", @"\");
					commandValue = Environment.ExpandEnvironmentVariables(commandValue);
					Process.Start(executable, args);
					commandKey.Close();
				}
				else {
					var commandKeyEdit = currentApplication.OpenSubKey(@"shell\edit\command", RegistryKeyPermissionCheck.ReadSubTree);
					if (commandKeyEdit == null) {
						Process.Start(this.ApplicationPath, "\"" + this.Owner.ParsingName + "\"");
					}
					else {
						var commandValue = commandKeyEdit.GetValue("").ToString();
						var argsArray = Shell32.CommandLineToArgs(commandValue).ToList();
						var executable = argsArray[0];
						argsArray.RemoveAt(0);
						for (int i = 0; i < argsArray.Count; i++) {
							if (argsArray[i] != "%1" && argsArray[i] != "%L" && argsArray[i].ToLowerInvariant().Contains("photoviewer.dll")) {
								argsArray[i] = "\"" + argsArray[i].Replace(",", "") + "\",";
							}
							else if ((argsArray[i] == "%1" || argsArray[i] == "%L") && !argsArray[i].ToLowerInvariant().Contains("photoviewer.dll")) {
								argsArray[i] = "\"" + argsArray[i] + "\"";
							}
						}
						var args = String.Join(" ", argsArray).Replace("%1", this.Owner.ParsingName).Replace("%L", this.Owner.ParsingName).Replace(@"\\", @"\");
						commandValue = Environment.ExpandEnvironmentVariables(commandValue);
						Process.Start(executable, args);
						commandKeyEdit.Close();
					}
				}
				currentApplication.Close();
			}

			applications.Close();
			root.Close();
		}
		*/
	}
}
