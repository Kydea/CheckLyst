using CheckListManager.Common.Core.Services;

namespace CheckListManager.Common.Logic.Services
{
	public interface IHashService : IService
	{
		byte[] HashString(string text);
		bool ValidateHash(string text, byte[] hash);
	}
}
