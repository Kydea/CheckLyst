using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public interface IHashService : ISingletonService
	{
		byte[] HashStringToBytes(string text);
		string HashStringToString(string text);
		bool ValidateHash(string text, byte[] hash);
	}
}
