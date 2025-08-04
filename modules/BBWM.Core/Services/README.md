# CRUD/Paging Automation

Full documentation here: https://wiki.bbconsult.co.uk/display/BLUEB/Grid+Pages+Automation

## What is our goal when we talk about CRUD/Paging?

Our goal is to implement a code functional to make customer projects development more efficient.
The goal is determined by business tasks and therefore those tasks determine the code solution.
Step left step right from the goal and we get into overcomplexity. 

Our particular business task requires to have some mechanism to speed up creating of grid pages.
The grid implements CRUD (create/read/update/delete) actions. Talking about “CRUD/Paging” we only suppose
a mechanism for GRID PAGES. It’s highly important to keep in mind. It fully determines our solution.
We don’t need anything else. For else things, we have another code/solutions.
Then we end up with the goal described:
-	we need to automate grid pages creation to spend less efforts
-	grid page should work with main DB context by default (99% case)
-	grid page can be adjusted to work with another DB context (Demo DB context for example)
-	grid page can be adjusted to work with other sources (file system, memory, AWS records (we do) etc.)
(but the last two points shouldn’t complicate the model)

## What is automation here and what are the components of CRUD/paging automation:
-	a default grid page with no custom behaviour should be created with minimum efforts.
    Ideally – adding a single empty controller class with necessary attributes (not C# attributes mentioned here). 
    Attributes could be, as an example: 
    o	C# attributes to define access rules
    o	C# attributes to define the route
    o	Generic parameters to define model/DTO/PageItemDTO/DataContext used by the grid
-	An ability to customize a particular API method on the API controller level with minimum efforts  
-	An ability to link the API controller to a service class to precisely customize behaviour on the services
    level with minimum efforts.
-	the service implementing CRUD should clearly describe what it does as the provider for the controller,
    it should be readable.
-	Avoid creating extra classes/code lines/dependency injections mapping if it’s possible.

## Current automation approach is based on three components:
1)	[Base inherited CRUD/paging controller](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/blob/develop/modules/BBWM.Core/Web/DataControllerBase.cs) to automate CRUD/paging methods for
    a business controller.
2)	[Default data service](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/blob/develop/modules/BBWM.Core/Services/DataService.cs) to implement all CRUD/Paging/other operations.
3)	[Set of CRUD/paging interfaces](https://gitlab.bbconsult.co.uk/blueberry/bbwt3/-/blob/develop/modules/BBWM.Core/Services/CrudInterfaces.cs) used by the controller to trigger CRUD methods for a case when
    a method is supported(customized) by a provider service.
