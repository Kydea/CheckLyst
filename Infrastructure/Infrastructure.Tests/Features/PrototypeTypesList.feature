Feature: PrototypeTypesList
	Description

##############################################################
# Add Prototype Type
##############################################################

@WithTransaction
Scenario: Add a prototype type without selecting a department
	Given I don't choose any department from the list
	And I assign 'test' as the name of the prototype
	When I send the add request
	Then the system does not allow to add the unnamed prototype type

@WithTransaction
Scenario: Add a prototype type without giving it a name
	Given I choose a department from the list
	And I assign '' as the name of the prototype
	When I send the add request
	Then the system returns an error message: 'Invalid name'

@WithTransaction
Scenario: Add a prototype type
	Given I choose a department from the list
	And I assign 'testPrototype' as the name of the prototype
	When I send the add request
	Then the system allows to correctly save the record into the database

##############################################################
# Edit Prototype Type
##############################################################

@WithTransaction
Scenario: Change the name of a prototype type
	Given I choose a department from the list
	And I click on a prototype type
	And I change the name of the prototype in 'changed Name' 
	When I send the edit request
	Then the system allows to correctly save the record into the database

##############################################################
# Delete Prototype Type
##############################################################

@WithTransaction
Scenario: Delete a prototype type
	Given I choose a department from the list
	And I click on a prototype type
	When I send the delete request
	Then the system correctly delete the record from the database

@WithTransaction
Scenario: Delete a prototype type used for a prototype
	Given I choose a department from the list
	And I click on a prototype type that is used for a prototype
	When I send the delete request
	Then the system does not allow the prototype type deletion from the database

