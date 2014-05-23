using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer {
	public class FriendlySizeConverter {
		public enum FileSizeMeasurements {
			Bytes = 0,
			Kilobytes = 1,
			Megabytes = 2,
			Gigabytes = 3,
		}

		public static long GetByteLength(double size, FileSizeMeasurements type) {
			switch (type) {
				case FileSizeMeasurements.Bytes:
					return Convert.ToInt32(size);
				case FileSizeMeasurements.Kilobytes:
					return Convert.ToInt32(size * 1000);
				case FileSizeMeasurements.Megabytes:
					return Convert.ToInt32(size * 1000000);
				case FileSizeMeasurements.Gigabytes:
					return Convert.ToInt32(size * 1000000000);
				default:
					return Convert.ToInt32(size);
			}
		}

		public static string GetFriendlySize(double bytes) {
			if (bytes < 1000)
				return bytes.ToString() + " B";
			else if (bytes < 1000000)
				return (Math.Round((bytes / 1000), 2, MidpointRounding.AwayFromZero)).ToString() + " KB";
			else if (bytes < 1000000000)
				return (Math.Round((bytes / 1000000), 2, MidpointRounding.AwayFromZero)).ToString() + " MB";
			else
				return (Math.Round((bytes / 1000000000), 2, MidpointRounding.AwayFromZero)).ToString() + " GB";
		}
	}
}
