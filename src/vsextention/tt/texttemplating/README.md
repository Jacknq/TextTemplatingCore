# TextTemplating 

 **CSX**  (C# script, cs script) and T4  from vscode via .net cli command - design time  transformation and possibility to use engine class for runtime transformation.

**Execute from context menu from VSCode!**

One of the very few tools that supports reflection and **web** projects!
Compatible with **.NET Core 7+**  cs projects

Scaffolding for .NET Core without IDE. Command line tool and designtime transformation tool, based on roslyn, **no mono or 3rdparty dependencies**.

### Support this project
[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate?hosted_button_id=RGHZAXSMKBSS4)

## Features

Generate **any** text, **any** code or script template, **any** language from your domain c# code, include it in your continous integration... very useful stuff! See DesignTimeSample etc.

![features](https://raw.githubusercontent.com/Jacknq/TextTemplatingCore/master/src/vsextention/tt/texttemplating/images/vscontextt.png)

## Requirements
 1) Install .NET SDK 7+ or higher  https://dotnet.microsoft.com/download/dotnet-core
 2) Install textemplating tool 
 ### Install locally - inside project folder
```
dotnet tool install TextTemplating.Tool
```

#### or Install globally 
```
dotnet tool install --global TextTemplating.Tool
```
Add nuget package to csproj, if used with cs project
```xml
<PackageReference Include="TextTemplating.Core" Version="5.0.0" /> 
```


### Usage 
transform template on the fly
```
dotnet tt hello.csx
dotnet tt Person.tt


 or 
 
dotnet tt trans -f Person.tt
```