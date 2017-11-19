Feature: InstanceCache

@CleanupRedis
Scenario: Create an instance
	Given I am logged in as 'CheckListManager'
	Given I have want to create an instance based on a prototype
	When I create the instance
	Then I have the instance stored in redis
	And I have the instance in my instances list
	And I do not have the instance in the list when I am logged as 'Jack'


#TEST:
#	Check remove when 1 user has it open
#	Check remove when more then 1 user has it open