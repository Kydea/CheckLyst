using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CheckListManager.Common.Logic.Services
{
	public class HashService : ServiceBase, IHashService
	{
		public byte[] HashString(string text)
		{
			if (text == null)
			{
				return null;
			}

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

			var computed = HashString(text);
			return hash.SequenceEqual(computed);
		}
	}
}