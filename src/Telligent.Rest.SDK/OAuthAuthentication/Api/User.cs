using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Telligent.Evolution.Extensibility.OAuthClient.Version1
{
	public class User
	{
		private object _syncRoot = new object();

		internal User(string userName, int userId, string languageKey)
		{
			UserName = userName;
			UserId = userId;
			LanguageKey = languageKey;
		}

		public string UserName { get; private set; }
		public int UserId { get; private set; }
		public string LanguageKey { get; private set; }
		public string OAuthToken { get; internal set; }
		internal string RefreshToken { get; set; }
		internal DateTime TokenExpiresUtc { get; set; }
		internal string SynchronizedUserName { get; set; }

		internal string Serialize(string signature)
		{
			var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signature));
			var data = Encoding.UTF8.GetBytes(string.Concat(
				Uri.EscapeDataString(UserName),
				"?",
				Uri.EscapeDataString(UserId.ToString("0")),
				"?",
				Uri.EscapeDataString(LanguageKey),
				"?",
				Uri.EscapeDataString(SynchronizedUserName ?? string.Empty),
				"?",
				Uri.EscapeDataString(OAuthToken),
				"?",
				Uri.EscapeDataString(RefreshToken),
				"?",
				Uri.EscapeDataString(TokenExpiresUtc.Ticks.ToString())
				));

			return string.Concat(
				Convert.ToBase64String(hmac.ComputeHash(data)),
				":",
				Convert.ToBase64String(data)
				);
		}

		static internal User Deserialize(string serializedUser, string signature)
		{
			var signatureAndMessage = serializedUser.Split(':');
			if (signatureAndMessage.Length != 2)
				return null;

			var hash = Convert.FromBase64String(signatureAndMessage[0]);
			var message = Convert.FromBase64String(signatureAndMessage[1]);
			var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signature));
			var validateHash = hmac.ComputeHash(message);
			bool valid = false;
			if (validateHash.Length == hash.Length)
			{
				for (int i = 0; i < hash.Length; i++)
				{
					if (hash[i] != validateHash[i])
						break;

					if (i == hash.Length - 1)
						valid = true;
				}
			}

			if (!valid)
				return null;

			string[] data = Encoding.UTF8.GetString(message).Split('?');
			if (data.Length != 7)
				return null;

			var user = new User(Uri.UnescapeDataString(data[0]), int.Parse(Uri.UnescapeDataString(data[1])), Uri.UnescapeDataString(data[2]));
			user.SynchronizedUserName = Uri.UnescapeDataString(data[3]);
			user.OAuthToken = Uri.UnescapeDataString(data[4]);
			user.RefreshToken = Uri.UnescapeDataString(data[5]);
			user.TokenExpiresUtc = new DateTime(long.Parse(Uri.UnescapeDataString(data[6])), DateTimeKind.Utc);

			return user;
		}

		internal static User Empty = new User(null, 0, null);

		internal object SyncRoot { get { return _syncRoot; } }

		public override bool Equals(object obj)
		{
			var user2 = obj as User;
			if (user2 == null)
				return false;

			return user2.UserName == null && this.UserName == null && user2.UserId == 0 && this.UserId == 0;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
