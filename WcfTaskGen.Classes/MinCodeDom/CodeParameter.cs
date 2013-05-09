using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CodeParameter
    {
        public CodeParameter(CodeTypeReference type, string name)
        {
            Type = type;
            Name = name;
        }

        public CodeParameter()
        {
        }

        public CodeTypeReference Type { get; set; }
        public string Name { get; set; }
    }
}