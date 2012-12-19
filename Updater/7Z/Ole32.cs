using System;
using System.Runtime.InteropServices;

namespace Microsoft.COM
{
  [StructLayout(LayoutKind.Explicit, Size = 16)]
  public struct PropVariant
  {
    [DllImport("ole32.dll")]
    private static extern int PropVariantClear(ref PropVariant pvar);

    [FieldOffset(0)]
    public ushort vt;
    [FieldOffset(8)]
    public IntPtr pointerValue;
    [FieldOffset(8)]
    public sbyte sbyteValue;
    [FieldOffset(8)]
    public byte byteValue;
    [FieldOffset(8)]
    public short shortValue;
    [FieldOffset(8)]
    public ushort ushortValue;
    [FieldOffset(8)]
    public int intValue;
    [FieldOffset(8)]
    public uint uintValue;
    [FieldOffset(8)]
    public long longValue;
    [FieldOffset(8)]
    public ulong ulongValue;
    [FieldOffset(8)]
    public float floatValue;
    [FieldOffset(8)]
    public double doubleValue;
    [FieldOffset(8)]
    public System.Runtime.InteropServices.ComTypes.FILETIME filetime;

    public VarEnum VarType
    {
      get { return (VarEnum)vt; }
    }

    public void Clear()
    {
      switch (VarType)
      {
        case VarEnum.VT_EMPTY:
          break;
        case VarEnum.VT_NULL:
        case VarEnum.VT_I2:
        case VarEnum.VT_I4:
        case VarEnum.VT_R4:
        case VarEnum.VT_R8:
        case VarEnum.VT_CY:
        case VarEnum.VT_DATE:
        case VarEnum.VT_ERROR:
        case VarEnum.VT_BOOL:
        //case VarEnum.VT_DECIMAL:
        case VarEnum.VT_I1:
        case VarEnum.VT_UI1:
        case VarEnum.VT_UI2:
        case VarEnum.VT_UI4:
        case VarEnum.VT_I8:
        case VarEnum.VT_UI8:
        case VarEnum.VT_INT:
        case VarEnum.VT_UINT:
        case VarEnum.VT_HRESULT:
        case VarEnum.VT_FILETIME:
          vt = 0;
          break;
        case VarEnum.VT_BSTR:
          Marshal.FreeBSTR(pointerValue);
          vt = 0;
          break;
        default:
          PropVariantClear(ref this);
          break;
      }
    }

    public object GetObject()
    {
      switch (VarType)
      {
        case VarEnum.VT_EMPTY:
          return null;
        case VarEnum.VT_FILETIME:
          return DateTime.FromFileTime(longValue);
        default:
          GCHandle PropHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
          try
          {
            return Marshal.GetObjectForNativeVariant(PropHandle.AddrOfPinnedObject());
          }
          finally
          {
            PropHandle.Free();
          }
      }
    }

    public void SetObject(object value)
    {
      if (value == null)
        vt = (ushort)VarEnum.VT_EMPTY;
      else
        switch (Type.GetTypeCode(value.GetType()))
        {
          case TypeCode.DBNull:
            vt = (ushort)VarEnum.VT_NULL;
            break;
          case TypeCode.Boolean:
            shortValue = Convert.ToInt16(value);
            vt = (ushort)VarEnum.VT_BOOL;
            break;
          //TypeCode.Char = 4,
          case TypeCode.SByte:
            sbyteValue = (sbyte)value;
            vt = (ushort)VarEnum.VT_I1;
            break;
          case TypeCode.Byte:
            byteValue = (byte)value;
            vt = (ushort)VarEnum.VT_UI1;
            break;
          case TypeCode.Int16:
            shortValue = (short)value;
            vt = (ushort)VarEnum.VT_I2;
            break;
          case TypeCode.UInt16:
            ushortValue = (ushort)value;
            vt = (ushort)VarEnum.VT_UI2;
            break;
          case TypeCode.Int32:
            intValue = (int)value;
            vt = (ushort)VarEnum.VT_I4;
            break;
          case TypeCode.UInt32:
            uintValue = (uint)value;
            vt = (ushort)VarEnum.VT_UI4;
            break;
          case TypeCode.Int64:
            longValue = (long)value;
            vt = (ushort)VarEnum.VT_I8;
            break;
          case TypeCode.UInt64:
            ulongValue = (ulong)value;
            vt = (ushort)VarEnum.VT_UI8;
            break;
          case TypeCode.Single:
            floatValue = (float)value;
            vt = (ushort)VarEnum.VT_R4;
            break;
          case TypeCode.Double:
            doubleValue = (double)value;
            vt = (ushort)VarEnum.VT_R8;
            break;
          //TypeCode.Decimal:
          //TypeCode.DateTime,
          case TypeCode.String:
            pointerValue = Marshal.StringToBSTR((string)value);
            vt = (ushort)VarEnum.VT_BSTR;
            break;
        }
    }
  }
}
