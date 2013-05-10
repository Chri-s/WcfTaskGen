using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WcfTaskGen.Classes.MinCodeDom;

namespace WcfTaskGen.Classes
{
    public static class ExtensionClassGenerator
    {
        public static void CreateExtensionClass(IEnumerable<AsyncOperationPair> pairs, InterfaceDescription interfaceDescription, CodeProvider codeProvider, TextWriter writer)
        {
            if (pairs == null)
                throw new ArgumentNullException("pairs", "pairs is null.");
            if (interfaceDescription == null)
                throw new ArgumentNullException("interfaceDescription", "interfaceDescription is null.");
            if (codeProvider == null)
                throw new ArgumentNullException("codeProvider", "codeProvider is null.");
            if (writer == null)
                throw new ArgumentNullException("writer", "writer is null.");

            string className = interfaceDescription.Type.Name.Substring(1) + "TaskAsyncExtensions";
            string classFullName = interfaceDescription.Type.Namespace + "." + className;

            CodeNamespace ns = new CodeNamespace(interfaceDescription.Type.Namespace);
            CodeClass @class = new CodeClass(className) { Modifier = interfaceDescription.Modifier, Type = ClassType.ExtensionClass };
            ns.Classes.Add(@class);

            foreach (AsyncOperationPair pair in pairs.Where(p => !p.ContainsRefOrOutParameters))
            {
                CodeMethod method = GetMethodDefinition(pair, Modifiers.Public, new CodeTypeReference(interfaceDescription.Type));

                method.Statement = new CodeFromAsync(pair.BeginOperation.Name, pair.EndOperation.Name, null, pair.Parameters.Select(p => new CodeParameter(new CodeTypeReference(p.ParameterType), p.Name)));
                if (pair.ReturnType != null)
                    method.Statement.ReturnType = new CodeTypeReference(pair.ReturnType);

                @class.Methods.Add(method);
            }

            codeProvider.GenerateCode(ns, writer);
        }

        public static string CreateExtensionClass(IEnumerable<AsyncOperationPair> pairs, InterfaceDescription interfaceDescription, CodeProvider codeProvider)
        {
            using (StringWriter writer = new StringWriter())
            {
                CreateExtensionClass(pairs, interfaceDescription, codeProvider, writer);
                return writer.ToString();
            }
        }

        private static CodeMethod GetMethodDefinition(AsyncOperationPair pair, Modifiers modifier, CodeTypeReference extendedType)
        {
            CodeMethod method = new CodeMethod()
            {
                Name = pair.Name + "TaskAsync",
                Modifier = modifier,
                ExtendedType = extendedType
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