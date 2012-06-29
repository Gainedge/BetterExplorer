using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;

	/// <summary>
	/// Custom marshaler for SecurityAttributes class.
	/// Marshals from/to the Win32 SECURITY_ATTRIBUTES struct.
	/// </summary>
	public class SecurityAttributesMarshaler : ICustomMarshaler
	{
		/// <summary>
		///  Required by the runtime marshaler. We return a new instance
		///  at each call, because our marshaler instance has some internal state. 
		/// </summary>
		/// <param name="data"></param>
		/// <returns>Our marshaler for the SecurityAttributes class</returns>
		// 
		public static ICustomMarshaler GetInstance(string data)
		{
			return new SecurityAttributesMarshaler();
		}

		// The original managed object. Needed for GC liveness and in/out support.
		private SecurityAttributes _wrapper;

		#region ICustomMarshaler Members

		public object MarshalNativeToManaged(System.IntPtr pNativeData)
		{
			// If native ptr is NULL, return null
			if (pNativeData == IntPtr.Zero)
				return null;

			// If we have an existing wrapper, re-use it.
			// This allows us to properly support the In/Out semantics.
			if (_wrapper != null)
			{
				_wrapper.Parse(pNativeData);
				return _wrapper;
			}

			// Create a new wrapper and parse the native struct
			_wrapper = new SecurityAttributes();
			_wrapper.Parse(pNativeData);
			return _wrapper;
		}

		public System.IntPtr MarshalManagedToNative(object ManagedObj)
		{
			SecurityAttributes saIn = (SecurityAttributes)ManagedObj;
			if (saIn == null)
				return IntPtr.Zero;

			// Keep a reference to the wrapper in case "MarshalNativeToManaged"
			// is called later (In/Out marshaling)
			this._wrapper = saIn;

			// Allocate unmanaged memeory to store the SECURITY_ATTRIBUTES struct
			SECURITY_ATTRIBUTES saOut = saIn.GetSECURITY_ATTRIBUTES();
			IntPtr pSaIn = Win32.AllocGlobal(SECURITY_ATTRIBUTES.SizeOf);
			Marshal.StructureToPtr(saOut, pSaIn, false);
			return pSaIn;
		}
		public void CleanUpManagedData(object ManagedObj)
		{
		}
		public int GetNativeDataSize()
		{
			// The return value is currently ignored.
			return -1;
		}
		public void CleanUpNativeData(System.IntPtr pNativeData)
		{
			// We need to preserve/restore the last error here, because
			// we're called before the P/Invoke call returns to the caller.
			// If the caller of the P/Invoke call needs to access GetLastError,
			// it would always be "0" because Win32.FreeGlobal resets it to "0".
			UInt32 lastErr = Win32.GetLastError();
			Win32.FreeGlobal(pNativeData);
			Win32.SetLastError(lastErr);
		}

		#endregion
	}
}
