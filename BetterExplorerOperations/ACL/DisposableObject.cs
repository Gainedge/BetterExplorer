using System;

namespace Microsoft.Win32.Security
{
	/// <summary>
	///  Abstract base class for any disposable object.
	///  Handle the finalizer and the call the Gc.SuppressFinalize.
	///  Derived classes must implement "Dispose(bool disposing)".
	/// </summary>
	public abstract class DisposableObject : IDisposable
	{
		~DisposableObject()
		{
			Dispose(false);
		}
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected abstract void Dispose(bool disposing);
	}
}
