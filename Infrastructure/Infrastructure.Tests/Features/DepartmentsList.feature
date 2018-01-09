Feature: DepartmentsList
	Description

##############################################################
# Add a Department 
##############################################################

@WithTransaction
Scenario: Add a department without giving it a name
	Given I assign '' as the name of the department
	When I submit the request to add this department
	Then the system does not saves the department and returns an error message: 'Invalid name'

@WithTransaction
Scenario: Add a department
	Given I assign 'testPrototype' as the name of the department
	When I submit the request to add this department
	Then the system allows to correctly save the department into the database

##############################################################
# Edit Department
##############################################################

@WithTransaction
Scenario: Change the name of a department
	Given I click on a department
	And I change the name of the department in 'changed Name' 
	When I submit the request to edit this department
	Then the system allows to correctly save the department into the database

##############################################################
# Delete Department
##############################################################

@WithTransaction
Scenario: Delete a department
	Given I click on a department
	When I submit the request to delete this department
	Then the system correctly delete the department from the database

@WithTransaction
Scenario: Delete a department used for a prototype type
	Given I click on a department that is used for creating a prototype type
	When I submit the request to delete this department
	Then the system does not allow the department deletion from the database

@WithTransaction
Scenario: Delete a department used for a prototype
	Given I click on a department that is used for creating a prototype
	When I submit the request to delete this department
	Then the system does not allow the department deletion from the database

