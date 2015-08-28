using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace BEHelper {
	[ServiceContract]
	public interface IBetterExplorerCommunication {

		[OperationContract]
		Boolean CreateLink(LinkData data);

	}
}
