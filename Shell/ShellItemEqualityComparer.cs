using BExplorer.Shell._Plugin_Interfaces;
using System;
using System.Collections.Generic;

namespace BExplorer.Shell
{
	public class ShellItemEqualityComparer : IEqualityComparer<IListItemEx>
	{
		public bool Equals(IListItemEx x, IListItemEx y) => x.Equals(y);
		public int GetHashCode(IListItemEx obj) => 0;
	}
}