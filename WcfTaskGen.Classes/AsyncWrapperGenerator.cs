using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WcfTaskGen.Classes.MinCodeDom;

namespace WcfTaskGen.Classes
{
    public static class AsyncWrapperGenerator
    {
        public static void CreateAsyncWrapper(IEnumerable<AsyncOperationPair> pairs, InterfaceDescription interfaceDescription, CodeProvider codeProvider, TextWriter writer)
        {
            if (pairs == null)
                throw new ArgumentNullException("pairs", "pairs is null.");
            if (interfaceDescription == null)
                throw new ArgumentNullException("interfaceDescription", "interfaceDescription is null.");
            if (codeProvider == null)
                throw new ArgumentNullException("codeProvider", "codeProvider is null.");
            if (writer == null)
                throw new ArgumentNullException("writer", "writer is null.");

            string interfaceName = interfaceDescription.Type.Name + "TaskAsync";
            string interfaceFullName = interfaceDescription.Type.Namespace + "." + interfaceName;

            CodeNamespace ns = new CodeNamespace(interfaceDescription.Type.Namespace);
            CodeClass @class = new CodeClass(interfaceName) { Modifier = interfaceDescription.Modifier, IsInterface = true, BaseType = new CodeTypeReference(interfaceDescription.Type) };
            ns.Classes.Add(@class);

            foreach (AsyncOperationPair pair in pairs.Where(p => !p.ContainsRefOrOutParameters))
            {
                CodeMethod method = GetMethodDefinition(pair, Modifiers.None);
                @class.Methods.Add(method);
            }

            @class = new CodeClass(interfaceDescription.Type.Name.Substring(1) + "Client") { Modifier = interfaceDescription.Modifier, IsPartial = true, BaseType = new CodeTypeReference(interfaceFullName) };
            ns.Classes.Add(@class);
            foreach (AsyncOperationPair pair in pairs.Where(p => !p.ContainsRefOrOutParameters))
            {
                CodeMethod method = GetMethodDefinition(pair, Modifiers.Public);

                method.Statement = new CodeFromAsync(pair.BeginOperation.Name, pair.EndOperation.Name, null, pair.Parameters.Select(p => new CodeParameter(new CodeTypeReference(p.ParameterType), p.Name)));
                if (pair.ReturnType != null)
                    method.Statement.ReturnType = new CodeTypeReference(pair.ReturnType);

                method.ImplementationClass = new CodeTypeReference(interfaceFullName);
                method.ImplementationMethod = method.Name;

                @class.Methods.Add(method);
            }

            codeProvider.GenerateCode(ns, writer);
        }

        public static string CreateAsyncWrapper(IEnumerable<AsyncOperationPair> pairs, InterfaceDescription interfaceDescription, CodeProvider codeProvider)
        {
            using (StringWriter writer = new StringWriter())
            {
                CreateAsyncWrapper(pairs, interfaceDescription, codeProvider, writer);
                return writer.ToString();
            }
        }

        private static CodeMethod GetMethodDefinition(AsyncOperationPair pair, Modifiers modifier)
        {
            CodeMethod method = new CodeMethod()
                            {
                                Name = pair.Name + "TaskAsync",
                                Modifier = modifier,

                            };
            if (pair.ReturnType == null)
                method.ReturnType = new CodeTypeReference(typeof(Task));
            else
                method.ReturnType = new CodeTypeReference(typeof(Task<>).MakeGenericType(pair.ReturnType));

            foreach (ParameterInfo parameter in pair.Parameters)
            {
                method.Parameters.Add(new CodeParameter(new CodeTypeReference(parameter.ParameterType), parameter.Name));
            }

            return method;
        }
    }
}