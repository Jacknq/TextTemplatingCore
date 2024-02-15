using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using TextTemplating.Infrastructure;

namespace TextTemplating.Tools
{
    public static class AppCommands
    {
        const string HelpTemplate = "-h|--help";

        /// <summary>
        /// Find the projet that contains the file
        /// </summary>
        /// <param name="childFile"></param>
        /// <param name="projectFile">Full path to project file</param>
        /// <returns></returns>
        static bool TryFindProjectFile(string childFile, out string projectFile)
        {
            string directory = Path.GetDirectoryName(childFile);
            while (Directory.Exists(directory) && Directory.GetDirectoryRoot(directory) != directory)
            {
                var files = Directory.EnumerateFiles(directory, "*.*proj").ToList();
                if (files.Any())
                {
                    projectFile = files.First();
                    return true;
                }
                directory = Path.GetDirectoryName(directory);
            }
            projectFile = null;
            return false;
        }

        #region Process

        public static void ProcessCommand(CommandLineApplication command)
        {
            command.Description = "Process template to CSharp class file for runtime transform";
            var fileOption = command.Option("-f|--file", "The texttemplate to be processed", CommandOptionType.SingleValue);
            var outputOption = command.Option("-o|--output", "Output directory path, default: out.cs", CommandOptionType.SingleValue);
            var classNameOption = command.Option("-c|--class", "Generated class name, default: GeneratedClass", CommandOptionType.SingleValue);
            var namespaceNameOption = command.Option("-ns|--namespace", "Generated namespace name, default: GeneratedNamespace",
                CommandOptionType.SingleValue);
            command.HelpOption(HelpTemplate);
            command.OnExecute(() =>
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, fileOption.Value());
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var outputPath = Path.Combine(Environment.CurrentDirectory, outputOption.Value() ?? fileName + ".cs");
                if (TryFindProjectFile(filePath, out string projectFile) == false)
                {
                    // throw new ProjectNotFoundException("Current work directory is not in a project directory");
                }

                // Resolve metadata
                var resolver = Program.DI.GetService<IMetadataResolveable>();
                var metadata = resolver.ReadProject(projectFile);


                string className = classNameOption.Value() ?? Path.GetFileName(fileName);
                string namespaceName = namespaceNameOption.Value() ?? metadata?.RootNamespace ?? "GeneratedNameSpace";

                return PreprocessTemplate(filePath, outputPath, className, namespaceName);
            });
        }



        /// <summary>
        /// conversion template
        /// </summary>
        /// <param name="file">file</param>
        /// <param name="outPut">ouput</param>
        /// <param name="className">class name</param>
        /// <param name="namespaceName">namespace</param>
        /// <returns></returns>
        static int PreprocessTemplate(string file, string outPut, string className, string namespaceName)
        {
            var templatesRoot = Path.GetDirectoryName(file);
            var engin = Program.DI.GetService<Engine>();
            var templateContent = File.ReadAllText(file);
            var result = engin.PreprocessT4Template(templateContent, className, namespaceName);
            File.WriteAllText(outPut, result.PreprocessedContent);
            return 0;
        }

        #endregion

        #region Transform

        public static void TransformCommand(CommandLineApplication command)
        {
            command.Description = "Transform tt template";
            var fileOption = command.Option("-f|--file", "The T4 template to be transformed", CommandOptionType.SingleValue);
            command.HelpOption(HelpTemplate);
            command.OnExecute(() =>
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, fileOption.Value());
                if (TryFindProjectFile(filePath, out string projectFile) == false)
                {
                    throw new ProjectNotFoundException("Current work directory is not in a project directory..");
                }

                // Resolve metadata
                var resolver = Program.DI.GetService<IMetadataResolveable>();
                resolver.ReadProject(projectFile);

                return TransformTemplate(filePath);
            });
        }

        static int TransformTemplate(string filePath)
        {
            var engin = Program.DI.GetService<Engine>();
            var templateContent = File.ReadAllText(filePath);
            var result = engin.ProcessT4Template(templateContent);
            var host = Program.DI.GetService<ITextTemplatingEngineHost>();
            var outputPath = Path.Combine(
                Path.GetDirectoryName(filePath),
                $"{Path.GetFileNameWithoutExtension(filePath)}{host.FileExtension}");
            File.WriteAllText(outputPath, result, host.Encoding);
            return 0;
        }

        //new stuff
        public static int ProcessTTFile(string path)
        {
            // Console.WriteLine("ProcessTTFile");
            var filePath = Path.Combine(Environment.CurrentDirectory, path);
            if (TryFindProjectFile(filePath, out string projectFile) == false)
            {
                throw new ProjectNotFoundException("Current work directory is not in a project directory.");
            }

            // Resolve metadata
            var resolver = Program.DI.GetService<IMetadataResolveable>();
            resolver.ReadProject(projectFile);

            return TransformTemplate(filePath);
        }

        public static int ProcessCSXFile(string path)
        {
            var engin = Program.DI.GetService<Engine>();
            var filePath2 = Path.Combine(Environment.CurrentDirectory, path);
            if (File.Exists(filePath2))
            {
                var templateContent = File.ReadAllText(filePath2);

                if (TryFindProjectFile(filePath2, out string projectFile) == false)
                { //simple execution no project file
                    var result1 = engin.CSX_Script(templateContent, filePath2);
                    return 0;
                    //  throw new ProjectNotFoundException("Current work directory is not in a project directory");
                }
                var host = Program.DI.GetService<ITextTemplatingEngineHost>();
                // Console.WriteLine("proj>>"+projectFile);
                // Resolve metadata
                IMetadataResolveable resolver = Program.DI.GetService<IMetadataResolveable>();
                ProjectMetadata projMetadata = resolver.ReadProject(projectFile);
                //Console.WriteLine("proj.meta>>"+projMetadata.OutputPath);

                // var scriptCode = File.ReadAllText(filePath2);
                var result = engin.ProcessCSXTemplate(templateContent, filePath2,
                resolver, projMetadata);

                // var outputPath = Path.Combine(
                //     Path.GetDirectoryName(filePath),
                //     $"{Path.GetFileNameWithoutExtension(filePath)}{host.FileExtension}");
                // File.WriteAllText(outputPath, result, host.Encoding);
            }

            return 0;
        }

        #endregion
    }
}