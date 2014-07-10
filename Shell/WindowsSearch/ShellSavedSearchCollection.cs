//Copyright (c) Microsoft Corporation.  All rights reserved.

using BExplorer.Shell.Interop;
namespace BExplorer.Shell {
	/// <summary>
	/// Represents a saved search
	/// </summary>
	public class ShellSavedSearchCollection : ShellItem {
		internal ShellSavedSearchCollection(IShellItem2 shellItem) : base(shellItem) { }
	}
}
