using System;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.AccessControl.Logic.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IUserSessionRepository _userSessionRepository;
		private readonly IHashService _hashService;

		public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository, IHashService hashService)
		{
			_userRepository = userRepository;
			_userSessionRepository = userSessionRepository;
			_hashService = hashService;
		}

		public async Task<Users> AuthenticateAsync(string code, string password)
		{
			if (string.IsNullOrEmpty(code))
			{
				throw new AuthenticationException(100, "User code not sent");
			}

			if (string.IsNullOrEmpty(password))
			{
				throw new AuthenticationException(101, "User password not sent");
			}

			var user = await _userRepository.GetAsync(code);

			if (user == null)
			{
				throw new AuthenticationException(102, $"User '{code}' not found");
			}
			if (!user.Enabled)
			{
				throw new AuthenticationException(103, $"User '{code}' not enabled");
			}
			if (!_hashService.ValidateHash(password, user.Password))
			{
				throw new AuthenticationException(104, "User password not valid");
			}

			//TODO: add LDAP / OAUTH SERVER

			return user;
		}

		public async Task<UserSessions> GetSessionAsync(string code)
		{
			var session = await _userSessionRepository.GetAsync(code);
			return session == null || session.ExpiresOn < DateTimeOffset.UtcNow ? null : session;
		}

		public async Task CreateSessionAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn)
		{
			var session = new UserSessions
			{
				Company = company,
				Code = code,
				User = user,
				Ticket = ticket,
				CreatedOn = DateTimeOffset.UtcNow,
				ExpiresOn = expiresOn
			};
			await _userSessionRepository.CreateAsync(session);
		}
	}
}