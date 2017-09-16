Feature: Login
	In order to protect the system
	An authentication should be in place

Scenario: Login without any username sent
	Given I want to login as '' with password ''
	When I send the login request
	Then the result should be an exception with error '100' and message 'User code not sent'

Scenario: Login without any password sent
	Given I want to login as 'CheckListManager' with password ''
	When I send the login request
	Then the result should be an exception with error '101' and message 'User password not sent'

Scenario: Login with a not existing user
	Given I want to login as 'Not Existing User' with password 'Test123'
	When I send the login request
	Then the result should be an exception with error '102' and message 'User Not Existing User not found'

# TODO create test data and rollback
@WithTransaction
Scenario: Login with a not enabled user
	Given The system contains the user 'TestUser01' with password 'Test Password' that is not enabled
	And I want to login as 'TestUser01' with password 'Test Password'
	When I send the login request
	Then the result should be an exception with error '103' and message 'User CheckListManager not enabled'
