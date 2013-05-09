using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CodeTypeReference
    {
        public CodeTypeReference(Type type)
        {
            Type = type;
        }

        public CodeTypeReference(string simpleName)
        {
            SimpleName = simpleName;
        }

        public CodeTypeReference()
        {
        }

        public Type Type { get; set; }
        public string SimpleName { get; set; }
    }
}