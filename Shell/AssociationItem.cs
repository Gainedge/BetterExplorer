using BExplorer.Shell.Interop;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BExplorer.Shell {
	//TODO: Document this or find out if we need it at all

	/// <summary>
	/// Represents an application that this <see cref="ShellItem"/> is associated with.
	/// </summary>
	public class AssociationItem {
		/// <summary>The <see cref="ShellItem"/> that this item is for</summary>
		public ShellItem Owner { get; private set; }

		/// <summary>The display name for the item to be displayed to the user</summary>
		[Obsolete("Not used at all")]
		public string DisplayName { get; set; }

		/// <summary>The path of the application this item represents</summary>
		public string ApplicationPath { get; set; }
		public BitmapSource Icon => new ShellItem(this.ApplicationPath.ToShellParsingName()).Thumbnail.SmallBitmapSource;


		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="item">The <see cref="ShellItem"/> that this item is for (sets the <see cref="Owner"/> property</param>
		public AssociationItem(ShellItem item) {
			this.Owner = item;
		}

		/// <summary>Invokes/Runs the application this item represents and supplies it with the owner</summary>
		public void Invoke() {
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
	}
}
