//#r "Newtonsoft.Json"
//#r "TextTemplating.Tool"
using System.Diagnostics;
using System;
using System.Linq;
using System.Reflection;
using TextTemplating;
using Newtonsoft.Json;
using DesignTimeSample;
using Internal;

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
