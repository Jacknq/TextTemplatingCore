using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using TextTemplating.Infrastructure;
using TextTemplating;

namespace TextTemplating.Tools
{
    class Program
    {
        public static IServiceProvider DI;

        static void ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMetadataResolveable, MsBuildProjectMetadataResolver>();
            services.AddSingleton<ITextTemplatingEngineHost, CommandLineEngineHost>();
            services.AddSingleton<RoslynCompilationService>();
            services.AddSingleton<Engine>();
            DI = services.BuildServiceProvider();
        }

        static int Main(string[] args)
        {
            ConfigureServices();
            const string helpTemplate = "-h|--help";
            var app = new CommandLineApplication
            {
                Description = "A simple Text Template Transformer for .Net Core"
            };

            var process = app.Command("proc", AppCommands.ProcessCommand);
            var transform = app.Command("trans", AppCommands.TransformCommand);

            app.HelpOption(helpTemplate);
            try
            {
                if (args.Length == 1)
                {
                    return ProcessFile(args);
                }
                else
                {//else check commands
                    return app.Execute(args);
                }
            }
            catch (CommandParsingException e)
            {
                app.ShowHelp(e.Command.Name);
                return 1;
            }
            catch (IOException e)
            {
                switch (e)
                {
                    case ProjectNotFoundException pe:
                    case FileNotFoundException fe:
                    case DirectoryNotFoundException de:
                        Console.WriteLine(e.Message);
                        break;
                }

                return 1;
            }
        }

        private static readonly string[] SupportedFileTypes = new[] {"tt","csx"};

        private static int ProcessFile(string[] args)
        {         
            if (args.Length == 1 &&
                isSuppFile(args[0], SupportedFileTypes))
            {   var f = args[0];
                ttConsole.WriteNormal("Executing file: " + f);
                if (f.EndsWith(".tt"))
                { return AppCommands.ProcessTTFile(f); }
                return AppCommands.ProcessCSXFile(f); 
            }
            return 0;
        }

        static bool isSuppFile(string s, string[] supptype)
        {
            bool sup = false;
            if (!string.IsNullOrEmpty(s))
            {
                var val = s.ToLower();                
                if(File.Exists( Path.Combine(Environment.CurrentDirectory, s)))
                {
                    foreach (var type in supptype)
                    {
                        if(val.ToLower().EndsWith(type))
                        { return true; }
                    }                
                }
            }
            return sup;
        }
    }
}