using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Data;
using CheckListManager.AccessControl.Logic.Authentication;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Messages.Users;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.Email;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
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
        private readonly IQueueService _queueService;
        private readonly IRedisCache _cache;
        private readonly ISettingService _settingService;
        private readonly IEmailServiceBase _emailService;

        public UserService(IUserRepository userRepository, IUserSessionRepository userSessionRepository,
            IHashService hashService, IQueueService queueService, IRedisCache cache, ISettingService settingService,
            IConfigurationService configurationService, IEmailServiceBase emailService)
        {
            _userRepository = userRepository;
            _userSessionRepository = userSessionRepository;
            _hashService = hashService;
            _queueService = queueService;
            _cache = cache;
            _settingService = settingService;
            _emailService = emailService;
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
                    throw new AuthenticationException(103, $"User {code} disabled");

                var LDAPHost = await _settingService.LDAPHostAsync(companyId.Value);

                if (!string.IsNullOrEmpty(LDAPHost) && !string.IsNullOrEmpty(user.AuthenticationCode))
                    VerifyLDAPPassword(LDAPHost, user.AuthenticationCode, password);
                else if (!_hashService.ValidateHash(password, user.Password))
                    throw new AuthenticationException(105, "Password not valid");

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
            }
            catch (AuthenticationException authenticationException)
            {
                await Task.WhenAll(
                    _userRepository.UserPasswordFailedAsync(user),
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

        private static void VerifyLDAPPassword(string LDAPHost, string Code, string Password)
        {
            try
            {
                var LdapDirectoryIdentifier =
                    new LdapDirectoryIdentifier(LDAPHost, 636, true, false); //ldap: 389, ldaps: 636
                var NetworkCredential = new NetworkCredential(Code, Password);
                var LdapConnection = new LdapConnection(LdapDirectoryIdentifier, NetworkCredential, AuthType.Basic);

                LdapConnection.SessionOptions.SecureSocketLayer = true;
                LdapConnection.SessionOptions.ProtocolVersion = 3;
                LdapConnection.SessionOptions.VerifyServerCertificate = (connection, certificate) => true;
                LdapConnection.Bind();
            }
            catch (Exception Exception)
            {
                throw new AuthenticationException(104, "LDAP Error. " + Exception.Message);
            }
        }

        public async Task SetPINAsync(long? companyId, string code, string password, string pin)
        {
            if (!companyId.HasValue)
                throw new AuthenticationException(99, "Company not sent");

            if (string.IsNullOrEmpty(code))
                throw new AuthenticationException(100, "User code not sent");

            if (string.IsNullOrEmpty(pin) || !Regex.IsMatch(pin, @"^[0-9]+$"))
                throw new AuthenticationException(106, "Invalid PIN");

            if (pin.Length < 6)
                throw new AuthenticationException(107, "PIN must be at least 6 characters long");

            await AuthenticateAsync(companyId, code, password);
            await _userRepository.SetPINAsync(companyId.Value, code, _hashService.HashStringToBytes(pin));
        }

        public async Task AuthenticateWithPINAsync(long? companyId, string code, string pin)
        {
            if (!companyId.HasValue)
                throw new AuthenticationException(99, "Company not sent");

            if (string.IsNullOrEmpty(code))
                throw new AuthenticationException(100, "User code not sent");

            Users user = null;

            try
            {
                if (string.IsNullOrEmpty(pin))
                    throw new AuthenticationException(101, "User PIN not sent");

                user = await _userRepository.GetAsync(companyId.Value, code);

                if (user == null)
                    throw new AuthenticationException(102, $"User {code} not found");

                if (!user.Enabled)
                    throw new AuthenticationException(103, $"User {code} disabled");

                if (string.IsNullOrEmpty(pin) || !Regex.IsMatch(pin, @"^[0-9]+$"))
                    throw new AuthenticationException(106, $"PIN not valid");

                if (!_hashService.ValidateHash(pin, user.PIN))
                    throw new AuthenticationException(105, "User pin not valid");

                user.NumberOfPINAttempts = 0;
                await _userRepository.SaveChangesAsync();
            }
            catch (AuthenticationException)
            {
                await _userRepository.UserPINFailedAsync(user);
                throw;
            }
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
                _cache.SetAsync(identityToken.RefreshToken, identityToken, IdentityCacheRegion,
                    expiresOn - DateTimeOffset.UtcNow)
            );
        }

        public async Task<IdentityToken> GetTokenAsync(string code)
        {
            var token = await _cache.GetAsync<IdentityToken>(code, IdentityCacheRegion);
            if (token != null)
                return token.ExpiresOn >= DateTimeOffset.UtcNow ? token : null;

            var session = await _userSessionRepository.GetAsync(code);
            if (session != null && session.ExpiresOn >= DateTimeOffset.UtcNow)
            {
                token = new IdentityToken(session.Ticket, session.Code, session.ExpiresOn);
                await _cache.SetAsync(token.RefreshToken, token, IdentityCacheRegion,
                    session.ExpiresOn - DateTimeOffset.UtcNow);
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

        public async Task<List<UserFunctions>> GetFunctionsAsync(long companyId, UsersRequest request)
        {
            return await _userRepository.GetFunctionsAsync(companyId, request.Code, request.Description);
        }

        public async Task<PagingResponse<List<UsersResponse>>> GetUsersAsync(long companyId,
            PagingRequest<UsersRequest> request)
        {
            var take = request.ItemsPerPage;
            var users = await _userRepository.GetUsersAsync(companyId, request.Data.Code, request.Data.Description,
                request.Skip, take);
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

        private async Task<UserResponse> ValidateAsync(IDefaultIdentity identity, UserRequest record)
        {
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);
            
            var response = new UserResponse();
            if (string.IsNullOrWhiteSpace(record.Code))
                response.Fields["Code"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid code"
                        }
                    }
                };

            if (string.IsNullOrWhiteSpace(record.Description))
                response.Fields["Description"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid value"
                        }
                    }
                };

            if (!string.IsNullOrEmpty(record.Password) && record.Password.Length < 8)
                response.Fields["Password"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Password must be long at least 8 characters"
                        }
                    }
                };
            else if (!string.IsNullOrEmpty(record.Password) &&
                     !Regex.IsMatch(record.Password,
                         @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,30}$"))
                response.Fields["Password"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message =
                                "The password must be long no more than 30 characters and contain at least a digit, a lower and an upper case letter"
                        }
                    }
                };

            if (!(string.IsNullOrEmpty(record.Password) && string.IsNullOrEmpty(record.Password2)) &&
                record.Password != record.Password2)
                response.Fields["Password2"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Password and confirm password must coincide"
                        }
                    }
                };

            if (requireUserReason && string.IsNullOrEmpty(record.Reason))
            {
                response.Fields["Reason"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Reason is a required field"
                        }
                    }
                };
            }

            response.Valid = !response.Fields.Any();
            return response;
        }

        public async Task<UserResponse> LoadUserAsync(DefaultIdentity identity, UserRequest record)
        {
            bool LDAP = !string.IsNullOrEmpty(await _settingService.LDAPHostAsync(identity.CompanyId));
            var response = new UserResponse
            {
                ShowPassword = !LDAP,
                ShowAuthenticationCode = LDAP
            };

            if (!string.IsNullOrEmpty(record.Code))
                try
                {
                    var user = await _userRepository.GetAsync(identity.CompanyId, record.Code);

                    response.GeneralFunctions = user.UserFunctions.Where(f =>
                            f.FunctionGroup != "Prototypes" && f.FunctionGroup != "Instances")
                        .Select(gf => new Function { functionGroup = gf.FunctionGroup, function = gf.Function })
                        .ToArray();

                    response.PrototypeFunctions = user.UserFunctions.Where(f => f.FunctionGroup == "Prototypes")
                        .GroupBy(pf => new { pf.Department, pf.PrototypeType })
                        .Select(g => new Function
                        {
                            department = g.Key.Department,
                            prototypeType = g.Key.PrototypeType,
                            functionGroup = "Prototypes",
                            functions = g.Select(pf => pf.Function).ToArray(),
                            level = g.First().Tag
                        })
                        .ToArray();

                    response.InstanceFunctions = user.UserFunctions.Where(f => f.FunctionGroup == "Instances")
                        .GroupBy(pf => new { pf.Department, pf.PrototypeType })
                        .Select(g => new Function
                        {
                            department = g.Key.Department,
                            prototypeType = g.Key.PrototypeType,
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

        private static string SerializeUserInformation(UserRequest user, IEnumerable<UserFunctions> userFunctions)
        {
            return JsonConvert.SerializeObject(new
            {
                user.Code,
                user.AuthenticationCode,
                user.Email,
                NameAndSurname = user.Description,
                user.Enabled,
                Functions = userFunctions.Select(uf => new
                {
                    uf.FunctionGroup,
                    uf.Function,
                    Department = uf.Department.HasValue ? uf.Department + " " + uf.Departments?.Name : null,
                    PrototypeType = uf.PrototypeType.HasValue ? uf.PrototypeType + " " + uf.PrototypeTypes?.Name : null,
                    Level = uf.Tag
                }),
                ReasonForChange = user.Reason
            });
        }

        public async Task<UserResponse> AddUserAsync(DefaultIdentity identity, UserRequest record)
        {
            var response = await ValidateAsync(identity, record);
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
                        Information = SerializeUserInformation(record, User.UserFunctions)
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
            var response = await ValidateAsync(identity, record);
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
                        Information = SerializeUserInformation(record, User.UserFunctions)
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

        public async Task<UserResponse> DeleteUserAsync(DefaultIdentity identity, UserRequest record)
        {
            var response = new UserResponse();
            try
            {
                if (identity.Code == record.Code)
                    throw new Exception("Users cannot delete themselves.");

                await _userRepository.DeleteAsync(identity.CompanyId, record.Code);
                await _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
                {
                    Company = identity.CompanyId,
                    User = identity.Code,
                    Action = TrailAction.UserDeleted,
                    Date = DateTimeOffset.UtcNow,
                    Information = JsonConvert.SerializeObject(new
                    {
                        record.Code,
                        ReasonForChange = record.Reason
                    })
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
                            new EmailMessage(user.Company, "User disabled",
                                await _settingService.UserBlockMessageAsync(user.Company),
                                user.Email,
                                new List<Tuple<string, string>> { new Tuple<string, string>("User", user.Description) })
                            {
                                TrailMessage = new EmailTrailMessage(user.Code, null, null)
                            }, cancellationToken);
                }

                await _userRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await _emailService.LogErrorAsync(e, "Failed to disable inactive users");
                Logger.Error(e);
            }

            try
            {
                var users = await _userRepository.GetUsersDisableWarningAsync();
                foreach (var user in users)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                        await _queueService.SendObjectAsync(QueueName.Email,
                            new EmailMessage(user.Company, "User disabled soon",
                                await _settingService.UserBlockWarningMessageAsync(user.Company),
                                user.Email,
                                new List<Tuple<string, string>> { new Tuple<string, string>("User", user.Description) })
                            {
                                TrailMessage = new EmailTrailMessage(user.Code, null, null)
                            }, cancellationToken);
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
                await _emailService.LogErrorAsync(e, "Failed to clear users pin");
                Logger.Error(e);
            }
        }
    }
}