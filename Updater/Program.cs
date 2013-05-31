using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using Nomad.Archive.SevenZip;
using System.Runtime.InteropServices;
using Microsoft.COM;
using System.Diagnostics;
using WindowsHelper;

namespace Updater {
  static class Program {

    private static string SevenZDllPath {
      get {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WindowsAPI.Is64bitProcess(Process.GetCurrentProcess())?"7z.dll":"7z.dll");
      }
  }
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] arguments) {
      var updateFiles = arguments.Where(c => c.StartsWith("UP:")).Single().Substring(3).Split(new Char[]{';'}, StringSplitOptions.RemoveEmptyEntries);
      String CurrentexePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
			string dir = Path.GetDirectoryName(CurrentexePath);
      foreach (var item in updateFiles) {
        MessageBox.Show(item);
        ListOrExtract(item, dir, true);
      }
    }

    private static void ListOrExtract(string archiveName, string extractLocation, bool extract) {
      using (SevenZipFormat Format = new SevenZipFormat(SevenZDllPath)) {
        IInArchive Archive = Format.CreateInArchive(SevenZipFormat.GetClassIdFromKnownFormat(KnownSevenZipFormat.SevenZip));
        if (Archive == null) {
          return;
        }

        try {
          using (InStreamWrapper ArchiveStream = new InStreamWrapper(File.OpenRead(archiveName))) {
            IArchiveOpenCallback OpenCallback = new ArchiveOpenCallback();

            // 32k CheckPos is not enough for some 7z archive formats
            ulong CheckPos = 128 * 1024;
            if (Archive.Open(ArchiveStream, ref CheckPos, OpenCallback) != 0)
              return;

            if (extract) {
              uint Count = Archive.GetNumberOfItems();
              for (int i = 0; i < Count; i++) {
                PropVariant Name = new PropVariant();
                Archive.GetProperty((uint)i, ItemPropId.kpidPath, ref Name);
                string FileName = (string)Name.GetObject();
                Archive.Extract(new uint[] { (uint)i }, 1, 0, new ArchiveExtractCallback((uint)i, FileName, extractLocation));
              }
              
            } else {
              //Console.WriteLine("List:");
              String files = "";
              uint Count = Archive.GetNumberOfItems();
              for (uint I = 0; I < Count; I++) {
                PropVariant Name = new PropVariant();
                Archive.GetProperty(I, ItemPropId.kpidPath, ref Name);
                files += String.Format("{0} - {1}\r\n", I, Name.GetObject());

              }
              MessageBox.Show(files);
            }
          }
        } finally {
          Marshal.ReleaseComObject(Archive);
        }
      }
    }

    private class ArchiveOpenCallback : IArchiveOpenCallback {
      public void SetTotal(IntPtr files, IntPtr bytes) {
      }

      public void SetCompleted(IntPtr files, IntPtr bytes) {
      }
    }

    private class ArchiveExtractCallback : IProgress, IArchiveExtractCallback {
      private uint FileNumber;
      private string FileName;
      private string Destinationdir;
      private OutStreamWrapper FileStream;

      public ArchiveExtractCallback(uint fileNumber, string fileName, string DestDir) {
        this.FileNumber = fileNumber;
        this.FileName = fileName;
        this.Destinationdir = DestDir;
      }

      #region IProgress Members

      public void SetTotal(ulong total) {
      }

      public void SetCompleted(ref ulong completeValue) {
      }

      #endregion

      #region IArchiveExtractCallback Members

      public int GetStream(uint index, out ISequentialOutStream outStream, AskMode askExtractMode) {
        if ((index == FileNumber) && (askExtractMode == AskMode.kExtract)) {
          string FileDir = Path.GetDirectoryName(FileName);
          if (!string.IsNullOrEmpty(FileDir))
            Directory.CreateDirectory(FileDir);
          FileStream = new OutStreamWrapper(File.Create(Path.Combine(this.Destinationdir, FileName)));

          outStream = FileStream;
        } else
          outStream = null;

        return 0;
      }

      public void PrepareOperation(AskMode askExtractMode) {
      }

      public void SetOperationResult(OperationResult resultEOperationResult) {
        FileStream.Dispose();
        Console.WriteLine(resultEOperationResult);
      }

      #endregion
    }

    private class ArchiveUpdateCallback : IProgress, IArchiveUpdateCallback {
      private IList<FileInfo> FileList;
      private Stream CurrentSourceStream;

      public ArchiveUpdateCallback(IList<FileInfo> list) {
        FileList = list;
      }

      #region IProgress Members

      public void SetTotal(ulong total) {
      }

      public void SetCompleted(ref ulong completeValue) {
      }

      #endregion

      #region IArchiveUpdateCallback Members

      public void GetUpdateItemInfo(int index, out int newData, out int newProperties, out uint indexInArchive) {
        newData = 1;
        newProperties = 1;
        indexInArchive = 0xFFFFFFFF;
      }

      private void GetTimeProperty(DateTime time, IntPtr value) {
        Marshal.GetNativeVariantForObject(time.ToFileTime(), value);
        Marshal.WriteInt16(value, (short)VarEnum.VT_FILETIME);
      }

      public void GetProperty(int index, ItemPropId propID, IntPtr value) {
        FileInfo Source = FileList[index];
        switch (propID) {
          case ItemPropId.kpidPath:
            Marshal.GetNativeVariantForObject(Path.GetFileName(Source.FullName), value);
            break;
          case ItemPropId.kpidIsFolder:
          case ItemPropId.kpidIsAnti:
            Marshal.GetNativeVariantForObject(false, value);
            break;
          //case ItemPropId.kpidAttributes:
          //  Marshal.WriteInt16(value, (short)VarEnum.VT_EMPTY);
          //  break;
          case ItemPropId.kpidCreationTime:
            GetTimeProperty(Source.CreationTime, value);
            break;
          case ItemPropId.kpidLastAccessTime:
            GetTimeProperty(Source.LastAccessTime, value);
            break;
          case ItemPropId.kpidLastWriteTime:
            GetTimeProperty(Source.LastWriteTime, value);
            break;
          case ItemPropId.kpidSize:
            Marshal.GetNativeVariantForObject((ulong)Source.Length, value);
            break;
          default:
            Marshal.WriteInt16(value, (short)VarEnum.VT_EMPTY);
            break;
        }
      }

      public void GetStream(int index, out ISequentialInStream inStream) {
        FileInfo Source = FileList[index];

        Console.Write("Packing: ");
        Console.Write(Path.GetFileName(Source.FullName));
        Console.Write(' ');

        CurrentSourceStream = Source.OpenRead();
        inStream = new InStreamTimedWrapper(CurrentSourceStream);
      }

      public void SetOperationResult(int operationResult) {
        CurrentSourceStream.Close();
        Console.WriteLine("Ok");
      }

      #endregion
    }
  }
}
