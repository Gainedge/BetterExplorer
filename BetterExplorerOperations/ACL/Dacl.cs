using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	class OrderAceAccess : IComparer
	{
		public int Compare(object o1, object o2)
		{
			AceAccess lhs = (AceAccess)o1;
			AceAccess rhs = (AceAccess)o2;

			// The order is:
			// denied direct aces
			// denied direct object aces
			// allowed direct aces
			// allowed direct object aces
			// denied inherit aces
			// denied inherit object aces
			// allowed inherit aces
			// allowed inherit object aces

			// inherited aces are always "greater" than non-inherited aces
			if(lhs.IsInherited && !rhs.IsInherited)
				return 1;

			if(!lhs.IsInherited && rhs.IsInherited)
				return -1;

			// if the aces are *both* either inherited or non-inherited, continue...

			// allowed aces are always "greater" than denied aces (subject to above)
			if(lhs.IsAllowed && !rhs.IsAllowed)
				return 1;

			if(!lhs.IsAllowed && rhs.IsAllowed)
				return -1;

			// if the aces are *both* either allowed or denied, continue...

			// object aces are always "greater" than non-object aces (subject to above)
			if(lhs.IsObjectAce && !rhs.IsObjectAce)
				return 1;

			if(!lhs.IsObjectAce && rhs.IsObjectAce)
				return -1;

			// aces are "equal" (e.g., both are access denied inherited object aces)
			return 0;
		}
	}

	/// <summary>
	/// Summary description for Dacl.
	/// </summary>
	public class Dacl : Acl
	{
		internal Dacl(IntPtr pacl) : base(pacl)
		{
		}
		public Dacl() : base()
		{
		}
		/// <summary>
		///  This algorithm was copied from ATL source code: CAdcl::PrepareAcesForACL.
		/// 
		///  We can't use QuickSort (or any other n log (n)) generic sort algorithm
		///  because we want partial ordering to be preserved. All we want to do is sort 
		///  the elements according to their "Order" (see OrderAceAccess.Compare method),
		///  but we want the elements which compare to "Equal" to remain in their
		///  original order in the array.
		/// </summary>
		protected override void PrepareAcesForACL()
		{
			IComparer comparer = new OrderAceAccess();

			int nCount = this.AceCount;

			// Find first "h" such that 
			// 1. h * 3 + 1 < nCount
			// 2. (h - 1) is exactly divisible by 3
			int h = 1;
			while(h * 3 + 1 < nCount)
				h = 3 * h + 1;

			while(h > 0)
			{
				for(int i = h - 1; i < nCount; i++)
				{
					Ace pivot = this.GetAce(i);

					int j;
					for(j = i; 
						(j >= h) && (comparer.Compare(this.GetAce(j - h), pivot) > 0); 
						j -= h)
					{
						this.SetAce(j, this.GetAce(j - h));
					}

					this.SetAce(j, pivot);
				}

				h /= 3;
			}
		}
		public void AddAce(AceAccess ace)
		{
			base.AddAce(ace);
		}
	}
}
