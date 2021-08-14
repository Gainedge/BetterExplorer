using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BExplorer.Shell.Interop;


namespace ShellLibrary.Interop {
  [ComImport, Guid("a88826f8-186f-4987-aade-ea0cef8fbfe8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IEnumExplorerCommand {
    /// <summary>Retrieves a specified number of elements that directly follow the current element.</summary>
    /// <param name="celt">
    /// <para>Type: <c>ULONG</c></para>
    /// <para>Specifies the number of elements to fetch.</para>
    /// </param>
    /// <param name="pUICommand">
    /// <para>Type: <c>IExplorerCommand**</c></para>
    /// <para>
    /// Address of an IExplorerCommand interface pointer array of celt elements that, when this method returns, is an array of
    /// pointers to the retrieved elements.
    /// </para>
    /// </param>
    /// <param name="pceltFetched">
    /// <para>Type: <c>ULONG*</c></para>
    /// <para>
    /// When this method returns, contains a pointer to the number of elements actually retrieved. This pointer can be <c>NULL</c>
    /// if this information is not needed.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>Type: <c>HRESULT</c></para>
    /// <para>If this method succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</para>
    /// </returns>
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-ienumexplorercommand-next HRESULT Next(
    // ULONG celt, IExplorerCommand **pUICommand, ULONG *pceltFetched );
    [PreserveSig]
    HResult Next([In] uint celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 0)] Object[] pUICommand, out uint pceltFetched);

    /// <summary>Not currently implemented.</summary>
    /// <param name="celt">
    /// <para>Type: <c>ULONG</c></para>
    /// <para>Currently unused.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <c>HRESULT</c></para>
    /// <para>If this method succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</para>
    /// </returns>
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-ienumexplorercommand-skip HRESULT Skip(
    // ULONG celt );
    [PreserveSig]
    HResult Skip([In] uint celt);

    /// <summary>Resets the enumeration to 0.</summary>
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-ienumexplorercommand-reset HRESULT Reset();
    void Reset();

    /// <summary>Not currently implemented.</summary>
    /// <returns>
    /// <para>Type: <c>IEnumExplorerCommand*</c></para>
    /// <para>Currently unused.</para>
    /// </returns>
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-ienumexplorercommand-clone HRESULT Clone(
    // IEnumExplorerCommand **ppenum );
    [return: MarshalAs(UnmanagedType.Interface)]
    IEnumExplorerCommand Clone();
  }
  [ComImport, Guid("64961751-0835-43c0-8ffe-d57686530e64"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IExplorerCommandProvider {
    /// <summary>Gets a specified Explorer command enumerator instance.</summary>
    /// <param name="punkSite">
    /// <para>Type: <c>IUnknown*</c></para>
    /// <para>A pointer to an interface used to set a site.</para>
    /// </param>
    /// <param name="riid">
    /// <para>Type: <c>REFIID</c></para>
    /// <para>A reference to the IID of the requested interface.</para>
    /// </param>
    /// <param name="ppv">
    /// <para>Type: <c>void**</c></para>
    /// <para>When this function returns, contains the interface pointer requested in riid. This will typically be IEnumExplorerCommand.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <c>HRESULT</c></para>
    /// <para>If this method succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</para>
    /// </returns>
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-iexplorercommandprovider-getcommands
    // HRESULT GetCommands( IUnknown *punkSite, REFIID riid, void **ppv );
    [PreserveSig]
    HResult GetCommands([MarshalAs(UnmanagedType.IUnknown)] object punkSite, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    // IExplorerCommand
    /// <summary>Gets a specified Explorer command instance.</summary>
    /// <param name="rguidCommandId">
    /// <para>Type: <c>REFGUID</c></para>
    /// <para>A reference to a command ID as a <c>GUID</c>. Used to obtain a command definition.</para>
    /// </param>
    /// <param name="riid">
    /// <para>Type: <c>REFIID</c></para>
    /// <para>A reference to the IID of the requested interface.</para>
    /// </param>
    /// <param name="ppv">
    /// <para>Type: <c>void**</c></para>
    /// <para>When this function returns, contains the interface pointer requested in riid. This will typically be IExplorerCommand.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <c>HRESULT</c></para>
    /// <para>If this method succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</para>
    /// </returns>
    // https://docs.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-iexplorercommandprovider-getcommand HRESULT
    // GetCommand( REFGUID rguidCommandId, REFIID riid, void **ppv );
    [PreserveSig]
    HResult GetCommand(in Guid rguidCommandId, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
  }
}
