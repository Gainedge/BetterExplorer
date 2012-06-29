using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	using Win32Structs;

	using HANDLE = System.IntPtr;
	using DWORD = System.UInt32;
	using BOOL = System.Int32;
	using LPVOID = System.IntPtr;
	using PSID = System.IntPtr;

	/// <summary>
	/// Summary description for Sid.
	/// </summary>
	public class Sid
	{
		#region Static methods
		public const string UnknownAccountName = "<unknown>";

		public static Sid Create(
			SID_IDENTIFIER_AUTHORITY identifierAuthority,
			params UInt32 []subAuthorities)
		{
			return UnsafeCreateSid("", identifierAuthority, subAuthorities);
		}
		public static Sid Create(
			string machineName,
			SID_IDENTIFIER_AUTHORITY identifierAuthority,
			params UInt32 []subAuthorities)
		{
			return UnsafeCreateSid(machineName, identifierAuthority, subAuthorities);
		}
		#endregion

		private readonly byte[] _psid;
		private readonly string _machineName;

		private string _domainName;
		private string _accountName;
		private string _sidString;


		public Sid(PSID psid, string machineName)
		{
			_machineName = machineName;
			_psid = CopySid(psid);
		}
		public Sid(string userName, string machineName)
		{
			_machineName = machineName;
			_psid = LookupSid(userName, machineName);
		}
		public Sid(PSID psid) : this(psid, "")
		{
		}
		public Sid(string userName) : this(userName, "")
		{
		}
		private unsafe byte[] LookupSid(string userName, string machineName)
		{
			DWORD cbSidSize = 0;
			DWORD cbDName = 0;
			SID_NAME_USE nameUse;
			BOOL rc = Win32.LookupAccountName(
				machineName, userName, IntPtr.Zero, ref cbSidSize, null, ref cbDName, out nameUse);
			switch(Win32.GetLastError())
			{
				case Win32.SUCCESS:
					throw new InvalidOperationException("Unexpected return code from LookupAccountName");

				case Win32.ERROR_INSUFFICIENT_BUFFER:
					char[] dname = new char[cbDName];
					byte[] psidBytes = new byte[cbSidSize];
					fixed(byte *psidPtr = psidBytes)
					{
						rc = Win32.LookupAccountName(
							machineName, userName, (IntPtr)psidPtr, ref cbSidSize, dname, ref cbDName, out nameUse);
						Win32.CheckCall(rc);
					}
					return psidBytes;

				default:
					Win32.ThrowLastError();
					return null; // never called
			}

		}
		private byte[] CopySid(PSID psid)
		{
			BOOL rc = Win32.IsValidSid(psid);
			Win32.CheckCall(rc);

			DWORD cbLength = Win32.GetLengthSid(psid);

			byte[] psidBytes = new byte[cbLength];
			Marshal.Copy(psid, psidBytes, 0, (int)cbLength);
			return psidBytes;
		}
		private void ThrowIfInvalid()
		{
			if (!IsValid)
			{
				throw new ArgumentException("SID structure is not valid");
			}
		}
		#region Private unsafe methods
		private unsafe void UnsafeGetAccountAndDomainName()
		{
			// Already done...
			if (_accountName != null)
				return;

			fixed(byte *psid = _psid)
			{
				IntPtr psidPtr = (IntPtr)psid;

				DWORD cchDLen = 0;
				DWORD cchALen = 0;
				SID_NAME_USE nameUse;

				// First, we ask for the length
				BOOL rc = Win32.LookupAccountSid(
					_machineName,
					psidPtr,
					null,
					ref cchALen,
					null,
					ref cchDLen,
					out nameUse);

				switch(Marshal.GetLastWin32Error())
				{
					case Win32.SUCCESS:
						throw new ArgumentException("Can't get account name length (unexpected return code from LookupAccountSidW)");

					case Win32.ERROR_NONE_MAPPED:
						_accountName = UnknownAccountName;
						_domainName = null;
						break;

					case Win32.ERROR_INSUFFICIENT_BUFFER:
						// Then we fetch the strings
						char[] DStr = new char[cchDLen];
						char[] AStr = new char[cchALen];
						rc = Win32.LookupAccountSid(
							_machineName,
							psidPtr,
							AStr,
							ref cchALen,
							DStr,
							ref cchDLen,
							out nameUse);
						Win32.CheckCall(rc);

						_domainName = new string(DStr, 0, (int)cchDLen);
						_accountName = new string(AStr, 0, (int)cchALen);
						break;

					default:
						Win32.ThrowLastError();
						break;
				}

				if (_domainName == null)
					_domainName = "";
			}

		}
		private unsafe void UnsafeGetSidString()
		{
			if (_sidString != null)
				return;

			IntPtr strPtr;
			fixed(byte *psid = _psid)
			{
				IntPtr psidPtr = (IntPtr)psid;

				BOOL rc = Win32.ConvertSidToStringSid(psidPtr, out strPtr);
				Win32.CheckCall(rc);
				try
				{
					_sidString = Marshal.PtrToStringAuto(strPtr);

				}
				finally
				{
					Win32.LocalFree(strPtr);
				}
			}
		}
		private unsafe bool UnsafeCheckIsValid()
		{
			if (_psid == null)
				return false;

			fixed(byte *psid = _psid)
			{
				BOOL rc = Win32.IsValidSid((IntPtr)psid);
				return (rc != Win32.FALSE);
			}
		}
		private static unsafe bool UnsafeEqualsSids(Sid s1, Sid s2)
		{
            if (object.ReferenceEquals(s1, null) && object.ReferenceEquals(s2, null))
                return true;

			if (object.ReferenceEquals(s1, null) || object.ReferenceEquals(s2, null))
				return false;

			if (!s1.IsValid)
				return false;

			if (!s2.IsValid)
				return false;

			fixed(byte *psid1 = s1._psid, psid2 = s2._psid)
			{
				BOOL rc = Win32.EqualSid((IntPtr)psid1, (IntPtr)psid2);
				return (rc != Win32.FALSE);
			}
		}
		private static unsafe bool UnsafeEqualsPrefixSids(Sid s1, Sid s2)
		{
            if (object.ReferenceEquals(s1, null) && object.ReferenceEquals(s2, null))
                return true;

			if (object.ReferenceEquals(s1, null) || object.ReferenceEquals(s2, null))
				return false;

			if (!s1.IsValid)
				return false;

			if (!s2.IsValid)
				return false;

			fixed(byte *psid1 = s1._psid, psid2 = s2._psid)
			{
				IntPtr psid1Ptr = (IntPtr)psid1;
				IntPtr psid2Ptr = (IntPtr)psid2;

				BOOL rc = Win32.EqualPrefixSid(psid1Ptr, psid2Ptr);
				return (rc != Win32.FALSE);
			}
		}
		private unsafe int UnsafeGetSidLength()
		{
			ThrowIfInvalid();
			fixed(byte *psid = this._psid)
			{
				IntPtr psidPtr = (IntPtr)psid;

				return (int)Win32.GetLengthSid(psidPtr);
			}
		}

		private unsafe int UnsafeGetSubAuthorityCount()
		{
			ThrowIfInvalid();

			fixed(byte *psid = _psid)
			{
				IntPtr psidPtr = (IntPtr)psid;
				
				IntPtr pbyteCount = Win32.GetSidSubAuthorityCount(psidPtr);
				Win32.CheckCall(Marshal.GetLastWin32Error());
				return Marshal.ReadByte(pbyteCount);
			}			
		}
		private unsafe UInt32 UnsafeGetSubAuthority(int index)
		{
			ThrowIfInvalid();

			fixed(byte *psid = _psid)
			{
				IntPtr psidPtr = (IntPtr)psid;
				
				IntPtr pdwSubAuth = Win32.GetSidSubAuthority(psidPtr, (UInt32)index);
				Win32.CheckCall(Marshal.GetLastWin32Error());
				return (uint)Marshal.ReadInt32(pdwSubAuth);
			}			
		}
		private unsafe SID_IDENTIFIER_AUTHORITY UnsafeGetSID_IDENTIFIER_AUTHORITY()
		{
			ThrowIfInvalid();

			fixed(byte *psid = _psid)
			{
				IntPtr psidPtr = (IntPtr)psid;
				
				IntPtr pAuth = Win32.GetSidIdentifierAuthority(psidPtr);
				Win32.CheckCall(Marshal.GetLastWin32Error());
				return (SID_IDENTIFIER_AUTHORITY)Marshal.PtrToStructure(pAuth, typeof(SID_IDENTIFIER_AUTHORITY));
			}			
		}
		/// <summary>
		///  Create a SID blob given an Authority and a variable array of sub authorities.
		/// </summary>
		/// <param name="IdentifierAuthority"></param>
		/// <param name="SubAuthorities"></param>
		/// <returns>The Sid object</returns>
		public unsafe static Sid UnsafeCreateSid(
			string machineName,
			SID_IDENTIFIER_AUTHORITY IdentifierAuthority,
			params UInt32 []SubAuthorities)
		{
			if (SubAuthorities.Length >= 255)
				throw new ArgumentException("Too many sub authorities", "SubAuthorities");

			byte nSubAuth = (byte)SubAuthorities.Length;
			UInt32 cbLength = Win32.GetSidLengthRequired(nSubAuth);
			byte[] sid = new byte[cbLength];
			fixed(byte *psid = sid)
			{
				IntPtr psidPtr = (IntPtr)psid;
				BOOL rc = Win32.InitializeSid(psidPtr, ref IdentifierAuthority, nSubAuth);
				Win32.CheckCall(rc);

				for(byte i = 0; i < nSubAuth; i++)
				{
					IntPtr ridPtr = Win32.GetSidSubAuthority(psidPtr, i);
					Marshal.WriteInt32(ridPtr, (int)SubAuthorities[i]);
				}
				return new Sid(psidPtr, machineName);
			}
		}
		#endregion

		public string DomainName
		{
			get
			{
				UnsafeGetAccountAndDomainName();
				return _domainName;
			}
		}
		public string AccountName
		{
			get
			{
				UnsafeGetAccountAndDomainName();
				return _accountName;
			}
		}
		public string CanonicalName
		{
			get
			{
				string dname = DomainName;
				string aname = AccountName;
				if (dname == "")
					return aname;
				else
					return dname + @"\" + aname;
			}
		}
		public string SidString
		{
			get
			{
				UnsafeGetSidString();
				return _sidString;
			}
		}
		public bool IsValid
		{
			get
			{
				return UnsafeCheckIsValid();
			}
		}
		public int Size
		{
			get
			{
				return UnsafeGetSidLength();
			}
		}
		public int GetSubAuthorityCount()
		{
			return UnsafeGetSubAuthorityCount();
		}
		public UInt32 GetSubAuthority(int index)
		{
			return UnsafeGetSubAuthority(index);
		}
		internal SID_IDENTIFIER_AUTHORITY GetSID_IDENTIFIER_AUTHORITY()
		{
			return UnsafeGetSID_IDENTIFIER_AUTHORITY();
		}
		internal byte[] GetNativeSID()
		{
			ThrowIfInvalid();
			return _psid;
		}
		public override bool Equals(object obj)
		{
			Sid other = obj as Sid;
			if (object.ReferenceEquals(other, null))
				return base.Equals (obj);

			return UnsafeEqualsSids(this, other);
		}
		public override int GetHashCode()
		{
			return SidString.GetHashCode();
		}
		public static bool operator==(Sid s1, Sid s2)
		{
			return UnsafeEqualsSids(s1, s2);
		}
		public static bool operator!=(Sid s1, Sid s2)
		{
			return !UnsafeEqualsSids(s1, s2);
		}
		public bool EqualsPrefix(Sid other)
		{
			return UnsafeEqualsPrefixSids(this, other);
		}

	}
}
