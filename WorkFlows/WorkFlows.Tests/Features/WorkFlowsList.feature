Feature: WorkFlowsList
	Description

# Profiles - Signatures
#	1. Revisor
#   2. Deputy approver
#   3. Approver
#   4. Deputy final approver
#   5. Final approver

#####################################
# Add Work Flow
#####################################

@WithTransaction
Scenario: Add a Workflow without a description
	Given I assign '' as the description of the workflow
	And I do not add any signature to the workflow
	When I submit the request to add the workflow
	Then the system does not save the workflow and returns an error message 'Invalid description' on the 'Description' field

@WithTransaction
Scenario: Add a Workflow without selecting any step
	Given I assign 'workflowTest11' as the description of the workflow
	And I do not add any signature to the workflow
	When I submit the request to add the workflow
	Then the system does not save the workflow and returns an error message: 'The work flow myst have at least one step'

@WithTransaction
Scenario: Add a Workflow
	Given I assign 'workflowTest12' as the description of the workflow
	And I add the 'Revisor' signature in the workflows' steps
	And I add the 'Deputy approver' signature in the workflows' steps
	When I submit the request to add the workflow
	Then the system should correctly save the workflow into the database

#####################################
# Edit Work Flow
#####################################

@WithTransaction
Scenario: Edit a Workflow giving it a invalid description
	Given I click on a workflow
	And I assign '' as the description of the workflow
	When I submit the request to edit the workflow
	Then the system does not edit the workflow and returns an error message 'Invalid description' on the 'Description' field

@WithTransaction
Scenario: Edit a Workflow deleting all the provided Steps
	Given I click on a workflow
	And I delete the steps from the workflow
	When I submit the request to edit the workflow
	Then the system does not save the workflow and returns an error

@WithTransaction
Scenario: Edit a Workflow
	Given I click on a workflow
	And I assign 'workflowTest13' as the description of the workflow
	When I submit the request to edit the workflow
	Then the system should correctly save workflow changes

#####################################
# Delete Work Flow
#####################################

@WithTransaction
Scenario: Delete a Workflow
	Given I click on a workflow
	When I submit the delete request to delete the workflow
	Then the system should correctly delete the workflow from the database

@WithTransaction
Scenario: Delete a Workflow that has been used for a prototype type
	Given I click on a workflow that has been used for a prototype type
	When I submit the delete request to delete the workflow
	Then the system does not delete the workflow and returns an error