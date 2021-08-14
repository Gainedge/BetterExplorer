using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;

namespace BEHelper {

	[ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class BetterExplorerService : IBetterExplorerCommunication {
		public Boolean CreateLink(LinkData data) {
			foreach (var link in data.Items) {
				Library.CreateSymbolicLink(link.DestinationData, link.SourceData,
					link.IsDirectory ? Library.SYMBOLIC_LINK_FLAG.Directory : Library.SYMBOLIC_LINK_FLAG.File);
			}
			Application.Exit();
			return true;
		}

	}
}
