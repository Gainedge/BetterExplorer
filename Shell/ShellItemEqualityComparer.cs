using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell._Plugin_Interfaces;

namespace BExplorer.Shell {
	public class ShellItemEqualityComparer : IEqualityComparer<IListItemEx> {
		public Boolean Equals(IListItemEx x, IListItemEx y) {
			return x.Equals(y);
		}

		public Int32 GetHashCode(IListItemEx obj) {
			return 0;
		}
	}
}
