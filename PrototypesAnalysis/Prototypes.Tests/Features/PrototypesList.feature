Feature: PrototypesList

Scenario: Access the system without grants
	Given I use a user that has not the grants for accessing the prototypes list
	When I access the page
	Then the system returns an error message: "the user does not have prototype grants on any company"
