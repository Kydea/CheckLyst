using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CheckListManager.Common.Logic.Services
{
	public class HashService : IHashService
	{
		public byte[] HashStringToBytes(string text)
		{
			if (string.IsNullOrEmpty(text)) return null;

			using (var hash = new SHA256Managed())
			{
				var bytes = Encoding.UTF8.GetBytes(text);
				return hash.ComputeHash(bytes);
			}
		}

		public bool ValidateHash(string text, byte[] hash)
		{
			if (string.IsNullOrEmpty(text) || hash == null)
			{
				return false;
			}

			var computed = HashStringToBytes(text);
			return hash.SequenceEqual(computed);
		}
	}
}