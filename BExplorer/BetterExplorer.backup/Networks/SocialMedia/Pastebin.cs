using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace BetterExplorer.Networks
{
	class PastebinClient
	{
		/*
		 * A note to developers that plan to use this code: 
		 * Please provide your own Developer Key to use.
		 * 
		 * The biggest reason is so that we don't have other
		 * software projects posing as us. (Besides, users that
		 * are asked to authorize your app will be confused when
		 * it says "Better Explorer" instead of your app's name.)
		 * 
		 * It's free and easy to get a Developer Key at
		 * http://pastebin.com/api.
		 */
		private string ILoginURL = "http://pastebin.com/api/api_login.php";
		private string IPostURL = "http://pastebin.com/api/api_post.php";
		private string IDevKey = "18c90acb75912697299ac92a5190a1e1";

		/// <summary>
		/// Get the key used to identify this unique user. If it is null, then you are not logged in.
		/// </summary>
		public string UserKey { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Body">The body of the new paste. (Max size is 512 KB.)</param>
		/// <param name="Subj">The title of the paste. Not required.</param>
		/// <param name="Private">The privacy setting of the paste. 0 = Public, 1 = Unlisted, 2 = Private. Default is Public. Private is only availabe if a username and password were added. Not required.</param>
		/// <param name="Expire">The expiration time for this paste. Valid values are "N" (Never), "10M" (10 minutes), "1H" (1 hour), "1D" (1 day), "1W" (1 week), "2W" (2 weeks), "1M" (1 month). Default is "N". Not required.</param>
		/// <param name="Format">The syntax highligting format for this paste. Over 200 values are accepted. See http://pastebin.com/api#5 for more details. Default is "none". Not required.</param>
		/// <exception cref="ArgumentNullException">Thrown if the Body value is blank.</exception>
		/// <exception cref="ArgumentException">Thrown if Private is set to "2" and the user is not logged in.</exception>
		/// <exception cref="WebException">Thrown if an error occurred while making the paste.</exception>
		/// <returns>Returns the Uri for the new paste.</returns>
		public Uri Send(string Body, string Subj = "", string Private = "0", string Expire = "N", string Format = "none")
		{
			if (string.IsNullOrEmpty(Body.Trim())) throw new ArgumentNullException("Body", "Cannot have an empty paste.");

			var IQuery = new NameValueCollection()
			{
				{ "api_dev_key", IDevKey },
				{ "api_option", "paste" },
				{ "api_paste_code", Body }
			};

			//IQuery.Add("api_paste_private", "0");
			if (!string.IsNullOrEmpty(Private.Trim())) IQuery.Add("api_paste_private", Private);

			//IQuery.Add("api_paste_name", Subj);
			if (!string.IsNullOrEmpty(Subj.Trim())) IQuery.Add("api_paste_name", Subj);

			//IQuery.Add("api_paste_expire_date", "N");
			if (!string.IsNullOrEmpty(Expire.Trim())) IQuery.Add("api_paste_expire_date", Expire);

			//IQuery.Add("api_paste_format", Format);
			if (!string.IsNullOrEmpty(Format.Trim())) IQuery.Add("api_paste_format", Format);

			//IQuery.Add("api_user_key", IUserKey);
			if (!string.IsNullOrEmpty(UserKey.Trim()))
			{
				IQuery.Add("api_user_key", UserKey);
			}
			else if (Private == "2")
			{
				throw new ArgumentException("Cannot have a private paste while not logged in.", "Private");

			}

			using (var IClient = new WebClient())
			{
				string IResponse = Encoding.UTF8.GetString(IClient.UploadValues(IPostURL, IQuery));

				Uri isValid = null;
				if (!Uri.TryCreate(IResponse, UriKind.Absolute, out isValid))
				{
					throw new WebException("An error occurred while making the paste.", WebExceptionStatus.SendFailure);
				}

				return isValid;
			}
		}

		/// <summary>
		/// Creates a new PastebinSharp instance without logging in. Private posting is not allowed.
		/// </summary>
		public PastebinClient() { UserKey = null; }

		/// <summary>
		/// Creates a new PastebinSharp instance with a Username and Password, which allows private posting.
		/// </summary>
		/// <param name="Username">Pastebin.com username</param>
		/// <param name="Password">Pastebin.com password</param>
		public PastebinClient(string Username, string Password)
		{
			var IQuery = new NameValueCollection()
			{
				{ "api_dev_key", IDevKey },
				{ "api_user_name", Username },
				{ "api_user_password", Password }
			};

			using (var wc = new WebClient())
			{
				byte[] respBytes = wc.UploadValues(ILoginURL, IQuery);
				string resp = Encoding.UTF8.GetString(respBytes);

				if (resp.Contains("Bad API request"))
				{
					throw new WebException(resp, WebExceptionStatus.SendFailure);
				}

				UserKey = resp;
			}
		}
	}
}
