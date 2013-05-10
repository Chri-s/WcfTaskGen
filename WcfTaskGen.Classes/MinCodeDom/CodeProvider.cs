using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public abstract class CodeProvider
    {
        public CodeProvider(string indentation)
        {
            this.indentation = indentation;
        }

        private TextWriter writer;
        private readonly string indentation;
        private int indentationLevel;
        private string currentIndentation;
        private bool isAtBeginningOfLine;

        public void GenerateCode(CodeNamespace @namespace, TextWriter writer)
        {
            if (@namespace == null)
                throw new ArgumentNullException("namespace", "namespace is null.");
            if (writer == null)
                throw new ArgumentNullException("writer", "writer is null.");

            this.writer = writer;
            this.indentationLevel = 0;
            this.currentIndentation = String.Empty;
            this.isAtBeginningOfLine = true;

            GenerateNamespace(@namespace);
        }

        protected abstract string GetEscapedName(string name);
        protected abstract void GenerateType(Type type);

        protected virtual void GenerateType(CodeTypeReference type)
        {
            if (!string.IsNullOrEmpty(type.SimpleName))
            {
                Write(GetEscapedName(type.SimpleName));
                return;
            }

            GenerateType(type.Type);
        }

        protected abstract void GenerateNamespace(CodeNamespace @namespace);

        protected void WriteExtensionParameterNameIfNeeded(string extensionParameterName)
        {
            if (extensionParameterName == null)
                return;

            Write(GetEscapedName(extensionParameterName));
            Write(".");
        }

        protected void IncreaseIndentation()
        {
            indentationLevel++;

            currentIndentation = string.Concat(Enumerable.Repeat(indentation, indentationLevel));
        }

        protected void DecreaseIndentation()
        {
            indentationLevel--;
            if (indentationLevel <= 0)
            {
                indentationLevel = 0;
                currentIndentation = String.Empty;
                return;
            }

            currentIndentation = string.Concat(Enumerable.Repeat(indentation, indentationLevel));
        }

        protected void Write(string s)
        {
            if (isAtBeginningOfLine)
            {
                writer.Write(currentIndentation);
                isAtBeginningOfLine = false;
            }

            writer.Write(s);
        }

        protected void WriteLine()
        {
            writer.WriteLine();
            isAtBeginningOfLine = true;
        }

        protected void WriteLine(string s)
        {
            Write(s);
            WriteLine();
        }
    }
}