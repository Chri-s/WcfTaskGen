using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CodeMethod
    {
        public CodeMethod()
        {
            Parameters = new List<CodeParameter>();
        }

        public CodeMethod(string name)
            : this()
        {
            Name = name;
        }

        public Modifiers Modifier { get; set; }
        public bool IsAsync { get; set; }
        public CodeTypeReference ReturnType { get; set; }
        public string Name { get; set; }
        public List<CodeParameter> Parameters { get; private set; }
        public CodeFromAsync Statement { get; set; }
        public CodeTypeReference ImplementationClass { get; set; }
        public string ImplementationMethod { get; set; }
    }
}