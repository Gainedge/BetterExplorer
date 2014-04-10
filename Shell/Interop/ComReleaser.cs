using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	class ComReleaser<T> : IDisposable where T : class
	{
		private T _obj;

		public ComReleaser(T obj)
		{
			if (obj == null) throw new ArgumentNullException("obj");
			if (!obj.GetType().IsCOMObject) throw new ArgumentOutOfRangeException("obj");
			_obj = obj;
		}

		public T Item { get { return _obj; } }

		public void Dispose()
		{
			if (_obj != null)
			{
				Marshal.FinalReleaseComObject(_obj);
				_obj = null;
			}
		}
	}
}
