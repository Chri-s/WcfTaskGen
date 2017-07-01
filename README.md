# WcfTaskGen (WCF Task-based operation generator)

This tool generates additional methods to use WCF Services with aysnc/await with .NET 4.0.

It can generate these methods in C# and VB.NET.

This is a simple command line tool which loads an assembly and generates TPL methods which can be used with async/await. For .NET 4.5 projects these methods can be generated with Visual Studio 2012 and later, but it doesn't create them for .NET 4 projects.

For further information see the [documentation](docs/quickstart.md).

## Known limitation: ##

This tool can't create TPL methods for operations which have ref/ByRef parameters. For these methods it would need to create the message contracts.
