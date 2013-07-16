// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections.Generic;
using System.Text;

namespace BetterExplorer.UsbEject
{
    /// <summary>
    /// The device class for volume devices.
    /// </summary>
    public class VolumeDeviceClass : DeviceClass
    {
        internal SortedDictionary<string, string> _logicalDrives = new SortedDictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the VolumeDeviceClass class.
        /// </summary>
        public VolumeDeviceClass()
            : base(new Guid(Native.GUID_DEVINTERFACE_VOLUME))
        {
            foreach(string drive in Environment.GetLogicalDrives())
            {
                StringBuilder sb = new StringBuilder(1024);
                if (Native.GetVolumeNameForVolumeMountPoint(drive, sb, (uint)sb.Capacity))
                {
                    _logicalDrives[sb.ToString()] = drive.Replace("\\", "");
                }
            }
        }

        internal override Device CreateDevice(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
        {
            return new Volume(deviceClass, deviceInfoData, path, index);
        }
    }
}
