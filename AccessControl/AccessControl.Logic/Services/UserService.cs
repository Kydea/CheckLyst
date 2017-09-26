using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CheckListManager.AccessControl.Data;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Services;
using NLog;

namespace CheckListManager.AccessControl.Logic.Services
{
	public class UserService : IUserService
	{
		private const string IdentityCacheRegion = "UserService.IdentityToken";

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly IUserRepository _userRepository;
		private readonly IUserSessionRepository _userSessionRepository;
		private readonly IHashService _hashService;
		private readonly IRedisCache _cache;
		private readonly IMapper _mapper;

		public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository,
			IHashService hashService, IRedisCache cache, IMapper mapper)
		{
			_userRepository = userRepository;
			_userSessionRepository = userSessionRepository;
			_hashService = hashService;
			_cache = cache;
			_mapper = mapper;
		}

		public async Task<Users> AuthenticateAsync(long? companyId, string code, string password)
		{
			if (!companyId.HasValue)
			{
				throw new AuthenticationException(99, "Company not sent");
			}

			if (string.IsNullOrEmpty(code))
			{
				throw new AuthenticationException(100, "User code not sent");
			}

			if (string.IsNullOrEmpty(password))
			{
				throw new AuthenticationException(101, "User password not sent");
			}

			var user = await _userRepository.GetAsync(companyId.Value, code);

			if (user == null)
			{
				throw new AuthenticationException(102, $"User {code} not found");
			}
			if (!user.Enabled)
			{
				throw new AuthenticationException(103, $"User {code} not enabled");
			}
			if (!_hashService.ValidateHash(password, user.Password))
			{
				throw new AuthenticationException(104, "User password not valid");
			}

			//TODO: add LDAP / OAUTH SERVER

			return user;
		}

		public Task CreateSessionAsync(long company, string code, string user, string ticket, DateTimeOffset expiresOn)
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
			var identityToken = new IdentityToken(ticket, code, expiresOn);

			return Task.WhenAll(
				_userSessionRepository.CreateAsync(session),
				_cache.SetAsync(identityToken.RefreshToken, identityToken, IdentityCacheRegion)
			);
		}

		public async Task<IdentityToken> GetTokenAsync(string code)
		{
			var token = await _cache.GetAsync<IdentityToken>(code, IdentityCacheRegion);
			if (token != null)
			{
				return token.ExpiresOn >= DateTimeOffset.UtcNow ? token : null;
			}

			var session = await _userSessionRepository.GetAsync(code);
			if (session != null && session.ExpiresOn >= DateTimeOffset.UtcNow)
			{
				token = new IdentityToken(session.Ticket, session.Code, session.ExpiresOn);
				await _cache.SetAsync(token.RefreshToken, token, IdentityCacheRegion);
			}

			return token;
		}

		public Task UpdateTokenAsync(IdentityToken token, DateTimeOffset expiresOn)
		{
			token = new IdentityToken(token.AuthenticationTicket, token.RefreshToken, expiresOn);

			return Task.WhenAll(
				_cache.SetAsync(token.RefreshToken, token, IdentityCacheRegion),
				_userSessionRepository.SetExpiresOnAsync(token.RefreshToken, DateTimeOffset.UtcNow)
			);
		}

		public async Task LogoutAsync(string code)
		{
			if (!string.IsNullOrEmpty(code))
			{
				try
				{
					await Task.WhenAll(
						_cache.RemoveAsync(code, IdentityCacheRegion),
						_userSessionRepository.SetExpiresOnAsync(code, DateTimeOffset.UtcNow)
					);
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "Error during logout");
				}
			}
		}

		public async Task<PagingResponse<List<UsersResponse>>> GetUsersAsync(long companyId, PagingRequest<UsersRequest> request)
		{
			var skip = (request.CurrentPage - 1) * request.ItemsPerPage;
			var take = request.ItemsPerPage;
			var users = await _userRepository.GetUsersAsync(companyId, request.Data.Code, skip, take);
			var count = await _userRepository.CountUsersAsync(companyId, request.Data.Code);

			var response = users.Select(u => new UsersResponse
			{
				Code = u.Code,
				Email = u.Email,
				Enabled = u.Enabled
			}).ToList();

			var pageCount = (int)Math.Ceiling(count / (decimal)request.ItemsPerPage);

			return new PagingResponse<List<UsersResponse>>
			{
				Data = response,
				ItemsPerPage = request.ItemsPerPage,
				Count = count,
				CurrentPage = request.CurrentPage,
				PageCount = pageCount
			};
		}

		public async Task<AddUserResponse> AddUserAsync(long companyId, AddUserRequest request)
		{
			var user = _mapper.Map<Users>(request);

			user.Company = companyId;
			user.CreatedOn = DateTimeOffset.UtcNow;
			user.Password = _hashService.HashStringToBytes(request.Password);

			await _userRepository.AddAsync(user);

			return new AddUserResponse
			{
				Code = user.Code
			};
		}
	}
}