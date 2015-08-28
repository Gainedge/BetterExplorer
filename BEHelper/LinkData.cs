using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace BEHelper {
	[DataContract]
	public class LinkData {

		[DataMember]
		public LinkItem[] Items { get; set; }

	}
}
