# TextTemplating .NET tool


 **CSX** and **T4** scripts using .net cli command for design time  transformation and possibility to use engine class for runtime transformation or on the fly script execution.
One of the very few tools that supports reflection and web projects (and dont crash)!

Compatible with **.NET 7+**
Better alernative for comandline or poweshell, executing from there. Usable on any operating system with .net core.

**IF** You have template from regular .net you need to adapt it for .net core, it doesnt work rightaway.. you ll get strange errors, start with simple template that works and modify it. This is the right way.

Nuget, commandline and powershell
https://www.nuget.org/packages/TextTemplating.Tool/

VSCODE Extension

https://marketplace.visualstudio.com/items?itemName=jacknq.TextTemplating

.NET Core 3 sdk introduced [.NET tool - cmd line utilites for .net](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)


### Why, what do I do with T4 or CSX?
Generate **any** text, **any** code or script template, **any** language from your domain c# code, include it in your continous integration... very useful stuff! See DesignTimeSample etc.

### Usage 
transform template on the fly
```
dotnet tt Person.tt
dotnet tt hello.csx

 or 
 
dotnet tt trans -f Person.tt
```

## Goal
Scaffolding for .NET Core without IDE.
Command line tool and designtime transformation tool,
based on roslyn, no mono or 3rdparty dependencies.


The Visual Studio 2017 and Xamarin Studio now supports to process *.tt files in desing time, but this repo is maybe still useful who wants to process templates in a dotnet core  project outside IDE (eg. in Win, Linux or macOS with Visual Studio Code -> VSCODE)

## How to install

### As a command line tool
### create manifest file
```
dotnet new tool-manifest
```
### Install locally
```
dotnet tool install TextTemplating.Tool
```

### OR Install globally 
```
dotnet tool install --global TextTemplating.Tool
```
Add nuget package
```xml
<PackageReference Include="TextTemplating.Core" Version="5.0.0" /> 
```
Now you can use the `dotnet tt` command as a command line tool locally or just call `tt` globally to transform templates at design-time, with the specified command line arguments.
## How to Use
Run `dotnet tt -h` to see the usage.


### As a design time tool
Add the same packages mentioned above, then you can run `dotnet tt trans -f Person.tt` to transform a text template.

Note: You can use all the packages that you have installed into your project when writing T4 template, so there may be no necessary to use "assembly" directive to reference assembly via assembly name(so I skipped these feature, you can also reference assembly via path).

### As tool you can call dynamically at runtime - that generate csharp class from tt template 
```
dotnet tt proc -f person.tt 
```

### As a library
To transform templates at runtime, you can also use the `Engine` class.

*Sample is work in progres*


# License
MIT
