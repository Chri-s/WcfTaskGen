using System;
using CommandLine;
using CommandLine.Text;

namespace WcfTaskGen
{
    class Options
    {
        [Option('a', "assembly", Required = true, HelpText = "The path to the assembly containing the interface.")]
        public string AssemblyPath { get; set; }

        [Option('i', "interface", Required = true, HelpText = "The name (including the namespace) of the WCF interface type.")]
        public string InterfaceType { get; set; }

        [Option('o', "output", Required = true, HelpText = "The path of the generated code file.")]
        public string OutputPath { get; set; }

        [Option('l', "language", DefaultValue = "C#", HelpText = "The programming language of the output. 'C#' or 'VB'.")]
        public string Language { get; set; }

        internal Languages ParsedLanguage { get; private set; }

        [Option('t', "type", DefaultValue = "Extension", HelpText = "The type of generated code. Extension or Interface.")]
        public string GeneratedType { get; set; }

        [Option("indentation", DefaultValue = 4, HelpText = "The number of spaces which are used for indentation.")]
        public int Indentation { get; set; }

        internal GenerationTypes ParsedGeneratedType { get; private set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public bool AreOptionsValid()
        {
            if (Language.Equals("C#", StringComparison.OrdinalIgnoreCase))
            {
                ParsedLanguage = Languages.CSharp;
            }
            else if (Language.Equals("VB", StringComparison.OrdinalIgnoreCase))
            {
                ParsedLanguage = Languages.VB;
            }
            else
            {
                Console.WriteLine("ERROR: Invalid value for /language. Must be C# or VB.");
                return false;
            }

            if (GeneratedType.Equals("Extension", StringComparison.OrdinalIgnoreCase) || GeneratedType.Equals("E", StringComparison.OrdinalIgnoreCase))
            {
                ParsedGeneratedType = GenerationTypes.Extensions;
            }
            else if (GeneratedType.Equals("Interface", StringComparison.OrdinalIgnoreCase) || GeneratedType.Equals("I", StringComparison.OrdinalIgnoreCase))
            {
                ParsedGeneratedType = GenerationTypes.Interface;
            }
            else
            {
                Console.WriteLine("ERROR: Invalid value for /type. Must be Extension or Interface.");
                return false;
            }

            if (Indentation < 0)
            {
                Console.WriteLine("ERROR: Indentation must be at least 0.");
                return false;
            }

            return true;
        }
    }
}