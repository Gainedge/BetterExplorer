using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Shell;

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
