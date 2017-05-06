# Parilis

Parilis compares two different databases and performs a series of actions to make their schemas equal. First, it gets a description of every table, constraint and index on each database. Then, it compiles a queue of actions that must be performed in order to make the structure of the *actual* database equal to the structure of a *reference* database. Finally, it executes all the actions and log the results.

## It's all about structure

Parilis only understands structure. I **does not perform** any semantic modification. For instance, if you want to rename a column on a specific table, you must do so before running Parilis. Likewise, if you need to modify data before performing any modification, you also need to do it before running Parilis.

## Instalation

To install the Nuget package with the implementation for **MS SQL Server** databases and your dependencies, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console) on **Visual Studio**:

```
PM> Install-Package Parilis.SqlServer
```

## Usage

First of all, create the description of each database:

```csharp
var actualConnection = new ConnectionInfo {
	DatabaseName = "actual_database_name",
	HostName = "uri_to_actual_database_instance",
	User = "username",
	Password = "password"
};
var actualDatabase = new DatabaseDescription(actualConnection);

var referenceConnection = new ConnectionInfo {
	DatabaseName = "reference_database_name",
	HostName = "uri_to_reference_database_instance",
	User = "username",
	Password = "password"
};
var referenceDatabase = new DatabaseDescription(referenceConnection);
```

Then simply run Parilis:

```csharp
var parilis = new Parilis(actualDatabase, referenceDatabase);
parilis.Run();
```

You can just get a list of modifications that need to be performed without actually performing them:

```csharp
var actionsToBePerformed = parilis.GetActions();
```
You can be notified when a modification is made. To do this, before call the Run method, simply register a lambda or delegate on the OnProgress event of the Parilis object to get the current runnning percentage and a message that describes the last executed action.

```csharp
var parilis = new Parilis(actualDescription, referenceDescription);
parilis.OnProgress += (percentualProgress, message) => {
   view.UpdateProgress(percentualProgress, message);
 };
parilis.Run();
```
or 

```csharp
var parilis = new Parilis(actualDescription, referenceDescription);
parilis.OnProgress += UpdateProgress;
parilis.Run();

[...]

private void UpdateProgress(double percentualProgress, string message) {
   view.UpdateProgress(percentualProgress, message);
 };
```

Finally, if your database has multiple schemas and you just want to make modifications to a single one, you can filter the database description like so:

```csharp
var actualDatabase = new DatabaseDescription(actualConnection).FilterBySchema("schema_name");
```

## Extensions

Parilis is built in a generic way to enable developers to create solutions for specific database products. The current version provides an [implementation for MS SQL Server databases](https://github.com/thiagotts/parilis/tree/master/SqlServer). In order to create solutions to other types of databases, you must provide implementations for the [interfaces in the Core project](https://github.com/thiagotts/parilis/tree/master/Core/Interfaces). Please refer to the MS SQL Server solution for a real example.

## Roadmap

Check the [project's roadmap](https://trello.com/b/dfrQlizZ) to keep track of what is going on.

## License

The MIT License (MIT)
