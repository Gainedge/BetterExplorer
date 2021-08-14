using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	internal class EnumUnknownClass : IEnumUnknown
	{
		List<ICondition> conditionList = new List<ICondition>();
		int current = -1;

		internal EnumUnknownClass(ICondition[] conditions)
		{
			conditionList.AddRange(conditions);
		}

		#region IEnumUnknown Members

		public HResult Next(uint requestedNumber, ref IntPtr buffer, ref uint fetchedNumber)
		{
			current++;

			if (current < conditionList.Count)
			{
				buffer = Marshal.GetIUnknownForObject(conditionList[current]);
				fetchedNumber = 1;
				return HResult.S_OK;
			}

			return HResult.S_FALSE;
		}

		public HResult Skip(uint number)
		{
			int temp = current + (int)number;

			if (temp > (conditionList.Count - 1))
			{
				return HResult.S_FALSE;
			}

			current = temp;
			return HResult.S_OK;
		}

		public HResult Reset()
		{
			current = -1;
			return HResult.S_OK;
		}

		public HResult Clone(out IEnumUnknown result)
		{
			result = new EnumUnknownClass(this.conditionList.ToArray());
			return HResult.S_OK;
		}

		#endregion
	}
}
