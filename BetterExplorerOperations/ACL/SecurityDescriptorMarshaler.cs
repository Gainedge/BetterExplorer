using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Microsoft.Win32.Security
{
	/// <summary>
	///  Custom marshaler for a SecurityDescriptor. 
	///  We only support the Managed -> Native directionality.
	/// </summary>
	public class SecurityDescriptorMarshaler : ICustomMarshaler
	{
		/// <summary>
		///  Required by the runtime marshaler.
		/// </summary>
		/// <param name="data"></param>
		/// <returns>Our marshaler for the SecurityDescriptor class</returns>
		public static ICustomMarshaler GetInstance(string data)
		{
			return new SecurityDescriptorMarshaler();;
		}

		private SecurityDescriptor _wrapper;

		#region ICustomMarshaler Members
		public object MarshalNativeToManaged(System.IntPtr pNativeData)
		{
			throw new NotSupportedException(
				"Marshaling a security descriptor from unmanged memory is not supported." +
				"Use the static memebers of the SecurityDescriptor class instead.");
		}
		public System.IntPtr MarshalManagedToNative(object ManagedObj)
		{
			SecurityDescriptor sdIn = (SecurityDescriptor)ManagedObj;
			IntPtr sdPtr = IntPtr.Zero;
			if (sdIn != null)
			{
				_wrapper = sdIn;		// Keep a ref to the managed object
				sdPtr = sdIn.Ptr;
			}

			Debug.Assert(sdPtr != IntPtr.Zero,
				"Warning: It's almost always a bad idea to use a NULL security descriptor");
			return sdPtr;
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
		}

		#endregion
	}
}
