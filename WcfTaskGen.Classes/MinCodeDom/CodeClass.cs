using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CodeClass
    {
        public CodeClass()
        {
            Methods = new List<CodeMethod>();
        }

        public CodeClass(string name)
            : this()
        {
            Name = name;
        }

        public bool IsInterface { get; set; }
        public Modifiers Modifier { get; set; }
        public bool IsPartial { get; set; }
        public string Name { get; set; }
        public CodeTypeReference BaseType { get; set; }
        public List<CodeMethod> Methods { get; private set; }
    }
}