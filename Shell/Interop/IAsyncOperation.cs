using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BExplorer.Shell.Interop
{
	 #region IAsyncOperation

		[ComImport]

		[Guid("3D8B0590-F691-11d2-8EA9-006097DF5BD4")]

		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

		public interface IAsyncOperation
		{

				[PreserveSig]

				Int32 SetAsyncMode([MarshalAs(UnmanagedType.VariantBool)] Boolean DoOpAsync);

				[PreserveSig]

				Int32 GetAsyncMode([MarshalAs(UnmanagedType.VariantBool)] out Boolean IsOpAsync);

				[PreserveSig]

				Int32 StartOperation(IBindCtx bcReserved);

				[PreserveSig]

				Int32 InOperation([MarshalAs(UnmanagedType.VariantBool)] out Boolean InAsyncOp);

				[PreserveSig]

				Int32 EndOperation(UInt32 hResult, IBindCtx bcReserved, DragDropEffects Effects);

		}

		#endregion

		[ClassInterface(ClassInterfaceType.None)]
		public class AsyncDataObject : System.Windows.Forms.DataObject, IAsyncOperation
		{

				#region Constants

				protected const int S_OK = 0x00000000;

				protected const int S_FALSE = 0x00000001;

				#endregion

				#region Fields

				protected Boolean asynchMode;

				protected Boolean inAsyncOperation;

				#endregion

				#region Events

				public event EventHandler StartAsynchOperation;

				public event EventHandler StopAsynchOperation;

				#endregion

				#region Constructors

				public AsyncDataObject()

						: base()
				{

						this.asynchMode = false;

						this.inAsyncOperation = false;

				}

				public AsyncDataObject(System.Runtime.InteropServices.ComTypes.IDataObject data)

						: base(data)
				{

						this.asynchMode = false;

						this.inAsyncOperation = false;

				}



				public AsyncDataObject(String format, Object data)

						: base(format, data)
				{

						this.asynchMode = false;

						this.inAsyncOperation = false;

				}

				#endregion

				#region System.Runtime.InteropServices.ComTypes.IAsyncOperation Members

				public virtual Int32 SetAsyncMode(Boolean DoOpAsync)
				{

						this.asynchMode = DoOpAsync;

						return S_OK;

				}

				public virtual Int32 GetAsyncMode(out Boolean IsOpAsync)
				{

						IsOpAsync = this.asynchMode;

						return S_OK;

				}

				public virtual Int32 StartOperation(IBindCtx bcReserved)
				{
						this.inAsyncOperation = true;

						this.OnStartAsynchOperation();

						return S_OK;
				}



				public virtual Int32 InOperation(out Boolean InAsyncOp)
				{

						InAsyncOp = this.inAsyncOperation;

						return S_OK;

				}

				public virtual Int32 EndOperation(uint hResult, IBindCtx bcReserved, DragDropEffects Effects)
				{
						this.inAsyncOperation = false;

						this.OnStopAsynchOperation();

						return S_OK;
				}



				#endregion

				#region Events processings

				protected void OnStartAsynchOperation()
				{

						if (this.StartAsynchOperation != null)
						{

								this.StartAsynchOperation(this, new EventArgs());

						}

				}

				protected void OnStopAsynchOperation()
				{

						if (this.StopAsynchOperation != null)
						{

								this.StopAsynchOperation(this, new EventArgs());

						}

				}

				#endregion
		}
}
