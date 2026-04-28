//#load "./program.cs" //cannot use namespace comment it
//#r "bin/Debug/net8.0/DesignTimeSample.dll" //build this project first
#r "nuget:Newtonsoft.Json,13.0.4" //nuget reference
using DesignTimeSample;
using System.Diagnostics;
using System;
using System.Linq;
using System.Reflection;
using TextTemplating;
using Newtonsoft.Json;
using Internal;
using Console = System.Console;

//referenced namespace usage
ttConsole.WriteHighlighted("somethin");
var helloWorld = "Hello world!";
Console.WriteLine(helloWorld);
//SIMPLE CLASS
class TestClass
{
    public int A { get; set; }
    public int B { get; set; }
}
var testClass = new TestClass { A = 1, B = 2 };
Console.WriteLine(testClass.A);
//USING LOCAL CONTEXT CLASS HERE!!!
var d = new DPerson() { Name = "Test Person" };
Console.WriteLine(d.Name);
var asm = System.AppDomain.CurrentDomain.GetAssemblies();
Console.WriteLine($"app domain assemblies: {asm.Count()}");
string jsonString = JsonConvert.SerializeObject(d, Formatting.Indented);
Console.WriteLine("Serialized JSON:");
Console.WriteLine(jsonString);
