# Parilis

Parilis compares two different databases and performs a series of actions to make their schemas equal. First, it gets a description of every table, constraint and index on each database. Then, it compiles a queue of actions that must be performed in order to make the structure of the *actual* database equal to the structure of a *reference* database. Finally, it executes all the actions and log the results.

## It's all about structure

Parilis only understands structure. I **does not perform** any semantic modification. For instance, if you want to rename a column on a specific table, you must do so before running Parilis. Likewise, if you need to modify data before performing any modification, you also need to do it before running Parilis.

## Usage

First of all, create the description of each database:

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

Then simply run Parilis:

    var parilis = new Parilis(actualDatabase, referenceDatabase);
    parilis.Run();

You can just get a list of modifications that need to be performed without actually performing them:

    var actionsToBePerformed = parilis.GetActions();

Finally, if your database has multiple schemas and you just want to make modifications to a single one, you can filter the database description like so:

    var actualDatabase = new DatabaseDescription(actualConnection).FilterBySchema("schema_name");

## Extensions

Parilis is built in a generic way to enable developers to create solutions for specific database products. The current version provides an [implementation for MS SQL Server databases](https://github.com/thiagotts/parilis/tree/master/SqlServer). In order to create solutions to other types of databases, you must provide implementations for the [interfaces in the Core project](https://github.com/thiagotts/parilis/tree/master/Core/Interfaces). Please refer to the MS SQL Server solution for a real example.

## License

The MIT License (MIT)