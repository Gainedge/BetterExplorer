using System;

namespace Microsoft.Win32.Security
{
	/// <summary>
	/// Summary description for Sacl.
	/// </summary>
	public class Sacl : Acl
	{
		internal Sacl(IntPtr pacl) : base(pacl)
		{
		}
		public Sacl() : base()
		{
		}
		protected override void PrepareAcesForACL()
		{
			// We don't need to sort them for SACL
			return;
		}
	}
}
