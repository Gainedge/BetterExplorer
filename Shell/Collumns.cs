using BExplorer.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell
{
	/// <summary>
	/// Class containing definition for columns
	/// </summary>
	[Serializable]
	public class Collumns
	{
		public PROPERTYKEY pkey;
		public string Name;
		public int Width;
	}
}
