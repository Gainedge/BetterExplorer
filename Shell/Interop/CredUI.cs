using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace BExplorer.Shell.Interop
{
	public static class CredUI
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CREDUI_INFO
		{
			public int cbSize;
			public IntPtr hwndParent;
			public string pszMessageText;
			public string pszCaptionText;
			public IntPtr hbmBanner;
		}

		[Flags]
		public enum CREDUI_FLAGS
		{
			INCORRECT_PASSWORD = 0x1,
			DO_NOT_PERSIST = 0x2,
			REQUEST_ADMINISTRATOR = 0x4,
			EXCLUDE_CERTIFICATES = 0x8,
			REQUIRE_CERTIFICATE = 0x10,
			SHOW_SAVE_CHECK_BOX = 0x40,
			ALWAYS_SHOW_UI = 0x80,
			REQUIRE_SMARTCARD = 0x100,
			PASSWORD_ONLY_OK = 0x200,
			VALIDATE_USERNAME = 0x400,
			COMPLETE_USERNAME = 0x800,
			PERSIST = 0x1000,
			SERVER_CREDENTIAL = 0x4000,
			EXPECT_CONFIRMATION = 0x20000,
			GENERIC_CREDENTIALS = 0x40000,
			USERNAME_TARGET_CREDENTIALS = 0x80000,
			KEEP_USERNAME = 0x100000,
		}

		public enum CredUIReturnCodes
		{
			NO_ERROR = 0,
			ERROR_CANCELLED = 1223,
			ERROR_NO_SUCH_LOGON_SESSION = 1312,
			ERROR_NOT_FOUND = 1168,
			ERROR_INVALID_ACCOUNT_NAME = 1315,
			ERROR_INSUFFICIENT_BUFFER = 122,
			ERROR_INVALID_PARAMETER = 87,
			ERROR_INVALID_FLAGS = 1004,
		}

		[DllImport("credui")]
		public static extern CredUIReturnCodes CredUIPromptForCredentials(ref CREDUI_INFO creditUR,
			string targetName,
			IntPtr reserved1,
			int iError,
			StringBuilder userName,
			int maxUserName,
			StringBuilder password,
			int maxPassword,
			[MarshalAs(UnmanagedType.Bool)] ref bool pfSave,
			CREDUI_FLAGS flags);

		[DllImport("credui.dll", CharSet = CharSet.Auto)]
		public static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
																															 IntPtr pAuthBuffer,
																															 uint cbAuthBuffer,
																															 StringBuilder pszUserName,
																															 ref int pcchMaxUserName,
																															 StringBuilder pszDomainName,
																															 ref int pcchMaxDomainame,
																															 StringBuilder pszPassword,
																															 ref int pcchMaxPassword);

		[DllImport("credui.dll", CharSet = CharSet.Auto)]
		public static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
																																 int authError,
																																 ref uint authPackage,
																																 IntPtr InAuthBuffer,
																																 uint InAuthBufferSize,
																																 out IntPtr refOutAuthBuffer,
																																 out uint refOutAuthBufferSize,
																																 ref bool fSave,
																																 int flags);



		public static void RunProcesssAsUser(String processPath)
		{
			// Setup the flags and variables
			StringBuilder userPassword = new StringBuilder(), userID = new StringBuilder();
			CREDUI_INFO credUI = new CREDUI_INFO();
			credUI.pszCaptionText = "Please enter the credentails for " + new ShellItem(processPath).DisplayName;
			credUI.pszMessageText = "DisplayedMessage";
			credUI.cbSize = Marshal.SizeOf(credUI);
			uint authPackage = 0;
			IntPtr outCredBuffer = new IntPtr();
			uint outCredSize;
			bool save = false;
			int result = CredUIPromptForWindowsCredentials(ref credUI,
																								 0,
																								 ref authPackage,
																								 IntPtr.Zero,
																								 0,
																								 out outCredBuffer,
																								 out outCredSize,
																								 ref save,
																								 1 /* Generic */);

			var usernameBuf = new StringBuilder(100);
			var passwordBuf = new StringBuilder(100);
			var domainBuf = new StringBuilder(100);

			int maxUserName = 100;
			int maxDomain = 100;
			int maxPassword = 100;
			if (result == 0)
			{
				if (CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
																					 domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
				{
					//TODO: ms documentation says we should call this but i can't get it to work
					//SecureZeroMem(outCredBuffer, outCredSize);

					//clear the memory allocated by CredUIPromptForWindowsCredentials 
					Ole32.CoTaskMemFree(outCredBuffer);

					SecureString pass = new SecureString();
					foreach (char _char in passwordBuf.ToString().ToCharArray())
					{
						pass.AppendChar(_char);
					}

					using (Process p = new Process())
					{
						p.StartInfo.UseShellExecute = true;
						p.StartInfo.WorkingDirectory = Path.GetDirectoryName(processPath);
						p.StartInfo.FileName = processPath;
						p.StartInfo.UserName = usernameBuf.ToString();
						p.StartInfo.Password = pass;
						p.StartInfo.Domain = domainBuf.ToString();
						p.StartInfo.UseShellExecute = false;
						p.Start();
					}
				}
			}




		}
	}
}
