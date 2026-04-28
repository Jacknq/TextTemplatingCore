using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using TextTemplating.T4.Parsing;
using TextTemplating.T4.Preprocessing;
//using Microsoft.CodeAnalysis.CSharp.Scripting.Compilers;
using System.Text.RegularExpressions;

namespace TextTemplating.Infrastructure
{
    public partial class Engine
    {
        private readonly ITextTemplatingEngineHost _host;
        private readonly RoslynCompilationService _compilationService;

        public Engine(ITextTemplatingEngineHost host, RoslynCompilationService compilationService)
        {
            _host = host;
            _compilationService = compilationService;
        }

        // todo add to cli tool
        /// <summary>
        /// convert tt template to c# code
        /// </summary>
        /// <param name="content">tt template</param>
        /// <param name="className">cs class</param>
        /// <param name="classNamespace">cs namespace</param>
        /// <returns>cs cs code with references</returns>
        public PreprocessResult PreprocessT4Template(string content, string className, string classNamespace)
        {
            var result = new Parser(_host).Parse(content);
            var transformation = new PreprocessTextTransformation(className, classNamespace, result, _host);
            var preprocessedContent = transformation.TransformText();

            var preprocessed = new PreprocessResult
            {
                References = result.References.Distinct().ToArray(),
                PreprocessedContent = preprocessedContent
            };

            return preprocessed;
        }

        /// <summary>
        /// run t4 template
        /// </summary>
        /// <param name="content">tt template</param>
        /// <returns></returns>
        public string ProcessT4Template(string content)
        {
            var className = "GeneratedClass";
            var classNamespace = "Generated";
            var assemblyName = "Generated";

            var preResult = PreprocessT4Template(content, className, classNamespace);

            //var compiler = new RoslynCompilationService(_host);
            var transformationAssembly = _compilationService.Compile(assemblyName, preResult);


            var transformationType = transformationAssembly.GetType(classNamespace + "." + className);

            var transformation = Activator.CreateInstance(transformationType) as TextTransformationBase;//(TextTransformationBase)

            transformation.Host = _host;
            return transformation.TransformText();
        }

        public string CSX_Script(string content, string filePath)
        {
            return ProcessCSXTemplate(content, filePath, null, null);
            // // Read script content
            // string scriptContent = File.ReadAllText(filePath);

            // // Get references based on using statements
            // string[] references = GetScriptReferences(scriptContent, filePath);
            // var opt =
            //         ScriptOptions.Default
            //         .WithReferences(references)
            //           .WithReferences(_host.StandardAssemblyReferences)
            //           .AddReferences(Assembly.GetExecutingAssembly())
            //           .WithMetadataResolver(ScriptMetadataResolver.Default.WithSearchPaths(RuntimeEnvironment.GetRuntimeDirectory()))
            //           //   .AddImports(_host.StandardImports) //no standard imports
            //           .WithFilePath(filePath);
            // // Use Script instead of CSharpScript
            // var script = CSharpScript.Create(
            //     scriptContent,
            //     //   globals: Assembly.GetExecutingAssembly(), // Add current assembly as a reference
            //     //   references: references.Select(r => MetadataReference.CreateFromFile(r)).ToArray(),
            //     options: opt);
            // //  { OutputKind = OutputKind.Dynamic, ScriptingFilePath = scriptPath });

            // // Execute script and get results
            // var loader = new InteractiveAssemblyLoader();
            // try
            // {
            //     CSharpScript.EvaluateAsync(content, options: opt, loader)
            //    .ContinueWith(s => s.Result).Wait();
            // }
            // catch (CompilationErrorException ex)
            // {
            //     if (ex.Message.Contains(")"))
            //     {
            //         var m = ex.Message.Split(")");
            //         ttConsole.WriteError(m[0] + ")");
            //         ttConsole.WriteError(m[1]);
            //     }
            //     else { ttConsole.WriteError(ex.Message); }
            //     ttConsole.WriteNormal("");
            //     ttConsole.WriteError(ex.StackTrace);
            // }
            // return "";
        }

        private static string[] GetScriptReferences(string scriptContent, string scriptPath)
        {
            // Extract using statements using regex
            var regex = new Regex(@"using\s+([^\s;]+);", RegexOptions.Multiline);
            var matches = regex.Matches(scriptContent);

            // Get reference paths for each using statement
            string[] references = matches.Select(m => Path.Combine(Path.GetDirectoryName(scriptPath), m.Groups[1].Value + ".dll"))
                                        .Where(File.Exists).ToArray();
            List<string> netVer = new() { "net10.0\\", "net8.0\\" };
            netVer.ForEach(ver =>
            {
                references = references.Concat(matches.Select(m => Path.Combine(Path.GetDirectoryName(scriptPath) + "\\bin\\Release\\" + ver, m.Groups[1].Value + ".dll"))
                                            .Where(File.Exists).ToArray()).ToArray();
                references = references.Concat(matches.Select(m => Path.Combine(Path.GetDirectoryName(scriptPath) + "\\bin\\Debug\\" + ver, m.Groups[1].Value + ".dll"))
                .Where(File.Exists).ToArray()).ToArray();
            });
            var res = references.Distinct().ToArray();
            // Console.WriteLine("rref length:" + res.Length + " ");
            // res.ToList().ForEach(x => Console.WriteLine(x));
            return res;
        }
        // private static MetadataReference[] ResolveNuGetReferences(string[] references)
        // {
        //     // Create a temporary workspace for NuGet package resolution
        //     var workspace = MSBuildWorkspace.Create();

        //     // Create a temporary project for package resolution
        //     var project = workspace.AddProject("ScriptProject", "1.0.0", "net8.0");
        //     foreach (var reference in references)
        //     {
        //         // Example: "Newtonsoft.Json"
        //         project.AddPackageReference(reference);
        //     }

        //     // Restore NuGet packages
        //     workspace.RestorePackagesAsync().Wait();

        //     // Get resolved references
        //     return project.References.ToArray();
        // }
        private string RestoreNugetPackages(string scriptContent)
        {
            var nugetPattern = @"#r\s+""nuget:\s*([^,]+),\s*([^""]+)""";
            var matches = System.Text.RegularExpressions.Regex.Matches(scriptContent, nugetPattern);

            // Find the NuGet packages folder first
            string nugetPackagesFolder = GetNugetPackagesFolder();

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string packageName = match.Groups[1].Value.Trim();
                string version = match.Groups[2].Value.Trim();

                string packagePath = Path.Combine(nugetPackagesFolder, packageName.ToLower(), version);

                // Skip if already exists in the folder
                if (Directory.Exists(packagePath))
                    continue;

                // Try to restore using nuget.config if it exists
                if (!TryRestoreWithNugetConfig(packageName, version))
                {
                    // Fallback: restore directly from nuget.org
                    TryRestoreFromNuget(packageName, version);
                }
            }

            return nugetPackagesFolder;
        }

        private bool TryRestoreWithNugetConfig(string packageName, string version)
        {
            try
            {
                // Look for nuget.config in current and up to 3 parent directories
                string nugetConfigPath = FindNugetConfig();

                if (string.IsNullOrEmpty(nugetConfigPath))
                    return false;

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"add package {packageName} --version {version}",
                    WorkingDirectory = Path.GetDirectoryName(nugetConfigPath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }
        private string GetNugetPackagesFolder()
        {
            // First, try to get folder from nuget.config
            string nugetConfigPath = FindNugetConfig();
            if (!string.IsNullOrEmpty(nugetConfigPath))
            {
                string packagesFolder = ExtractPackagesFolderFromConfig(nugetConfigPath);
                if (!string.IsNullOrEmpty(packagesFolder) && Directory.Exists(packagesFolder))
                    return packagesFolder;
            }

            // Fallback to default cache location
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages");
        }

        private string ExtractPackagesFolderFromConfig(string nugetConfigPath)
        {
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(nugetConfigPath);

                var configNode = doc.SelectSingleNode("//config/add[@key='globalPackagesFolder']");
                if (configNode != null)
                {
                    string value = configNode.Attributes["value"]?.Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Handle relative paths
                        if (!Path.IsPathRooted(value))
                        {
                            value = Path.Combine(Path.GetDirectoryName(nugetConfigPath), value);
                        }
                        return Path.GetFullPath(value);
                    }
                }
            }
            catch
            {
                // If parsing fails, return null and use default
            }

            return null;
        }


        private bool TryRestoreFromNuget(string packageName, string version)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"package search {packageName} --version {version} --exact-match",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        // Package found, restore it
                        return RestorePackageLocally(packageName, version);
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool RestorePackageLocally(string packageName, string version)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"nuget locals http-cache --clear",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    process.WaitForExit();
                }

                // Now restore the package
                psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"package search {packageName} --version {version}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = System.Diagnostics.Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private string FindNugetConfig()
        {
            string currentDir = Directory.GetCurrentDirectory();
            int levelsChecked = 0;
            int maxLevels = 3;

            while (!string.IsNullOrEmpty(currentDir) && levelsChecked <= maxLevels)
            {
                string nugetConfigPath = Path.Combine(currentDir, "nuget.config");

                if (File.Exists(nugetConfigPath))
                    return nugetConfigPath;

                var parent = Directory.GetParent(currentDir);
                if (parent == null)
                    break;

                currentDir = parent.FullName;
                levelsChecked++;
            }

            return null;
        }
        private List<MetadataReference> ParseNugetReferences(string scriptContent,
        string nugetGlobalpath = null)
        {
            var nugetReferences = new List<MetadataReference>();
            var nugetPattern = @"#r\s+""nuget:\s*([^,]+),\s*([^""]+)""";
            var matches = System.Text.RegularExpressions.Regex.Matches(scriptContent, nugetPattern);
           // ttConsole.WriteNormal($"matches>{matches.Count}");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string packageName = match.Groups[1].Value.Trim();
                string version = match.Groups[2].Value.Trim();

                string nugetCache = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".nuget", "packages");

                string packagePath = Path.Combine(nugetCache, packageName.ToLower(), version);

                if (Directory.Exists(packagePath))
                {
                    var dllFile = Directory.GetFiles(packagePath, $"{packageName}.dll", SearchOption.AllDirectories)
                    .FirstOrDefault(f => !f.Contains("ref/"));

                    if (!string.IsNullOrEmpty(dllFile))
                        nugetReferences.Add(MetadataReference.CreateFromFile(dllFile));
                }
                if (nugetGlobalpath != null && Directory.Exists(nugetGlobalpath))
                {
                    //ttConsole.WriteNormal($"match {packageName}");
                    string globalPath = Path.Combine(nugetGlobalpath, packageName.ToLower(), version);
                    var dllFile = Directory.GetFiles(globalPath, $"{packageName}.dll", SearchOption.AllDirectories)
                                .Where(f => !f.Contains("ref/"))
                                .Where(f => ExtractFrameworkPriority(f) > 0) // Only files with recognized frameworks                               
                                .OrderByDescending(d => ExtractFrameworkPriority(d))
                                .FirstOrDefault(f => IsCompatibleFramework(f, GetDotNetVersionObject().Major.ToString()));
                    // If no framework-specific match, fallback to any dll
                    if (dllFile == null)
                    {
                        dllFile = Directory.GetFiles(globalPath, $"{packageName}.dll", SearchOption.AllDirectories)
                            .FirstOrDefault(f => !f.Contains("ref/"));
                    }
                    if (!string.IsNullOrEmpty(dllFile))
                    {
                        ttConsole.WriteNormal($"addingdll {dllFile}");
                        nugetReferences.Add(MetadataReference.CreateFromFile(dllFile));
                    }
                }
            }

            return nugetReferences;
        }
        static bool IsCompatibleFramework(string filePath, string targetFramework)
        {
            int fileVersion = ExtractFrameworkPriority(filePath);

            // targetFramework is "10" from GetDotNetVersionObject().Major
            string normalized = $"net{targetFramework}.0";
            int targetVersion = ExtractFrameworkPriority(normalized);

            return fileVersion > 0 && targetVersion > 0 && fileVersion <= targetVersion;
        }
        static int ExtractFrameworkPriority(string filePath)
        {
            // Try net6.0 format first (with dot) - MUST check this first
            var match = System.Text.RegularExpressions.Regex.Match(filePath, @"net(\d+)\.(\d+)");
            if (match.Success)
            {
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                return major * 100 + minor; // net6.0 = 600, net10.0 = 1000
            }

            // Try netstandard format
            match = System.Text.RegularExpressions.Regex.Match(filePath, @"netstandard(\d+)\.(\d+)");
            if (match.Success)
            {
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                return major * 100 + minor; // netstandard2.0 = 200
            }

            // Try net40 format (no dot) - LAST resort
            match = System.Text.RegularExpressions.Regex.Match(filePath, @"net(\d{2})(?![.\d])");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var version))
                return version * 10; // net40 = 400

            return -1;
        }
        static Version GetDotNetVersionObject()
        {
            string fullVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            // ttConsole.WriteNormal($"fullversion>{fullVersion}");
            string versionString = fullVersion.Replace(".NET ", "");
            return new Version(versionString);
        }
        private static string RemoveNugetDirectives(string scriptContent)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                scriptContent,
                @"^\s*#r\s+""nuget:[^""]*""\s*\n?",
                "",
                System.Text.RegularExpressions.RegexOptions.Multiline
            );
        }
        string output = "";
        public string ProcessCSXTemplate(string content, string filePath,
        IMetadataResolveable resolver = null, ProjectMetadata projmeta = null)
        {
            //  ttConsole.WriteNormal("ProcessCSXTemplate");
            var references = new List<MetadataReference>();
            // project references
            if (resolver != null)
            { references.AddRange(resolver.ResolveMetadataReference()); }

            //todo NUGET
            string nugetPackagesFolder = RestoreNugetPackages(content);
            var nugetReferences = ParseNugetReferences(content, nugetPackagesFolder);
            //  ttConsole.WriteNormal($"nuget folder {nugetPackagesFolder} ref.count: {nugetReferences.Count}");
            references.AddRange(nugetReferences);
            var content2proc = RemoveNugetDirectives(content);
            // assembly instruction           
            output = projmeta?.OutputPath;
            var opt =
                    ScriptOptions.Default

                      .WithReferences(_host.StandardAssemblyReferences)
                      .AddReferences(Assembly.GetExecutingAssembly())
                      .WithMetadataResolver(
                        ScriptMetadataResolver.Default
                             .WithSearchPaths(nugetPackagesFolder)) // Use the returned packages folder)
                      .WithMetadataResolver(ScriptMetadataResolver.Default.WithSearchPaths(RuntimeEnvironment.GetRuntimeDirectory()))
                      //   .AddImports(_host.StandardImports) //no standard imports
                      .WithFilePath(filePath);

            var refFiltd = references.Where((item, index) =>
            !_host.StandardAssemblyReferences.Any(
                   x => item.Display.Contains(x + ".dll"))
                   //dont load dlls that re already included in standard
                   ).ToList();
            // foreach (var item in rr)
            // { Console.WriteLine("ref:" + item.Display); }
            // opt = opt.WithReferences(rr).AddImports(_host.StandardImports); //system object not defined
            foreach (var dep in refFiltd)
            {
                // Logger.Debug("Adding reference to a runtime dependency => " + runtimeDependency);
                opt = opt.AddReferences(MetadataReference.CreateFromFile(dep.Display));
            }
            var loader = new InteractiveAssemblyLoader();
            try
            {
                CSharpScript.EvaluateAsync(content2proc, options: opt, loader)
                .ContinueWith(s => s.Result).Wait();
            }
            catch (CompilationErrorException ex)
            {
                if (ex.Message.Contains(")"))
                {
                    var m = ex.Message.Split(")");
                    ttConsole.WriteError(m[0] + ")");
                    ttConsole.WriteError(m[1]);
                }
                else { ttConsole.WriteError(ex.Message); }
                ttConsole.WriteNormal("");
                ttConsole.WriteError(ex.StackTrace);
            }
            return "";
        }

        [GeneratedRegex("using\\s+([^\\s;]+);", RegexOptions.Multiline)]
        private static partial Regex MyRegex();

        // string r = @"(?:^|\n)(\s*(:?using))\s+((?<attribute>\w*(:?\.)*\w*));";

    }
}
