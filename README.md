# Project Title

This BOT is build for companies to handle the client support requests easily. Bot can be integrated to the company page and the company can connect their ticketing system to the bot. Once connected, the bot can accept messages from the both registered and non registered users. Registration process is also inbuilt to the conversations. Registered users can create cases, check case status and subscribe for one time notification for the case updates.

## Getting Started
- clone the project from GIT HUB
- Run the solution file using visual studio
- Build and run the service to get the swagger UI

### Prerequisites

- Visual studio
- .NET core 3.0
- Nuget packages
- BOT, JIRA, MongoDB connections

```
Give examples
```

### Dependencies
* Microsoft.AspNetCore.App - The web framework used
* Microsoft.NETCore.App 3.0- The web framework used
* [MongoDB.Driver - 2.10.4](http://www.mongodb.org/display/DOCS/CSharp+Language+Center/) - The database used
* [Newtonsoft.Json - 12.0.3](https://www.newtonsoft.com/json/) 
* [log4net - 2.0.8](http://logging.apache.org/log4net/) 

### Installing

A step by step series of examples that tell you how to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

- Send a greeting message
- To use support system, follow the registration process [testing input is included in bots reply]
- Once registered, create a case. Note down the case number that bot provides you, you can also subscribe for updates
- Check case status using the case number you got from the BOT. Subscribe if needed.
- Please send us the case number [sinoj.george@outlok.com] , we will update case status fo you to receive the notification
- We value the client / customer data, so only when you enter the correct company name and verification code, system will allow you to register.

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests
- TicketBOT.Core project contains the base models, interfaces and services
- TicketBOT.JIRA contains the models, interface implentation services. Users can follow this project to create a connection to another ticket system.
- Ticket BOT project is the start up project which uses the core and JIRA project

Every project is structures to the following main folders

- Models [Models used by the project]
- Services [Interfaces and its implementation]
   - Interfaces [Interfaces used]
- Helpers [Helper classes used in the project]

- Resources [Resources used]
- Controllers [Only for the startup project, contains the API end point controllers]

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

 - .NET CORE
 - MONGO DB
 - FB BOT PLATFORM

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Changelog
### [1.2.0.0] - 2020-06-25
- completed the project for hackathon

### [1.1.0.0] - 2020-06-07
- Added
```
- Setup database
```
- Changed
- Fixed

### [1.0.0.0] - 2020-06-07
- Added
```
Init project
```
- Changed
- Fixed

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc
