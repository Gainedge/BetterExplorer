using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BEHelper {

	[DataContract]
	public class LinkItem {
		[DataMember]
		public String SourceData { get; set; }
		[DataMember]
		public String DestinationData { get; set; }
		[DataMember]
		public Boolean IsDirectory { get; set; }
	}
}
