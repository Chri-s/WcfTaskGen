using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WcfTaskGen.Classes;
using WcfTaskGen.Classes.MinCodeDom;
using CommandLine;

namespace WcfTaskGen
{
    class Program
    {
        static string assemblyPath;

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options) || !options.AreOptionsValid())
            {
                return;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(options.AssemblyPath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR: The specified assembly path does not exist.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load the assembly: " + ex.Message);
                return;
            }

            Program.assemblyPath = assembly.Location;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            WcfInspector inspector = new WcfInspector(assembly);

            try
            {
                if (!inspector.SearchType(options.InterfaceType))
                {
                    Console.WriteLine("ERROR: The interface type does not exist in the specified assembly.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to load the specified interface type: " + ex.Message);
                return;
            }

            if (inspector.InterfaceDescriptions.Count == 0)
            {
                Console.WriteLine("ERROR: The specified type was found, but is no WCF service contract.");
                return;
            }

            FileStream stream;

            try
            {
                stream = new FileStream(options.OutputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to open the output file: " + ex.Message);
                return;
            }

            CodeProvider codeProvider = (options.ParsedLanguage == Languages.CSharp) ? (CodeProvider)new CSharpCodeProvider(new string(' ', options.Indentation)) : new VBCodeProvider(new string(' ', options.Indentation));

            using (stream)
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                if (options.ParsedGeneratedType == GenerationTypes.Extensions)
                    ExtensionClassGenerator.CreateExtensionClass(inspector.InterfaceDescriptions[0].AsyncOperationPairs, inspector.InterfaceDescriptions[0], codeProvider, writer);
                else
                    AsyncWrapperGenerator.CreateAsyncWrapper(inspector.InterfaceDescriptions[0].AsyncOperationPairs, inspector.InterfaceDescriptions[0], codeProvider, writer);
            }
        }

        static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName name = new AssemblyName(args.Name);
            String asmToCheck = Path.GetDirectoryName(assemblyPath) + "\\" + name.Name + ".dll";

            if (File.Exists(asmToCheck))
            {
                return Assembly.ReflectionOnlyLoadFrom(asmToCheck);
            }

            return Assembly.ReflectionOnlyLoad(name.FullName);
        }
    }
}