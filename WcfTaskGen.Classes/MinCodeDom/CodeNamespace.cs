using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CodeNamespace
    {
        public CodeNamespace()
        {
            Classes = new List<CodeClass>();
        }

        public CodeNamespace(string name)
            : this()
        {
            Name = name;
        }

        public string Name { get; set; }
        public List<CodeClass> Classes { get; private set; }
    }
}