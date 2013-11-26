using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BExplorer.Shell
{
	public class LVItemColor
	{
		[XmlElement("list")]
		public String ExtensionList { get; private set; }
		[XmlIgnore]
		public Color TextColor { get; private set; }


		public LVItemColor(String extensions, Color textColor)
		{
			this.ExtensionList = extensions;
			this.TextColor = textColor;
		}
	}
}
