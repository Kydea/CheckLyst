Feature: FilterPanel
	Description
	
Scenario: Get Trail list by User
	And I want the trails of a specific user
	When I send the search request
	Then the result should be a list of records belonging to the same user

Scenario: Get Trail list by CheckList code
	And I want a list of trails identified with a specific checklist code
	When I send the search request
	Then the result should be a list of records with the same checklist code