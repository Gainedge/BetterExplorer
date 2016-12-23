using System;

namespace BetterExplorer {

	/// <summary>Contains functionality for presenting the size of a file</summary>
	public class FriendlySizeConverter {

		/// <summary>The various units in which to display the size of a file</summary>
		public enum FileSizeMeasurements {
			Bytes = 0,
			Kilobytes = 1,
			Megabytes = 2,
			Gigabytes = 3,
		}

		/// <summary>
		/// Converts a the size of a file in units (<see cref="FileSizeMeasurements"/> and <see cref="double"/>) into the number of bytes
		/// </summary>
		/// <param name="size"></param>
		/// <param name="type"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Converts bytes into a user friendly string converted into larger units when desired.
		/// </summary>
		/// <param name="bytes">The number of bytes</param>
		/// <returns></returns>
		public static string GetFriendlySize(double bytes) {
			if (bytes < 1000)
				return bytes.ToString() + " B";
			else if (bytes < 1000000)
				return Math.Round((bytes / 1000), 2, MidpointRounding.AwayFromZero).ToString() + " KB";
			else if (bytes < 1000000000)
				return Math.Round((bytes / 1000000), 2, MidpointRounding.AwayFromZero).ToString() + " MB";
			else
				return Math.Round((bytes / 1000000000), 2, MidpointRounding.AwayFromZero).ToString() + " GB";
		}
	}
}
