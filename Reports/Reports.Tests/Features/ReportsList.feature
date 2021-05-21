Feature: ReportsList

@WithTransaction
Scenario: Add a Report without a code
	Given I assign '' as the code of the report
	When I submit the request to add the report
	Then the system does not save the report and returns an error message 'Invalid code' on the 'Code' field
