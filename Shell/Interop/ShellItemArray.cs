using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	internal class ShellItemArray : IShellItemArray
	{
		List<IShellItem> shellItemsList = new List<IShellItem>();

		internal ShellItemArray(IShellItem[] shellItems)
		{
			shellItemsList.AddRange(shellItems);
		}

		#region IShellItemArray Members

		public HResult BindToHandler(IntPtr pbc, ref Guid rbhid, ref Guid riid, out IntPtr ppvOut)
		{
			throw new NotSupportedException();
		}

		public HResult GetPropertyStore(int Flags, ref Guid riid, out IntPtr ppv)
		{
			throw new NotSupportedException();
		}

		public HResult GetPropertyDescriptionList(ref PROPERTYKEY keyType, ref Guid riid, out IntPtr ppv)
		{
			throw new NotSupportedException();
		}

		public HResult GetAttributes(uint dwAttribFlags, uint sfgaoMask, out uint psfgaoAttribs)
		{
			throw new NotSupportedException();
		}

		public HResult EnumItems(out IntPtr ppenumShellItems)
		{
			throw new NotSupportedException();
		}

		#endregion

		public void BindToHandler(IntPtr pbc, Guid bhid, Guid riid, out IntPtr ppv)
		{
			throw new NotImplementedException();
		}

		public void GetPropertyStore(int flags, Guid riid, out IntPtr ppv)
		{
			throw new NotImplementedException();
		}

		public void GetPropertyDescriptionList(int keyType, Guid riid, out IntPtr ppv)
		{
			throw new NotImplementedException();
		}

		public void GetAttributes(int dwAttribFlags, int sfgaoMask, out int psfgaoAttribs)
		{
			throw new NotImplementedException();
		}

		void IShellItemArray.GetCount(out uint pdwNumItems)
		{
			pdwNumItems = (uint)shellItemsList.Count;
			//return HResult.S_OK;
		}

		void IShellItemArray.GetItemAt(uint dwIndex, out IShellItem ppsi)
		{
			int index = (int)dwIndex;

			if (index < shellItemsList.Count)
			{
				ppsi = shellItemsList[index];
				//return HResult.S_OK;
			}
			else
			{
				ppsi = null;
				//return HResult.S_FALSE;
			}
		}

		void IShellItemArray.EnumItems(out IntPtr ppenumShellItems)
		{
			throw new NotSupportedException();
		}
	}
}
