using System;
using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.AccessControl.Tests.Contexts;
using CheckListManager.AccessControl.Tests.Data;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Services;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace CheckListManager.AccessControl.Tests.Steps.Authentication
{
	[Binding]
	public sealed class LoginSteps
	{
		private readonly LoginContext _context;
		private readonly IUserService _userService;
		private readonly IUserRepositoryExtended _userRepository;
		private readonly IHashService _hashService;
		private readonly ICompanyRepositoryExtended _companyRepository;

		public LoginSteps(LoginContext context, IUserService userService, IUserRepositoryExtended userRepository,
			IHashService hashService, ICompanyRepositoryExtended companyRepository)
		{
			_context = context;
			_userService = userService;
			_userRepository = userRepository;
			_hashService = hashService;
			_companyRepository = companyRepository;
		}

		[Given(@"The system contains the user '(.*)' with password '(.*)' that is not enabled")]
		public async Task GivenTheSystemContainsTheUserWithPasswordThatIsNotEnabled(string username, string password)
		{
			var user = new Users
			{
				Code = username,
				Password = _hashService.HashStringToBytes(password),
				Company = (await _companyRepository.GetFirstCompanyAsync()).ID,
				CreatedOn = DateTimeOffset.UtcNow,
				Enabled = false
			};
			await _userRepository.CreateTestUser(user);
		}

		[Given(@"I want to login as '(.*)' with password '(.*)'")]
		public void GivenIWantToLoginAsWithPassword(string username, string password)
		{
			_context.Username = username;
			_context.Password = password;
		}

		[When(@"I send the login request")]
		public async Task WhenISendTheLoginRequest()
		{
			try
			{
				await _userService.AuthenticateAsync(_context.Username, _context.Password);
			}
			catch (AuthenticationException e)
			{
				_context.Exception = e;
			}
		}

		[Then(@"the result should be an exception with error '(.*)' and message '(.*)'")]
		public void ThenTheResultShouldBeAnExceptionWithErrorAndMessage(int error, string message)
		{
			Assert.That(_context.Exception, Is.Not.Null);
			Assert.AreEqual(error, _context.Exception.ErrorCode);
			Assert.AreEqual(message, _context.Exception.Message);
		}
	}
}
