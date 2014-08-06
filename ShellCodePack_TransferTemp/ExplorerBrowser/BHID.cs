using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.Versioning;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using MS.WindowsAPICodePack.Internal;
using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;
using Microsoft.Win32;
using System.Security;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Controls;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Interop;

namespace Microsoft.WindowsAPICodePack.Controls.WindowsForms {
	/// <summary>
	/// BHID class
	/// </summary>
	public class BHID {
		static Guid m_SFObject = new Guid("3981e224-f559-11d3-8e3a-00c04f6837d5");
		static Guid m_SFUIObject = new Guid("3981e225-f559-11d3-8e3a-00c04f6837d5");

		public static Guid SFObject {
			get { return m_SFObject; }
		}

		public static Guid SFUIObject {
			get { return m_SFUIObject; }
		}


		/// <summary>
		/// Returns the IShellFolder interface from IShellItem
		/// </summary>
		/// <param name="sitem">The IShellItem represented like ShellObject</param>
		/// <returns></returns>
		public static IShellFolder GetIShellFolder(ShellObject sitem) {
			IShellFolder result;
			((IShellItem2)sitem.nativeShellItem).BindToHandler(IntPtr.Zero,
				BHID.SFObject, typeof(IShellFolder).GUID, out result);
			return result;
		}
	}
}
