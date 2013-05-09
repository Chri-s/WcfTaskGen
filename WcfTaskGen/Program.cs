using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WcfTaskGen.Classes;
using WcfTaskGen.Classes.MinCodeDom;

namespace WcfTaskGen
{
    class Program
    {
        static string assemblyPath;

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine(@"Usage: WcfTaskGen.exe <Assembly path> <Interface type> <Output path> <Language (C# or VB)>");
                return;
            }

            string assemblyPath = args[0].Trim();
            string interfaceType = args[1].Trim();
            string outputPath = args[2].Trim();
            string language = args[3].Trim();

            try
            {
                if (!File.Exists(assemblyPath))
                {
                    Console.WriteLine("ERROR: The specified assembly path does not exist.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: The specified assembly path could not be accessed: " + ex.Message);
                return;
            }

            if (Path.GetInvalidPathChars().Any(c => outputPath.Contains(c.ToString())))
            {
                Console.WriteLine("ERROR: The specified output path contains invalid characters.");
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
            {
                Console.WriteLine("ERROR: The directory of the specified output path does not exist.");
                return;
            }

            if (!(string.Equals("C#", language, StringComparison.OrdinalIgnoreCase) || string.Equals("VB", language, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("ERROR: Language must be C# or VB.");
                return;
            }

            Assembly assembly;
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
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
                if (!inspector.SearchType(interfaceType))
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
                stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Unable to open the output file: " + ex.Message);
                return;
            }

            CodeProvider codeProvider = language.Equals("C#", StringComparison.OrdinalIgnoreCase) ? (CodeProvider)new CSharpCodeProvider("    ") : new VBCodeProvider("    ");

            using (stream)
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
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