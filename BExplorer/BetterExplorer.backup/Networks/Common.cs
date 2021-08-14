using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer.Networks {
	/// <summary>
	/// An event handler for NetworkItemEventArgs.
	/// </summary>
	public delegate void NetworkItemEventHandler(object sender, NetworkItemEventArgs e);

	public class DropBoxAuth {
		/*
		 * A note to developers that plan to use this code. 
		 * Please provide your own app key and secret to use.
		 * 
		 * The biggest reason is so that we don't have other
		 * software projects posing as us. (Besides, users that
		 * are asked to authorize your app will be confused when
		 * it says "Better Explorer" instead of your app's name.)
		 * 
		 * It's free and easy to apply for an app at
		 * https://www.dropbox.com/developers/apps/create.
		 */
		public static string Key = "h95fvxzzkjusao9";
		public static string Secret = "a00lskhbpcow9tw";
	}

	public class SkyDriveAuth {
		/*
		 * A note to developers that plan to use this code. 
		 * Please provide your own app key and secret to use.
		 * 
		 * The biggest reason is so that we don't have other
		 * software projects posing as us. (Besides, users that
		 * are asked to authorize your app will be confused when
		 * it says "Better Explorer" instead of your app's name.)
		 * 
		 * It's free and easy to apply for an app at
		 * https://account.live.com/developers/applications/.
		 */
		public static string Key = "000000004C0F9074";
		public static string Secret = "RV-kb8qkoDiif4bdhlCTDBIcjvAIKRHM";
		public static string[] scopes = new string[] { "wl.basic", "wl.photos", "wl.skydrive_update" };
	}

	/// <summary>
	/// What service is connected to a given NetworkAccount instance.
	/// </summary>
	public enum AccountService {
		/// <summary>
		/// An FTP server.
		/// </summary>
		FTP = 0,

		/// <summary>
		/// An FTPS server.
		/// </summary>
		FTPS = 1,

		/// <summary>
		/// A WebDAV server.
		/// </summary>
		WebDAV = 2,

		/// <summary>
		/// A Dropbox account.
		/// </summary>
		Dropbox = 3,

		/// <summary>
		/// A Box.net account.
		/// </summary>
		BoxDotNet = 4,

		/// <summary>
		/// A SkyDrive account.
		/// </summary>
		SkyDrive = 5,
	}

	/// <summary>
	/// The type of this NetworkAccount (i.e. server, online storage, or social media) which also defines what is allowed in terms of capabilities.
	/// </summary>
	public enum AccountType {
		/// <summary>
		/// A FTP or WebDAV server. It allows uploading files, and viewing/downloading files.
		/// </summary>
		Server = 0,

		/// <summary>
		/// An online storage service. It allows uploading files, and viewing/downloading files.
		/// </summary>
		OnlineStorage = 1,

		/// <summary>
		/// A social media service. It allows uploading files.
		/// </summary>
		SocialMedia = 2,
	}

	/// <summary>
	/// An EventArgs class for events that need to pass a NetworkItem.
	/// </summary>
	public class NetworkItemEventArgs {
		/// <summary>
		/// The NetworkItem passed by the event.
		/// </summary>
		public NetworkItem NetworkItem { get; private set; }

		public NetworkItemEventArgs(NetworkItem item = null) {
			NetworkItem = item;
		}
	}

}
