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
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using NLog;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Core.Web.Security;

namespace CheckListManager.AccessControl.Logic.Services
{
	public class UserService : IUserService
	{
		private const string IdentityCacheRegion = "UserService.IdentityToken";

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private readonly IUserRepository _userRepository;
		private readonly IUserSessionRepository _userSessionRepository;
		private readonly IHashService _hashService;
		private readonly IQueueService _queueService;
		private readonly IRedisCache _cache;
		private readonly IMapper _mapper;

		public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository,
			IHashService hashService, IQueueService queueService, IRedisCache cache, IMapper mapper)
		{
			_userRepository = userRepository;
			_userSessionRepository = userSessionRepository;
			_hashService = hashService;
			_queueService = queueService;
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

			await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
			{
				Company = user.Company,
				User = user.Code,
				Action = TrailAction.Login,
				Date = DateTimeOffset.UtcNow
			});

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
				_cache.SetAsync(identityToken.RefreshToken, identityToken, IdentityCacheRegion, expiresOn - DateTimeOffset.UtcNow)
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
				await _cache.SetAsync(token.RefreshToken, token, IdentityCacheRegion, session.ExpiresOn - DateTimeOffset.UtcNow);
			}

			return token;
		}

		public Task UpdateTokenAsync(IdentityToken token, DateTimeOffset expiresOn)
		{
			token = new IdentityToken(token.AuthenticationTicket, token.RefreshToken, expiresOn);

			return Task.WhenAll(
				_cache.SetAsync(token.RefreshToken, token, IdentityCacheRegion, expiresOn - DateTimeOffset.UtcNow),
				_userSessionRepository.SetExpiresOnAsync(token.RefreshToken, expiresOn)
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
			var take = request.ItemsPerPage;
			var users = await _userRepository.GetUsersAsync(companyId, request.Data.Code, request.Data.Description, request.Skip, take);
			var count = await _userRepository.CountUsersAsync(companyId, request.Data.Code, request.Data.Description);

			var response = users.Select(u => new UsersResponse
			{
				Code = u.Code,
                Description = u.Description,
                Email = u.Email,
				Enabled = u.Enabled,
                AuthenticationCode = u.AuthenticationCode,
                LastLoginOn = u.LastLoginOn,
                CreatedBy = u.CreatedBy,
                CreatedOn = u.CreatedOn,
                ModifiedBy = u.ModifiedBy,
                ModifiedOn = u.ModifiedOn
            }).ToList();

			return new PagingResponse<List<UsersResponse>>
			{
				Data = response,
				ItemsPerPage = request.ItemsPerPage,
				Count = count,
				CurrentPage = request.CurrentPage
			};
		}

        private UserResponse Validate(UserRequest record)
        {
            var response = new UserResponse();
            if (string.IsNullOrWhiteSpace(record.Code))
                response.Fields["Code"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid code"
                        }
                    }
                };

            if (string.IsNullOrWhiteSpace(record.Description))
                response.Fields["Description"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid value"
                        }
                    }
                };

            if (!string.IsNullOrEmpty(record.Password) && record.Password.Length < 8)
                response.Fields["Password"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Password must be longer then 8 character"
                        }
                    }
                };

            if (!(string.IsNullOrEmpty(record.Password) && string.IsNullOrEmpty(record.Password2)) && record.Password != record.Password2)
                response.Fields["Password2"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Password and confirm password must coincide"
                        }
                    }
                };

            response.Valid = response.Fields.Count() == 0;
            return response;
        }

        public async Task<UserResponse> LoadUserAsync(DefaultIdentity identity, UserRequest record)
        {
            var response = new UserResponse();
            try
            {
                var User = await _userRepository.GetAsync(identity.CompanyId, record.Code);

                response.GeneralFunctions = User.UserFunctions.Where(f => f.FunctionGroup != "Prototypes" && f.FunctionGroup != "Instances")
                    .Select(gf => new Function { functionGroup = gf.FunctionGroup, function = gf.Function })
                    .ToArray();

                response.PrototypeFunctions = User.UserFunctions.Where(f => f.FunctionGroup == "Prototypes")
                    .GroupBy(pf => pf.PrototypeType)
                    .Select(g => new Function {
                        department = g.First().Department,
                        prototypeType = g.Key,
                        functionGroup = "Prototypes",
                        functions = g.Select(pf => pf.Function).ToArray(),
                        level = g.First().Tag 
                    })
                    .ToArray();

                response.InstanceFunctions = User.UserFunctions.Where(f => f.FunctionGroup == "Instances")
                    .GroupBy(pf => pf.PrototypeType)
                    .Select(g => new Function
                    {
                        department = g.First().Department,
                        prototypeType = g.Key,
                        functionGroup = "Instances",
                        functions = g.Select(pf => pf.Function).ToArray(),
                        level = g.First().Tag
                    })
                    .ToArray();
            }
            catch (Exception Exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = Exception.Message
                });
                response.Valid = false;
            }

            return response;
        }

        private List<UserFunctions> BuildFunctions(DefaultIdentity identity, UserRequest record)
        {
            var List = new List<UserFunctions>();
            foreach (var Function in record.GeneralFunctions)
                List.Add(new UserFunctions
                {
                    FunctionGroup = Function.functionGroup,
                    Function = Function.function,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedBy = identity.Code
                });

            foreach (var Row in record.PrototypeFunctions)
                foreach (var Function in Row.functions)
                    List.Add(new UserFunctions
                    {
                        Department = Row.department,
                        PrototypeType = Row.prototypeType,
                        FunctionGroup = "Prototypes",
                        Function = Function,
                        Tag = Row.level,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CreatedBy = identity.Code
                    });

            foreach (var Row in record.InstanceFunctions)
                foreach (var Function in Row.functions)
                    List.Add(new UserFunctions
                    {
                        Department = Row.department,
                        PrototypeType = Row.prototypeType,
                        FunctionGroup = "Instances",
                        Function = Function,
                        Tag = Row.level,
                        CreatedOn = DateTimeOffset.UtcNow,
                        CreatedBy = identity.Code
                    });

            return List;
        }

        public async Task<UserResponse> AddUserAsync(DefaultIdentity identity, UserRequest record)
		{
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _userRepository.AddAsync(identity, new Users
                    {
                        Code = record.Code,
                        Description = record.Description,
                        Email = record.Email,
                        Password = _hashService.HashStringToBytes(record.Password),
                        AuthenticationCode = record.AuthenticationCode,
                        Enabled = record.Enabled,
                        
                        UserFunctions = BuildFunctions(identity, record)
                    });
                }
                catch (Exception Exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = Exception.Message
                    });
                    response.Valid = false;
                }

            return response;
		}

        public async Task<UserResponse> EditUserAsync(DefaultIdentity identity, UserRequest record)
        {
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _userRepository.EditAsync(identity, new Users
                    {
                        Code = record.Code,
                        Description = record.Description,
                        Email = record.Email,
                        Password = _hashService.HashStringToBytes(record.Password),
                        Enabled = record.Enabled,
                        AuthenticationCode = record.AuthenticationCode,

                        UserFunctions = BuildFunctions(identity, record)
                    });
                }
                catch (Exception Exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = Exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<UserResponse> DeleteUserAsync(DefaultIdentity identity, string code)
        {
            var response = new UserResponse();
            try
            {
                await _userRepository.DeleteAsync(identity.CompanyId, code);
            }
            catch (Exception Exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = Exception.Message
                });
                response.Valid = false;
            }

            return response;
        }
    }
}