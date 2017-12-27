using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using CheckListManager.Common.Logic.Email;
using Newtonsoft.Json;

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
		private readonly ISettingService _settingService;

		public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository,
			IHashService hashService, IQueueService queueService, IRedisCache cache, ISettingService settingService)
		{
			_userRepository = userRepository;
			_userSessionRepository = userSessionRepository;
			_hashService = hashService;
			_queueService = queueService;
			_cache = cache;
			_settingService = settingService;
		}

		public async Task<Users> AuthenticateAsync(long? companyId, string code, string password)
		{
            if (!companyId.HasValue)
                throw new AuthenticationException(99, "Company not sent");

            if (string.IsNullOrEmpty(code))
                throw new AuthenticationException(100, "User code not sent");

            Users user = null;

            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new AuthenticationException(101, "User password not sent");

                user = await _userRepository.GetAsync(companyId.Value, code);

                if (user == null)
                    throw new AuthenticationException(102, $"User {code} not found");

                if (!user.Enabled)
                    throw new AuthenticationException(103, $"User {code} not enabled");

	            if (
					user.Companies.MaximumNumberOfPasswordFailure > 0
					&& user.NumberOfAttempts >= user.Companies.MaximumNumberOfPasswordFailure
					&& user.LastLoginFailedOn != null
					&& user.Companies.LoginFailedTimeout > 0
					&& (DateTimeOffset.UtcNow - user.LastLoginFailedOn.Value).TotalMinutes < user.Companies.LoginFailedTimeout
				)
		            throw new AuthenticationException(104, $"User {code} temporary disabled");

				if (!_hashService.ValidateHash(password, user.Password))
                    throw new AuthenticationException(105, "User password not valid");

	            await Task.WhenAll(
		            _userRepository.UserLoginCompleteAsync(user),
			        _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
			        {
				        Company = user.Company,
				        User = user.Code,
				        Action = TrailAction.Login,
				        Date = DateTimeOffset.UtcNow
			        })
	            );

				//TODO: add LDAP / OAUTH SERVER
			}
			catch (AuthenticationException authenticationException)
			{
				await Task.WhenAll(
					_userRepository.UserLoginFailedAsync(user),
					_queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
					{
						Company = companyId.Value,
						User = code,
						Action = TrailAction.LoginFailed,
						Date = DateTimeOffset.UtcNow,
						Information = authenticationException.Message
					})
				);

                throw;
            }

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

		public async Task LogoutAsync(DefaultIdentity identity, string token)
		{
			if (!string.IsNullOrEmpty(token))
			{
				try
				{
					await Task.WhenAll(
						_cache.RemoveAsync(token, IdentityCacheRegion),
						_userSessionRepository.SetExpiresOnAsync(token, DateTimeOffset.UtcNow)
					);
                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.Logout,
                        Date = DateTimeOffset.UtcNow
                    });
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

            response.Valid = !response.Fields.Any();
            return response;
        }

        public async Task<UserResponse> LoadUserAsync(DefaultIdentity identity, UserRequest record)
        {
            var response = new UserResponse();
            try
            {
                var user = await _userRepository.GetAsync(identity.CompanyId, record.Code);

                response.GeneralFunctions = user.UserFunctions.Where(f => f.FunctionGroup != "Prototypes" && f.FunctionGroup != "Instances")
                    .Select(gf => new Function { functionGroup = gf.FunctionGroup, function = gf.Function })
                    .ToArray();

                response.PrototypeFunctions = user.UserFunctions.Where(f => f.FunctionGroup == "Prototypes")
                    .GroupBy(pf => pf.PrototypeType)
                    .Select(g => new Function {
                        department = g.First().Department,
                        prototypeType = g.Key,
                        functionGroup = "Prototypes",
                        functions = g.Select(pf => pf.Function).ToArray(),
                        level = g.First().Tag 
                    })
                    .ToArray();

                response.InstanceFunctions = user.UserFunctions.Where(f => f.FunctionGroup == "Instances")
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
            catch (Exception exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = exception.Message
                });
                response.Valid = false;
            }

            return response;
        }

        private void BuildFunctions(DefaultIdentity identity, UserRequest record, Users User)
        {
            foreach (var function in record.GeneralFunctions)
                User.UserFunctions.Add(new UserFunctions
                {
                    Company = identity.CompanyId,
                    User = record.Code,
                    FunctionGroup = function.functionGroup,
                    Function = function.function,
                    CreatedOn = DateTimeOffset.UtcNow,
                    CreatedBy = identity.Code
                });

            foreach (var row in record.PrototypeFunctions)
                if (row.functions != null)
                    foreach (var function in row.functions)
                        User.UserFunctions.Add(new UserFunctions
                        {
                            Company = identity.CompanyId,
                            User = record.Code,
                            Department = row.department,
                            PrototypeType = row.prototypeType,
                            FunctionGroup = "Prototypes",
                            Function = function,
                            Tag = row.level,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });

            foreach (var row in record.InstanceFunctions)
                if (row.functions != null)
                    foreach (var function in row.functions)
                        User.UserFunctions.Add(new UserFunctions
                        {
                            Company = identity.CompanyId,
                            User = record.Code,
                            Department = row.department,
                            PrototypeType = row.prototypeType,
                            FunctionGroup = "Instances",
                            Function = function,
                            Tag = row.level,
                            CreatedOn = DateTimeOffset.UtcNow,
                            CreatedBy = identity.Code
                        });
        }

        private static string SerializeUserInformation(string code, string authenticationCode, IEnumerable<UserFunctions> userFunctions)
        {
            return JsonConvert.SerializeObject(new
            {
                Code = code,
                AuthenticationCode = authenticationCode,
                Functions = userFunctions.Select(uf => new
                {
	                uf.FunctionGroup,
	                uf.Function,
                    Department = uf.Department + " " + uf.Departments?.Name,
                    PrototypeType = uf.PrototypeType + " " + uf.PrototypeTypes?.Name,
	                uf.Tag
                })
            });
        }

        public async Task<UserResponse> AddUserAsync(DefaultIdentity identity, UserRequest record)
		{
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    var User = new Users
                    {
                        Code = record.Code,
                        Description = record.Description,
                        Email = record.Email,
                        Password = _hashService.HashStringToBytes(record.Password),
                        AuthenticationCode = record.AuthenticationCode,
                        Enabled = record.Enabled
                    };

                    BuildFunctions(identity, record, User);

                    await _userRepository.AddAsync(identity, User);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.UserCreated,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeUserInformation(record.Code, record.AuthenticationCode, User.UserFunctions)
                    });
                }
                catch (Exception exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = exception.Message
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
                    var User = new Users
                    {
                        Code = record.Code,
                        Description = record.Description,
                        Email = record.Email,
                        Password = _hashService.HashStringToBytes(record.Password),
                        Enabled = record.Enabled,
                        AuthenticationCode = record.AuthenticationCode
                    };

                    BuildFunctions(identity, record, User);

                    await _userRepository.EditAsync(identity, User);

                    await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                    {
                        Company = identity.CompanyId,
                        User = identity.Code,
                        Action = TrailAction.UserEdited,
                        Date = DateTimeOffset.UtcNow,
                        Information = SerializeUserInformation (record.Code, record.AuthenticationCode, User.UserFunctions)
                    });
                }
                catch (Exception exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = exception.Message
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
                if (identity.Code == code)
                    throw new Exception("Users cannot delete themselves.");

                await _userRepository.DeleteAsync(identity.CompanyId, code);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.UserDeleted,
                    Date = DateTimeOffset.UtcNow,
                    Information = code
                });
            }
            catch (Exception exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = exception.Message
                });
                response.Valid = false;
            }

            return response;
        }

		public async Task CheckInactiveUsers(CancellationToken cancellationToken = new CancellationToken())
		{
			try
			{
				var users = await _userRepository.GetUsersToDisableAsync();
				foreach (var user in users)
				{
					user.Enabled = false;
					if (!string.IsNullOrEmpty(user.Email))
						await _queueService.SendObjectAsync(QueueName.Email,
							new EmailMessage(user.Company, "User disabled", await _settingService.UserBlockMessageAsync(user.Company),
								user.Email, user.Description), cancellationToken);
				}

				await _userRepository.SaveChangesAsync();
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}
			try
			{
				var users = await _userRepository.GetUsersDisableWarningAsync();
				foreach (var user in users)
				{
					if (!string.IsNullOrEmpty(user.Email))
						await _queueService.SendObjectAsync(QueueName.Email,
							new EmailMessage(user.Company, "User disabled soon", await _settingService.UserBlockWarningMessageAsync(user.Company),
								user.Email, user.Description), cancellationToken);
				}
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}
		}

		public async Task CheckUnchangedPins(CancellationToken cancellationToken = new CancellationToken())
		{
			try
			{
				var users = await _userRepository.GetUsersWithExpiredPin();
				foreach (var user in users)
					user.PIN = null;

				await _userRepository.SaveChangesAsync();
			}
			catch (Exception e)
			{
				Logger.Error(e);
			}
		}
	}
}