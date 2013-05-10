using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        public CodeTypeReference ExtendedType { get; set; }
        public Modifiers Modifier { get; set; }
        public CodeTypeReference ReturnType { get; set; }
        public string Name { get; set; }
        public List<CodeParameter> Parameters { get; private set; }
        public CodeFromAsync Statement { get; set; }
        public CodeTypeReference ImplementationClass { get; set; }
        public string ImplementationMethod { get; set; }

        public string GetExtendedParameterName()
        {
            string parameterName = "instance";

            int index = 1;

            while (Parameters.Any(p => string.Equals(p.Name, parameterName, StringComparison.OrdinalIgnoreCase)))
            {
                parameterName = String.Format(CultureInfo.InvariantCulture, "parameter{0}", index);
                index++;
            }

            return parameterName;
        }
    }
}