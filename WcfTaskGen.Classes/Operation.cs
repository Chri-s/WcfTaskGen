using System;
using System.Linq;
using System.Reflection;

namespace WcfTaskGen.Classes
{
    public class Operation
    {
        private readonly MethodInfo method;
        private readonly Type returnType;
        private readonly string name;
        private readonly InterfaceDescription interfaceDescription;
        private readonly bool containsRefOrOutParameters;

        internal Operation(MethodInfo method, InterfaceDescription interfaceDescription)
        {
            if (method == null)
                throw new ArgumentNullException("method", "method is null.");
            if (interfaceDescription == null)
                throw new ArgumentNullException("interfaceDescription", "interfaceDescription is null.");

            this.method = method;
            this.interfaceDescription = interfaceDescription;
            returnType = (method.ReturnType == typeof(void)) ? null : method.ReturnType;
            name = method.Name;
            containsRefOrOutParameters = method.GetParameters().Any(p => p.ParameterType.IsByRef);
        }

        public InterfaceDescription InterfaceDescription
        {
            get { return interfaceDescription; }
        }

        public Type ReturnType
        {
            get { return returnType; }
        }

        public ParameterInfo[] Parameters
        {
            get { return method.GetParameters(); }
        }

        public string Name
        {
            get { return name; }
        }

        public bool ContainsRefOrOutParameters
        {
            get { return containsRefOrOutParameters; }
        }
    }
}