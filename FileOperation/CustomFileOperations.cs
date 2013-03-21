using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.FileOperations;

namespace FileOperation {
  public class CustomFileOperations {

    public static void CopyFile(ShellObject source, String destination)
		{
			CopyFile(source, destination, CopyFileOptions.None);
		}

    public static void CopyFile(ShellObject source, String destination, 
			CopyFileOptions options)
		{
			CopyFile(source, destination, options, null);
		}

    public static bool CopyFile(ShellObject source, String destination, 
			CopyFileOptions options, CopyFileCallback callback)
		{

        return CopyFile(source, destination, options, callback, null);
		}
    public static Boolean MoveFile(ShellObject source, String destination,
      MoveFileFlags options, CopyFileCallback callback) {

      return MoveFile(source, destination, options, callback, null);
    }


    public static void GetCopyData(ShellObject sourceObject, string destDirName, ref List<KeyValuePair<ShellObject, String>> Items) {
      if (sourceObject.IsFolder && sourceObject.Properties.System.FileExtension.Value == null) {
        foreach (var item in (ShellContainer)sourceObject) {
          var tempDestination = Path.Combine(destDirName, sourceObject.GetDisplayName(DisplayNameType.Default));
            GetCopyData(item,tempDestination, ref Items);
        }
      } else {
        Items.Add(new KeyValuePair<ShellObject, string>(sourceObject, Path.Combine(destDirName, Path.GetFileName(sourceObject.ParsingName))));
      }
    }

    public static List<KeyValuePair<ShellObject, String>> GetCopyDataAll(ShellObject[] objects, String destination) {
      List<KeyValuePair<ShellObject, String>> result = new List<KeyValuePair<ShellObject, string>>();
      for (int i = 0; i < objects.Count(); i++) {
        CustomFileOperations.GetCopyData(objects[i], destination, ref result);
      }
      return result;
    }
    public static bool FileOperationCopy(Tuple<ShellObject, String, String, Int32> source, CopyFileOptions options, CopyFileCallback callback, int itemIndex, List<CollisionInfo> colissions) {
      var currentItem = colissions.Where(c => c.item == source.Item1).SingleOrDefault();
      var result = false;
      if (currentItem != null) {
        if (!currentItem.IsCheckedC && currentItem.IsChecked) {
          result = CopyFile(source.Item1, source.Item2, options, callback);
        } else if (currentItem.IsCheckedC && currentItem.IsChecked) {
          result = CopyFile(source.Item1, source.Item3, options, callback);
        }
      } else {
        result = CopyFile(source.Item1, source.Item3, options, callback);
      }
      return result;
    }

    public static void FileOperationMove(Tuple<ShellObject, String, String, Int32> source, MoveFileFlags options, CopyFileCallback callback, int itemIndex, List<CollisionInfo> colissions) {
      var currentItem = colissions.Where(c => c.item == source.Item1).SingleOrDefault();
      if (currentItem != null) {
        if (!currentItem.IsCheckedC && currentItem.IsChecked) {
          MoveFile(source.Item1, source.Item2, options, callback);
        } else if (currentItem.IsCheckedC && currentItem.IsChecked) {
          MoveFile(source.Item1, source.Item3, options, callback);
        }
      } else {
        MoveFile(source.Item1, source.Item3, options, callback);
      }
    }


    public static bool CopyFile(ShellObject source, String destination, 
			CopyFileOptions options, CopyFileCallback callback, object state)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (destination == null) 
				throw new ArgumentNullException("destination");
			if ((options & ~CopyFileOptions.All) != 0) 
				throw new ArgumentOutOfRangeException("options");
      //string dirName = String.Empty;

      if (!Directory.Exists(Path.GetDirectoryName(destination))) {
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
      } 
 
      new FileIOPermission(
        FileIOPermissionAccess.Read, source.ParsingName).Demand();
      new FileIOPermission(
        FileIOPermissionAccess.Write, destination).Demand();

      CopyProgressRoutine cpr = (callback == null ?
        null : new CopyProgressRoutine(new CopyProgressData(
        source, destination, callback, state).CallbackHandler));

      bool cancel = false;
      return CopyFileEx(source.ParsingName, destination, cpr, IntPtr.Zero, ref cancel, (int)options);
		}

    public static Boolean MoveFile(ShellObject source, String destination,
      MoveFileFlags options, CopyFileCallback callback, object state) {
      if (source == null) throw new ArgumentNullException("source");
      if (destination == null)
        throw new ArgumentNullException("destination");
      if ((options & ~ MoveFileFlags.ALL) != 0)
        throw new ArgumentOutOfRangeException("options");
      //string dirName = String.Empty;

      if (!Directory.Exists(Path.GetDirectoryName(destination))) {
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
      }

      new FileIOPermission(
        FileIOPermissionAccess.Read, source.ParsingName).Demand();
      new FileIOPermission(
        FileIOPermissionAccess.Write, destination).Demand();

      CopyProgressRoutine cpr = (callback == null ?
        null : new CopyProgressRoutine(new CopyProgressData(
        source, destination, callback, state).CallbackHandler));

      bool cancel = false;
      return MoveFileWithProgress(source.ParsingName, destination, cpr,
        IntPtr.Zero, options);
    }

		private class CopyProgressData
		{
      private ShellObject _source = null;
      private String _destination = null;
			private CopyFileCallback _callback = null;
			private object _state = null;

      public CopyProgressData(ShellObject source, String destination, 
				CopyFileCallback callback, object state)
			{
				_source = source; 
				_destination = destination;
				_callback = callback;
				_state = state;
			}

			public int CallbackHandler(
				long totalFileSize, long totalBytesTransferred, long streamSize, 
				long streamBytesTransferred, int streamNumber, int callbackReason,
				IntPtr sourceFile, IntPtr destinationFile, IntPtr data)
			{
				return (int)_callback(_source, _destination, _state, 
					totalFileSize, totalBytesTransferred);
			}
		}

		private delegate int CopyProgressRoutine(
			long totalFileSize, long totalBytesTransferred, long streamSize, 
			long streamBytesTransferred, int streamNumber, int callbackReason,
			IntPtr sourceFile, IntPtr destinationFile, IntPtr data);

    [Flags]
    public enum MoveFileFlags {
      NONE = 0x0,
      MOVEFILE_REPLACE_EXISTING = 0x00000001,
      MOVEFILE_COPY_ALLOWED = 0x00000002,
      MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004,
      MOVEFILE_WRITE_THROUGH = 0x00000008,
      MOVEFILE_CREATE_HARDLINK = 0x00000010,
      MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x00000020,
      ALL = MOVEFILE_REPLACE_EXISTING | MOVEFILE_COPY_ALLOWED | MOVEFILE_DELAY_UNTIL_REBOOT | MOVEFILE_WRITE_THROUGH | MOVEFILE_CREATE_HARDLINK | MOVEFILE_FAIL_IF_NOT_TRACKABLE
    }

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool CopyFileEx(
			string lpExistingFileName, string lpNewFileName,
			CopyProgressRoutine lpProgressRoutine,
			IntPtr lpData, ref bool pbCancel, int dwCopyFlags);

    [SuppressUnmanagedCodeSecurity]
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool MoveFileWithProgress(string lpExistingFileName,
       string lpNewFileName, CopyProgressRoutine lpProgressRoutine,
       IntPtr lpData, MoveFileFlags dwFlags);
	}



  public delegate CopyFileCallbackAction CopyFileCallback(ShellObject source, String destination, object state, long totalFileSize, long totalBytesTransferred);

	public enum CopyFileCallbackAction
	{
		Continue = 0,
		Cancel = 1,
		Pause = 2,
		Quiet = 3
	}

	[Flags]
	public enum CopyFileOptions
	{
		None = 0x0,
		FailIfDestinationExists = 0x1,
		Restartable = 0x2,
		AllowDecryptedDestination = 0x8,
		All = FailIfDestinationExists | Restartable | AllowDecryptedDestination
	}
}
