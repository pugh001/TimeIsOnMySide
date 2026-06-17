# ASP dotNET Core REST Example



## Why this template?

This template provided by Technology Strategies contains an example of how you could code a .net 8 c# REST micro service.

It is provided to software engineers as way to get a jump start within the Odin environment. The example integrates with all the fundamental components found within Odin like the Externalized configuration, FluentBit, Postgress and the Identity Provider.

You will note this contains a single project file and no other library projects. It was designed like this to be micro service consisting out of a single simple project without any other class libraries, this is done to take away the appetite or ability to build an accidental monolith by creating dependencies between multiple services within a system.

The fundamental idea is that each microservice should strive to be completely independent in runtime and in code from other micro services. Each micro service must be tiny to allow in to be completely re-written within a sprint if required with no impact on any other micro services within a system.

## Getting started

The template is built in such a way that it will throw exceptions when the required configuration is not provided, this is done to guide the engineer to all the areas that require attention. So do not be alarmed if the service does not run / work out of the box.

This service will require that the service configuration is externalized into the externalized config server once deployed. When running locally the appsettings.local.json file will be used. At time of writting this readme the external config server is a Spring Cloud Config Server. Read more about it here https://spring.io/projects/spring-cloud-config

This service also uses Serilog to log to FluentBit using FluentD. The reason this is done is to decouple the log storage from the service. At the time of writting this readme FluentBit is persisting the logs to Open Search.

To understand how this example works, start to read through the startup code in the Program.cs file and then the RestExampleController.cs. 

This example can be modified as you see fit and you do not need to keep any of the code or structure.

## What this example contains

This solution has integration examples that shows how to integrate with;

- Externalized Configuration Server
- Vault
- FluentBit
- Database
- Identity Provider
- Web Services

## Whats New

- 2024/02/01 Update to dot net 8
