using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class VBCodeProvider : CodeProvider
    {
        private static readonly string[] Keywords = new string[] { "AddHandler", "AddressOf", "Alias", "And", "AndAlso", "As", "Boolean", "ByRef", "Byte", "ByVal", "Call", "Case", "Catch", "CBool", "CByte", "CChar", "CDate", "CDbl", "CDec", "Char", "CInt", "Class", "CLng", "CObj", "Const", "Continue", "CSByte", "CShort", "CSng", "CStr", "CType", "CUInt", "CULng", "CUShort", "Date", "Decimal", "Declare", "Default", "Delegate", "Dim", "DirectCast", "Do", "Double", "Each", "Else", "ElseIf", "End", "EndIf", "Enum", "Erase", "Error", "Event", "Exit", "False", "Finally", "For", "Friend", "Function", "Get", "GetType", "GetXMLNamespace", "Global", "GoSub", "GoTo", "Handles", "If", "Implements", "Imports", "In", "Inherits", "Integer", "Interface", "Is", "IsNot", "Let", "Lib", "Like", "Long", "Loop", "Me", "Mod", "Module", "Module Statement", "MustInherit", "MustOverride", "MyBase", "MyClass", "Namespace", "Narrowing", "New", "Next", "Not", "Nothing", "NotInheritable", "NotOverridable", "Object", "Of", "On", "Operator", "Option", "Optional", "Or", "OrElse", "Out", "Overloads", "Overridable", "Overrides", "ParamArray", "Partial", "Private", "Property", "Protected", "Public", "RaiseEvent", "ReadOnly", "ReDim", "REM", "RemoveHandler", "Resume", "Return", "SByte", "Select", "Set", "Shadows", "Shared", "Short", "Single", "Static", "Step", "Stop", "String", "Structure", "Structure", "Sub", "SyncLock", "Then", "Throw", "To", "True", "Try", "TryCast", "TypeOf…Is", "UInteger", "ULong", "UShort", "Using", "Variant", "Wend", "When", "While", "Widening", "With", "WithEvents", "WriteOnly", "Xor" };

        public VBCodeProvider(string indentation)
            : base(indentation)
        {
        }

        protected override void GenerateType(CodeTypeReference type)
        {
            if (!string.IsNullOrEmpty(type.SimpleName))
            {
                Write("Global.");
                Write(GetEscapedName(type.SimpleName));
                return;
            }

            GenerateType(type.Type);
        }

        protected override string GetEscapedName(string name)
        {
            string[] parts = name.Split('.');

            for (int i = 0; i < parts.Length; i++)
            {
                if (Keywords.Any(k => string.Equals(k, parts[i], StringComparison.OrdinalIgnoreCase)))
                    parts[i] = "[" + parts[i] + "]";
            }

            return string.Join(".", parts);
        }

        protected override void GenerateType(Type type)
        {
            if (type.IsArray)
            {
                GenerateType(type.GetElementType());
                Write("(");

                int rank = type.GetArrayRank();
                if (rank > 1)
                    Write(new string(',', rank - 1));

                Write(")");

                return;
            }

            if (type.IsGenericType)
            {
                Type definition = type.GetGenericTypeDefinition();
                string fullName = definition.FullName;
                int index = fullName.IndexOf('`');
                Write(GetEscapedName(fullName.Substring(0, index)));
                Write("(Of ");

                Type[] arguments = type.GetGenericArguments();
                for (int i = 0; i < arguments.Length; i++)
                {
                    GenerateType(arguments[i]);

                    if (i < arguments.Length - 1)
                        Write(", ");
                }

                Write(")");
                return;
            }

            Write("Global.");
            Write(GetEscapedName(type.FullName));
        }

        protected override void GenerateParameter(CodeParameter parameter)
        {
            Write("ByVal ");
            Write(GetEscapedName(parameter.Name));
            Write(" As ");
            GenerateType(parameter.Type);
        }

        protected override void GenerateMethod(CodeMethod method, bool isInInterface)
        {
            IncreaseIndentation();
            Write(GetModifier(method.Modifier));
            if (method.Modifier != Modifiers.None)
                Write(" ");

            if (method.IsAsync)
                Write("Async ");

            if (method.ReturnType == null)
                Write("Sub ");
            else
                Write("Function ");

            Write(GetEscapedName(method.Name));

            Write("(");

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                GenerateParameter(method.Parameters[i]);

                if (i < method.Parameters.Count - 1)
                    Write(", ");
            }
            Write(")");

            if (method.ReturnType != null)
            {
                Write(" As ");
                GenerateType(method.ReturnType);
            }

            if (method.ImplementationClass != null && !string.IsNullOrEmpty(method.ImplementationMethod))
            {
                Write(" Implements ");
                GenerateType(method.ImplementationClass);
                Write(".");
                Write(GetEscapedName(method.ImplementationMethod));
            }

            WriteLine();

            if (!isInInterface)
            {
                if (method.Statement != null)
                {
                    IncreaseIndentation();
                    GenerateFromAsync(method.Statement);
                    DecreaseIndentation();
                }

                if (method.ReturnType == null)
                    WriteLine("End Sub");
                else
                    WriteLine("End Function");
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
                Write("Partial ");

            Write(@class.IsInterface ? "Interface " : "Class ");
            Write(GetEscapedName(@class.Name));

            if (@class.BaseType != null)
            {
                WriteLine();
                IncreaseIndentation();

                if (@class.IsInterface)
                    Write("Inherits ");
                else
                    Write("Implements ");

                GenerateType(@class.BaseType);
                DecreaseIndentation();
            }

            WriteLine();
            WriteLine();

            foreach (CodeMethod method in @class.Methods)
            {
                GenerateMethod(method, @class.IsInterface);
            }

            if (@class.IsInterface)
                WriteLine("End Interface");
            else
                WriteLine("End Class");

            DecreaseIndentation();
        }

        protected override void GenerateNamespace(CodeNamespace @namespace)
        {
            Write("Namespace Global.");
            WriteLine(GetEscapedName(@namespace.Name));

            foreach (CodeClass @class in @namespace.Classes)
            {
                GenerateClass(@class);
            }

            WriteLine("End Namespace");
        }

        private string GetModifier(Modifiers modifier)
        {
            switch (modifier)
            {
                case Modifiers.Internal:
                    return "Friend";

                case Modifiers.Public:
                    return "Public";

                case Modifiers.Private:
                    return "Private";

                case Modifiers.None:
                    return String.Empty;

                default:
                    return "?";
            }
        }

        private void GenerateFromAsync(CodeFromAsync statement)
        {
            Write("Return System.Threading.Tasks.Task.Factory.FromAsync");

            if (statement.ReturnType == null && statement.Parameters.Count == 0)
            {
                Write("(AddressOf ");
                Write(statement.BeginMethodName);
                Write(", AddressOf ");
                Write(statement.EndMethodName);
                WriteLine(", Nothing)");
            }
            else if (statement.Parameters.Count < 4)
            {
                Write("(Of ");

                List<CodeTypeReference> types = new List<CodeTypeReference>(statement.Parameters.Select(p => p.Type));
                if (statement.ReturnType != null)
                    types.Add(statement.ReturnType);

                for (int i = 0; i < types.Count; i++)
                {
                    GenerateType(types[i]);

                    if (i < types.Count - 1)
                        Write(", ");
                }

                Write(")(AddressOf ");
                Write(statement.BeginMethodName);
                Write(", AddressOf ");
                Write(statement.EndMethodName);

                foreach (CodeParameter parameter in statement.Parameters)
                {
                    Write(", ");
                    Write(GetEscapedName(parameter.Name));
                }

                WriteLine(", Nothing)");
            }
            else
            {
                if (statement.ReturnType != null)
                {
                    Write("(Of ");
                    GenerateType(statement.ReturnType);
                    Write(")");
                }

                Write("(");
                Write(statement.BeginMethodName);
                Write("(");

                Write(string.Join(", ", statement.Parameters.Select(p => GetEscapedName(p.Name))));
                Write(", Nothing, Nothing)"); // AsyncCallback callback, object asyncState
                Write(", AddressOf ");
                Write(statement.EndMethodName);
                WriteLine(")");
            }
        }
    }
}