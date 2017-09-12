using System.Threading.Tasks;
using CheckListManager.AccessControl.Logic.Exceptions;
using CheckListManager.AccessControl.Logic.Services;
using CheckListManager.AccessControl.Tests.Contexts;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace CheckListManager.AccessControl.Tests.Steps
{
	[Binding]
	public sealed class LoginSteps
	{
		private readonly LoginContext _context;
		private readonly IUserService _userService;

		public LoginSteps(LoginContext context, IUserService userService)
		{
			_context = context;
			_userService = userService;
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
