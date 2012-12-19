// Version 2.0

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.COM;

namespace Nomad.Archive.SevenZip
{
  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000000050000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IProgress
  {
    void SetTotal(ulong total);
    void SetCompleted([In] ref ulong completeValue);
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600100000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IArchiveOpenCallback
  {
    // ref ulong replaced with IntPtr because handlers ofter pass null value
    // read actual value with Marshal.ReadInt64
    void SetTotal(
      IntPtr files,  // [In] ref ulong files, can use 'ulong* files' but it is unsafe
      IntPtr bytes); // [In] ref ulong bytes
    void SetCompleted(
      IntPtr files,  // [In] ref ulong files
      IntPtr bytes); // [In] ref ulong bytes
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000500100000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ICryptoGetTextPassword
  {
    [PreserveSig]
    int CryptoGetTextPassword(
      [MarshalAs(UnmanagedType.BStr)] out string password);
    //[return : MarshalAs(UnmanagedType.BStr)]
    //string CryptoGetTextPassword();
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000500110000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ICryptoGetTextPassword2
  {
    void CryptoGetTextPassword2(
      [MarshalAs(UnmanagedType.Bool)] out bool passwordIsDefined,
      [MarshalAs(UnmanagedType.BStr)] out string password);
  }

  public enum AskMode : int
  {
    kExtract = 0,
    kTest,
    kSkip
  }

  public enum OperationResult : int
  {
    kOK = 0,
    kUnSupportedMethod,
    kDataError,
    kCRCError
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600200000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IArchiveExtractCallback //: IProgress
  {
    // IProgress
    void SetTotal(ulong total);
    void SetCompleted([In] ref ulong completeValue);

    // IArchiveExtractCallback
    [PreserveSig]
    int GetStream(
      uint index,
      [MarshalAs(UnmanagedType.Interface)] out ISequentialOutStream outStream,
      AskMode askExtractMode);
    // GetStream OUT: S_OK - OK, S_FALSE - skeep this file

    void PrepareOperation(AskMode askExtractMode);
    void SetOperationResult(OperationResult resultEOperationResult);
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600300000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IArchiveOpenVolumeCallback
  {
    void GetProperty(
      ItemPropId propID, // PROPID
      IntPtr value); // PROPVARIANT

    [PreserveSig]
    int GetStream(
      [MarshalAs(UnmanagedType.LPWStr)] string name,
      [MarshalAs(UnmanagedType.Interface)] out IInStream inStream);
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600400000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IInArchiveGetStream
  {
    [return: MarshalAs(UnmanagedType.Interface)]
    ISequentialInStream GetStream(uint index);
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300010000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ISequentialInStream
  {
    //[PreserveSig]
    //int Read(
    //  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data,
    //  uint size,
    //  IntPtr processedSize); // ref uint processedSize

    uint Read(
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data,
      uint size);

    /*
    Out: if size != 0, return_value = S_OK and (*processedSize == 0),
      then there are no more bytes in stream.
    if (size > 0) && there are bytes in stream, 
    this function must read at least 1 byte.
    This function is allowed to read less than number of remaining bytes in stream.
    You must call Read function in loop, if you need exact amount of data
    */
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300020000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ISequentialOutStream
  {
    [PreserveSig]
    int Write(
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data,
      uint size,
      IntPtr processedSize); // ref uint processedSize
    /*
    if (size > 0) this function must write at least 1 byte.
    This function is allowed to write less than "size".
    You must call Write function in loop, if you need to write exact amount of data
    */
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300030000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IInStream //: ISequentialInStream
  {
    //[PreserveSig]
    //int Read(
    //  [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data,
    //  uint size,
    //  IntPtr processedSize); // ref uint processedSize

    // ISequentialInStream
    uint Read(
      [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data,
      uint size);

    // IInStream
    //[PreserveSig]
    void Seek(
      long offset,
      uint seekOrigin,
      IntPtr newPosition); // ref long newPosition
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300040000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IOutStream //: ISequentialOutStream
  {
    // ISequentialOutStream
    [PreserveSig]
    int Write(
      [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] data,
      uint size,
      IntPtr processedSize); // ref uint processedSize

    // IOutStream
    //[PreserveSig]
    void Seek(
      long offset,
      uint seekOrigin,
      IntPtr newPosition); // ref long newPosition

    [PreserveSig]
    int SetSize(long newSize);
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300060000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IStreamGetSize
  {
    ulong GetSize();
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000300070000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IOutStreamFlush
  {
    void Flush();
  }

  public enum ItemPropId : uint
  {
    kpidNoProperty = 0,

    kpidHandlerItemIndex = 2,
    kpidPath,
    kpidName,
    kpidExtension,
    kpidIsFolder,
    kpidSize,
    kpidPackedSize,
    kpidAttributes,
    kpidCreationTime,
    kpidLastAccessTime,
    kpidLastWriteTime,
    kpidSolid,
    kpidCommented,
    kpidEncrypted,
    kpidSplitBefore,
    kpidSplitAfter,
    kpidDictionarySize,
    kpidCRC,
    kpidType,
    kpidIsAnti,
    kpidMethod,
    kpidHostOS,
    kpidFileSystem,
    kpidUser,
    kpidGroup,
    kpidBlock,
    kpidComment,
    kpidPosition,
    kpidPrefix,
    // 4.58+
    kpidNumSubFolders,
    kpidNumSubFiles,
    kpidUnpackVer,
    kpidVolume,
    kpidIsVolume,
    kpidOffset,
    kpidLinks,
    kpidNumBlocks,
    kpidNumVolumes,
    kpidTimeType,
    // 4.60+
    kpidBit64,
    kpidBigEndian,
    kpidCpu,
    kpidPhySize,
    kpidHeadersSize,
    kpidChecksum,
    kpidCharacts,
    kpidVa,

    kpidTotalSize = 0x1100,
    kpidFreeSpace,
    kpidClusterSize,
    kpidVolumeName,

    kpidLocalName = 0x1200,
    kpidProvider,

    kpidUserDefined = 0x10000
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600600000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  //[AutomationProxy(true)]
  public interface IInArchive
  {
    [PreserveSig]
    int Open(
      IInStream stream,
      /*[MarshalAs(UnmanagedType.U8)]*/ [In] ref ulong maxCheckStartPosition,
      [MarshalAs(UnmanagedType.Interface)] IArchiveOpenCallback openArchiveCallback);
    [PreserveSig]
    int Close();
    //void GetNumberOfItems([In] ref uint numItem);
    uint GetNumberOfItems();

    void GetProperty(
      uint index,
      ItemPropId propID, // PROPID
      ref PropVariant value); // PROPVARIANT

    [PreserveSig]
    int Extract(
      [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] uint[] indices, //[In] ref uint indices,
      uint numItems,
      int testMode,
      [MarshalAs(UnmanagedType.Interface)] IArchiveExtractCallback extractCallback);
    // indices must be sorted 
    // numItems = 0xFFFFFFFF means all files
    // testMode != 0 means "test files operation"

    void GetArchiveProperty(
      uint propID, // PROPID
      ref PropVariant value); // PROPVARIANT

    //void GetNumberOfProperties([In] ref uint numProperties);
    uint GetNumberOfProperties();
    void GetPropertyInfo(
      uint index,
      [MarshalAs(UnmanagedType.BStr)] out string name,
      out ItemPropId propID, // PROPID
      out ushort varType); //VARTYPE

    //void GetNumberOfArchiveProperties([In] ref uint numProperties);
    uint GetNumberOfArchiveProperties();
    void GetArchivePropertyInfo(
      uint index,
      [MarshalAs(UnmanagedType.BStr)] string name,
      ref uint propID, // PROPID
      ref ushort varType); //VARTYPE
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600800000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IArchiveUpdateCallback // : IProgress
  {
    // IProgress
    void SetTotal(ulong total);
    void SetCompleted([In] ref ulong completeValue);

    // IArchiveUpdateCallback
    void GetUpdateItemInfo(int index,
      out int newData, // 1 - new data, 0 - old data
      out int newProperties, // 1 - new properties, 0 - old properties
      out uint indexInArchive); // -1 if there is no in archive, or if doesn't matter

    void GetProperty(
      int index,
      ItemPropId propID, // PROPID
      IntPtr value); // PROPVARIANT

    void GetStream(int index, out ISequentialInStream inStream);

    void SetOperationResult(int operationResult);
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600820000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IArchiveUpdateCallback2 // : IArchiveUpdateCallback
  {
    // IProgress
    void SetTotal(ulong total);
    void SetCompleted([In] ref ulong completeValue);

    // IArchiveUpdateCallback
    void GetUpdateItemInfo(int index,
      out int newData, // 1 - new data, 0 - old data
      out int newProperties, // 1 - new properties, 0 - old properties
      out uint indexInArchive); // -1 if there is no in archive, or if doesn't matter

    void GetProperty(
      int index,
      ItemPropId propID, // PROPID
      IntPtr value); // PROPVARIANT

    void GetStream(int index, out ISequentialInStream inStream);

    void SetOperationResult(int operationResult);

    // IArchiveUpdateCallback2
    void GetVolumeSize(int index, out ulong size);
    void GetVolumeStream(int index, out ISequentialOutStream volumeStream);
  }

  public enum FileTimeType : int
  {
    kWindows,
    kUnix,
    kDOS
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600A00000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IOutArchive
  {
    void UpdateItems(
      ISequentialOutStream outStream,
      int numItems,
      IArchiveUpdateCallback updateCallback);

    FileTimeType GetFileTimeType();
  }

  [ComImport]
  [Guid("23170F69-40C1-278A-0000-000600030000")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface ISetProperties
  {
    void SetProperties(
      [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] names,
      IntPtr values,
      int numProperties);
  }

  public enum ArchivePropId : uint
  {
    kName = 0,
    kClassID,
    kExtension,
    kAddExtension,
    kUpdate,
    kKeepName,
    kStartSignature,
    kFinishSignature,
    kAssociate
  }

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int CreateObjectDelegate(
    [In] ref Guid classID,
    [In] ref Guid interfaceID,
    //out IntPtr outObject);
    [MarshalAs(UnmanagedType.Interface)] out object outObject);

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int GetHandlerPropertyDelegate(
    ArchivePropId propID,
    ref PropVariant value); // PROPVARIANT

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int GetNumberOfFormatsDelegate(out uint numFormats);

  [UnmanagedFunctionPointer(CallingConvention.StdCall)]
  public delegate int GetHandlerProperty2Delegate(
    uint formatIndex,
    ArchivePropId propID,
    ref PropVariant value); // PROPVARIANT

  public class StreamWrapper : IDisposable
  {
    protected Stream _BaseStream;

    protected StreamWrapper(Stream baseStream)
    {
      _BaseStream = baseStream;
    }

    public virtual void Dispose()
    {
      _BaseStream.Close();
    }

    public virtual void Seek(long offset, uint seekOrigin, IntPtr newPosition)
    {
      long Position = (uint)_BaseStream.Seek(offset, (SeekOrigin)seekOrigin);
      if (newPosition != IntPtr.Zero)
        Marshal.WriteInt64(newPosition, Position);
    }

    public Stream BaseStream
    {
      get { return _BaseStream; }
    }
  }

  public class InStreamWrapper : StreamWrapper, ISequentialInStream, IInStream//, IStreamGetSize
  {
    public InStreamWrapper(Stream baseStream) : base(baseStream) { }

    public uint Read(byte[] data, uint size)
    {
      return (uint)_BaseStream.Read(data, 0, (int)size);
    }

    public ulong GetSize()
    {
      return (ulong)BaseStream.Length;
    }
  }

  // Can close base stream after period of inactivity and reopen it when needed.
  // Useful for long opened archives (prevent locking archive file on disk).
  public class InStreamTimedWrapper : StreamWrapper, ISequentialInStream, IInStream
  {
    private string _BaseStreamFileName;
    private long BaseStreamLastPosition;
    private Timer CloseTimer;

    private const int KeepAliveInterval = 5 * 1000; // 5 sec

    public InStreamTimedWrapper(Stream baseStream)
      : base(baseStream)
    {
      FileStream BaseFileStream = _BaseStream as FileStream;
      if ((BaseFileStream != null) && !_BaseStream.CanWrite && _BaseStream.CanSeek)
      {
        _BaseStreamFileName = BaseFileStream.Name;
        CloseTimer = new Timer(new TimerCallback(CloseStream), null, KeepAliveInterval, Timeout.Infinite);
      }
    }

    private void CloseStream(object state)
    {
      if (CloseTimer != null)
      {
        CloseTimer.Dispose();
        CloseTimer = null;
      }

      if (_BaseStream != null)
      {
        if (_BaseStream.CanSeek)
          BaseStreamLastPosition = _BaseStream.Position;
        _BaseStream.Close();
        _BaseStream = null;
      }
    }

    public override void Dispose()
    {
      CloseStream(null);
      _BaseStreamFileName = null;
    }

    public void Flush()
    {
      CloseStream(null);
    }

    protected void ReopenStream()
    {
      // If base stream closed (by us or by external code) then try to reopen stream
      if ((_BaseStream == null) || !_BaseStream.CanRead)
      {
        if (_BaseStreamFileName != null)
        {
          _BaseStream = new FileStream(_BaseStreamFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
          _BaseStream.Position = BaseStreamLastPosition;
          CloseTimer = new Timer(new TimerCallback(CloseStream), null, KeepAliveInterval, Timeout.Infinite);
        }
        else
          throw new ObjectDisposedException("StreamWrapper");
      }
      else
        if (CloseTimer != null)
          CloseTimer.Change(KeepAliveInterval, Timeout.Infinite);
    }

    /*public int Read(byte[] data, uint size, IntPtr processedSize)
    {
      int Processed = BaseStream.Read(data, 0, (int)size);
      if (processedSize != IntPtr.Zero)
        Marshal.WriteInt32(processedSize, Processed);
      return 0;
    }*/

    public uint Read(byte[] data, uint size)
    {
      ReopenStream();
      return (uint)_BaseStream.Read(data, 0, (int)size);
    }

    public override void Seek(long offset, uint seekOrigin, IntPtr newPosition)
    {
      if ((_BaseStream == null) && (_BaseStreamFileName != null) && (offset == 0) && (seekOrigin == 0))
      {
        BaseStreamLastPosition = 0;
        if (newPosition != IntPtr.Zero)
          Marshal.WriteInt64(newPosition, BaseStreamLastPosition);
      }
      else
      {
        ReopenStream();
        base.Seek(offset, seekOrigin, newPosition);
      }
    }

    public string BaseStreamFileName
    {
      get { return _BaseStreamFileName; }
    }
  }

  public class OutStreamWrapper : StreamWrapper, ISequentialOutStream, IOutStream
  {
    public OutStreamWrapper(Stream baseStream) : base(baseStream) { }

    public int SetSize(long newSize)
    {
      _BaseStream.SetLength(newSize);
      return 0;
    }

    public int Write(byte[] data, uint size, IntPtr processedSize)
    {
      _BaseStream.Write(data, 0, (int)size);
      if (processedSize != IntPtr.Zero)
        Marshal.WriteInt32(processedSize, (int)size);
      return 0;
    }
  }
}
