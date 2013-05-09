using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CSharpCodeProvider : CodeProvider
    {
        private static readonly string[] Keywords = new string[] { "abstract", "add", "alias", "as", "ascending", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "descending", "do", "double", "dynamic", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "from", "get", "global", "goto", "group", "if", "implicit", "in", "int", "interface", "internal", "into", "is", "join", "let", "lock", "long", "namespace", "new", "null", "object", "operator", "orderby", "out", "override", "params", "partial", "private", "protected", "public", "readonly", "ref", "remove", "return", "sbyte", "sealed", "select", "set", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "value", "var", "virtual", "void", "volatile", "where", "while", "yield" };

        public CSharpCodeProvider(string indentation)
            : base(indentation)
        {
        }

        protected string GetEscapedName(string name)
        {
            string[] parts = name.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                if (Keywords.Contains(parts[i]))
                    parts[i] = "@" + parts[i];
            }

            return string.Join(".", parts);
        }

        protected override void GenerateType(CodeTypeReference type)
        {
            if (!string.IsNullOrEmpty(type.SimpleName))
            {
                Write(GetEscapedName(type.SimpleName));
                return;
            }

            GenerateType(type.Type);
        }

        protected void GenerateType(Type type)
        {
            if (type.IsArray)
            {
                GenerateType(type.GetElementType());
                Write("[");

                int rank = type.GetArrayRank();
                if (rank > 1)
                    Write(new string(',', rank - 1));

                Write("]");

                return;
            }

            if (type.IsGenericType)
            {
                Type definition = type.GetGenericTypeDefinition();
                string fullName = definition.FullName;
                int index = fullName.IndexOf('`');
                Write(GetEscapedName(fullName.Substring(0, index)));
                Write("<");

                Type[] arguments = type.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++)
                {
                    GenerateType(arguments[i]);

                    if (i < arguments.Length - 1)
                        Write(", ");
                }

                Write(">");
                return;
            }

            Write(GetEscapedName(type.FullName));
        }

        protected override void GenerateParameter(CodeParameter parameter)
        {
            GenerateType(parameter.Type);
            Write(" ");
            Write(GetEscapedName(parameter.Name));
        }

        protected override void GenerateMethod(CodeMethod method, bool isInInterface)
        {
            IncreaseIndentation();
            Write(GetModifier(method.Modifier));
            if (method.Modifier != Modifiers.None)
                Write(" ");

            if (method.IsAsync)
                Write("async ");

            if (method.ReturnType == null)
                Write("void");
            else
                GenerateType(method.ReturnType);
            Write(" ");

            Write(GetEscapedName(method.Name));

            Write("(");

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                GenerateParameter(method.Parameters[i]);

                if (i < method.Parameters.Count - 1)
                    Write(", ");
            }
            Write(")");

            if (isInInterface)
            {
                WriteLine(";");
            }
            else
            {
                WriteLine();
                WriteLine("{");

                if (method.Statement != null)
                {
                    IncreaseIndentation();
                    GenerateFromAsync(method.Statement);
                    DecreaseIndentation();
                }

                WriteLine("}");
            }
            DecreaseIndentation();
        }

        protected override void GenerateClass(CodeClass @class)
        {
            IncreaseIndentation();
            Write(GetModifier(@class.Modifier));
            if (@class.Modifier != Modifiers.None)
                Write(" ");

            if (@class.IsPartial)
                Write("partial ");

            Write(@class.IsInterface ? "interface " : "class ");
            Write(GetEscapedName(@class.Name));

            if (@class.BaseType != null)
            {
                Write(" : ");
                GenerateType(@class.BaseType);
            }

            WriteLine();
            WriteLine("{");

            foreach (CodeMethod method in @class.Methods)
            {
                GenerateMethod(method, @class.IsInterface);
            }

            WriteLine("}");
            DecreaseIndentation();
        }

        protected override void GenerateNamespace(CodeNamespace @namespace)
        {
            Write("namespace ");
            WriteLine(GetEscapedName(@namespace.Name));
            WriteLine("{");

            foreach (CodeClass @class in @namespace.Classes)
            {
                GenerateClass(@class);
            }

            WriteLine("}");
        }

        private string GetModifier(Modifiers modifier)
        {
            switch (modifier)
            {
                case Modifiers.Internal:
                    return "internal";

                case Modifiers.Public:
                    return "public";

                case Modifiers.Private:
                    return "private";

                case Modifiers.None:
                    return String.Empty;

                default:
                    return "?";
            }
        }

        private void GenerateFromAsync(CodeFromAsync statement)
        {
            Write("return System.Threading.Tasks.Task.Factory.FromAsync");

            if (statement.ReturnType == null && statement.Parameters.Count == 0)
            {
                Write("(");
                Write(statement.BeginMethodName);
                Write(", ");
                Write(statement.EndMethodName);
                WriteLine(", null);");
            }
            else if (statement.Parameters.Count < 4)
            {
                Write("<");

                List<CodeTypeReference> types = new List<CodeTypeReference>(statement.Parameters.Select(p => p.Type));
                if (statement.ReturnType != null)
                    types.Add(statement.ReturnType);

                for (int i = 0; i < types.Count; i++)
                {
                    GenerateType(types[i]);

                    if (i < types.Count - 1)
                        Write(", ");
                }

                Write(">(");
                Write(statement.BeginMethodName);
                Write(", ");
                Write(statement.EndMethodName);

                foreach (CodeParameter parameter in statement.Parameters)
                {
                    Write(", ");
                    Write(GetEscapedName(parameter.Name));
                }

                WriteLine(", null);");
            }
            else
            {
                if (statement.ReturnType != null)
                {
                    Write("<");
                    GenerateType(statement.ReturnType);
                    Write(">");
                }

                Write("(");
                Write(statement.BeginMethodName);
                Write("(");

                Write(string.Join(", ", statement.Parameters.Select(p => GetEscapedName(p.Name))));
                Write(", null, null)"); // AsyncCallback callback, object asyncState
                Write(", ");
                Write(statement.EndMethodName);
                WriteLine(");");
            }
        }
    }
}