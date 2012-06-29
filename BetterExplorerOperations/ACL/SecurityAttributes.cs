using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;

	/// <summary>
	/// Summary description for SecurityAttributes.
	/// </summary>
	public class SecurityAttributes
	{
		private bool _inheritHandles = false;
		private SecurityDescriptor _secDesc;

		public SecurityAttributes()
		{
		}
		//
		// Only called from the custom marshaler
		//
		internal void Parse(IntPtr pSecAttr)
		{
			if (pSecAttr == IntPtr.Zero)
				throw new ArgumentException("Can't parse a NULL struct", "pSecAttr");

			SECURITY_ATTRIBUTES attrs = (SECURITY_ATTRIBUTES)
				new MemoryMarshaler(pSecAttr).ParseStruct(typeof(SECURITY_ATTRIBUTES));

			_inheritHandles = Win32.ToBool(attrs.bInheritHandle);

			if (attrs.lpSecurityDescriptor != IntPtr.Zero)
			{
				// Create a new SecDesc only if we don't already have one or
				// if the one we have hasn't the same ptr to unmanged memory 
				if (_secDesc == null || _secDesc.Ptr != attrs.lpSecurityDescriptor)
					_secDesc = new SecurityDescriptor(attrs.lpSecurityDescriptor);
			}
		}
		//
		// Only called from the custom marshaler
		//
		internal SECURITY_ATTRIBUTES GetSECURITY_ATTRIBUTES()
		{
			SECURITY_ATTRIBUTES attrs = new SECURITY_ATTRIBUTES() ;
			attrs.nLength = (uint)SECURITY_ATTRIBUTES.SizeOf;;
			attrs.bInheritHandle = (_inheritHandles ? Win32.TRUE : Win32.FALSE);
			attrs.lpSecurityDescriptor = (_secDesc == null ? IntPtr.Zero : _secDesc.Ptr);
			return attrs;
		}
		public bool InheridHandles
		{
			get
			{
				return _inheritHandles;
			}
			set
			{
				_inheritHandles = value;
			}
		}
		public SecurityDescriptor SecurityDescriptor
		{
			get
			{
				return _secDesc;
			}
			set
			{
				_secDesc = value;
			}
		}
	}
}
