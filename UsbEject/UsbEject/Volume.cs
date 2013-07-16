// UsbEject version 1.0 March 2006
// written by Simon Mourier <email: simon [underscore] mourier [at] hotmail [dot] com>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BetterExplorer.UsbEject
{
    /// <summary>
    /// A volume device.
    /// </summary>
    public class Volume : Device, IComparable
    {
        private string _volumeName;
        private string _logicalDrive;
        private int[] _diskNumbers;
        private List<Device> _disks;
        private List<Device> _removableDevices;

        internal Volume(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
            :base(deviceClass, deviceInfoData, path, index)
        {
        }

        /// <summary>
        /// Gets the volume's name.
        /// </summary>
        public string VolumeName
        {
            get
            {
                if (_volumeName == null)
                {
                    StringBuilder sb = new StringBuilder(1024);
                    if (!Native.GetVolumeNameForVolumeMountPoint(Path + "\\", sb, (uint)sb.Capacity))
                    {
                        // throw new Win32Exception(Marshal.GetLastWin32Error());
                        
                    }

                    if (sb.Length > 0)
                    {
                        _volumeName = sb.ToString();
                    }
                }
                return _volumeName;
            }
        }

        /// <summary>
        /// Gets the volume's logical drive in the form [letter]:\
        /// </summary>
        public string LogicalDrive
        {
            get
            {
                if ((_logicalDrive == null) && (VolumeName != null))
                {
                    ((VolumeDeviceClass)DeviceClass)._logicalDrives.TryGetValue(VolumeName, out _logicalDrive);
                }
                return _logicalDrive;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this volume is a based on USB devices.
        /// </summary>
        public override bool IsUsb
        {
            get
            {
                if (Disks != null)
                {
                    foreach (Device disk in Disks)
                    {
                        if (disk.IsUsb)
                            return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a list of underlying disks for this volume.
        /// </summary>
        public List<Device> Disks
        {
            get
            {
                if (_disks == null)
                {
                    _disks = new List<Device>();

                    if (DiskNumbers != null)
                    {
                        DiskDeviceClass disks = new DiskDeviceClass();
                        foreach (int index in DiskNumbers)
                        {
                            if (index < disks.Devices.Count)
                            {
                                _disks.Add(disks.Devices[index]);
                            }
                        }
                    }
                }
                return _disks;
            }
        }

        private int[] DiskNumbers
        {
            get
            {
                if (_diskNumbers == null)
                {
                    List<int> numbers = new List<int>();
                    if (LogicalDrive != null)
                    {

                        //IntPtr hFile = Native.CreateFile(@"\\.\" + LogicalDrive, Native.GENERIC_READ, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
                        IntPtr hFile = Native.CreateFile(@"\\.\" + LogicalDrive, 0, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE, IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
                        if (hFile.ToInt32() == Native.INVALID_HANDLE_VALUE)
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        int size = 0x400; // some big size
                        IntPtr buffer = Marshal.AllocHGlobal(size);
                        int bytesReturned = 0;
                        try
                        {
                            if (!Native.DeviceIoControl(hFile, Native.IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, buffer, size, out bytesReturned, IntPtr.Zero))
                            {
                                // do nothing here on purpose
                            }
                        }
                        finally
                        {
                            Native.CloseHandle(hFile);
                        }

                        if (bytesReturned > 0)
                        {
                            int numberOfDiskExtents = (int)Marshal.PtrToStructure(buffer, typeof(int));
                            for (int i = 0; i < numberOfDiskExtents; i++)
                            {
                                IntPtr extentPtr = new IntPtr(buffer.ToInt32() + Marshal.SizeOf(typeof(long)) + i * Marshal.SizeOf(typeof(Native.DISK_EXTENT)));
                                Native.DISK_EXTENT extent = (Native.DISK_EXTENT)Marshal.PtrToStructure(extentPtr, typeof(Native.DISK_EXTENT));
                                numbers.Add(extent.DiskNumber);
                            }
                        }
                        Marshal.FreeHGlobal(buffer);
                    }

                    _diskNumbers = new int[numbers.Count];
                    numbers.CopyTo(_diskNumbers);
                }
                return _diskNumbers;
            }
        }

        /// <summary>
        /// Gets a list of removable devices for this volume.
        /// </summary>
        public override List<Device> RemovableDevices
        {
            get
            {
                if (_removableDevices == null)
                {
                    _removableDevices = new List<Device>();
                    if (Disks == null)
                    {
                        _removableDevices = base.RemovableDevices;
                    }
                    else
                    {
                        foreach (Device disk in Disks)
                        {
                            foreach (Device device in disk.RemovableDevices)
                            {
                                _removableDevices.Add(device);
                            }
                        }
                    }
                }
                return _removableDevices;
            }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
        public override int CompareTo(object obj)
        {
            Volume device = obj as Volume;
            if (device == null)
                throw new ArgumentException();

            if (LogicalDrive == null)
                return 1;

            if (device.LogicalDrive == null)
                return -1;

            return LogicalDrive.CompareTo(device.LogicalDrive);
        }
    }
}
