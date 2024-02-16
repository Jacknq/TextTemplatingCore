using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Emit;

namespace TextTemplating.Infrastructure
{
    public class CommandLineEngineHost : ITextTemplatingEngineHost
    {

        public string FileExtension { get; set; } = ".cs";

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public string TemplateFilePath { get; }

        public void SetFileExtension(string extension) => FileExtension = extension;

        public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective) => Encoding = encoding;

        /// <inheritdoc />
        /// <summary>
        /// Share reference with project
        /// </summary>
        public IList<string> StandardAssemblyReferences { get; } = new List<string>()
        {
            "mscorlib",
           // "netstandard",
            "System",
            "System.Console",
            "System.Core",
            "System.Linq",
            "System.Linq.Queryable",
            "System.Linq.Expressions",
             "System.Linq.Parallel",
             "System.Data",
             "System.Data.DataSetExtensions",           
            "System.Dynamic",
            "System.Runtime"
            ,"System.Reflection"
           //   "DesignTimeSample" //wil be ot found
           //   "TextTemplating.Core" //works but creates dependency
            
        };

        public IList<string> StandardImports { get; } = new List<string>
        {
            "System",
            "System.Linq",
            "System.Linq.Expressions",
            "System.IO",
            "System.Collections.Generic",
            //"System.Console",
            "System.Diagnostics",
            "System.Dynamic",         
            "System.Text",
            "System.Threading.Tasks",
            "TextTemplating",
            "TextTemplating.Infrastructure",
            "TextTemplating.T4.Parsing",
            "TextTemplating.T4.Preprocessing"
        };

        public void LogErrors(EmitResult result)
        {
            if (!result.Success)
            {
                throw new InvalidOperationException(string.Format("Build failed. Diagnostics: {0}", string.Join(Environment.NewLine, result.Diagnostics)));
            }
        }

        public string LoadIncludeFile(string fileName)
        {
            if (Path.IsPathRooted(fileName))
            {
                return File.ReadAllText(fileName);
            }

            if(string.IsNullOrWhiteSpace(TemplateFilePath))
                return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, fileName));
            else
                return File.ReadAllText(Path.Combine(Path.GetDirectoryName(TemplateFilePath), fileName));
        }

        public string ResolvePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(Path.GetDirectoryName(TemplateFilePath), path);
        }


    }
}
