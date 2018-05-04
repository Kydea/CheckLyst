Feature: DataSourcesList
	Description

##############################################################
# Add a Data Source 
##############################################################

@WithTransaction
Scenario: Add a Data Source
	Given I assign 'testDataSource2123' as the code of the Data Source
	And I assign 'test' as the class of the Data Source
	When I submit the request to add the Data Source
	Then the system correctly adds the Data Source

@WithTransaction
Scenario: Add a Data Source without setting a code
	Given I assign '' as the code of the Data Source
	When I submit the request to add the Data Source
	Then the system does not add the Data Source and returns an error message: 'Invalid code'

##############################################################
# Edit a Data Source 
##############################################################

@WithTransaction
Scenario: Edit a Data Source
	Given I click on a Data Source
	And I assign 'test description' as the description of the Data Source
	When I submit the request to edit the Data Source
	Then the system correctly saves the changes of the Data Source

##############################################################
# Delete a Data Source 
##############################################################

@WithTransaction
Scenario: Delete a Data Source
	Given I click on a Data Source
	When I submit the request to delete the Data Source
	Then the system correctly deletes the Data Source