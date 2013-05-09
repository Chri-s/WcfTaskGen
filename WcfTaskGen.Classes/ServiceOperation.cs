using System;
using System.Reflection;
using WcfTaskGen.Classes.CustomAttributes;

namespace WcfTaskGen.Classes
{
    public class ServiceOperation : Operation
    {
        private readonly bool isAsyncPattern;

        internal ServiceOperation(MethodInfo method, OperationContractAttribute attribute, InterfaceDescription interfaceDescription)
            : base(method, interfaceDescription)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute", "attribute is null.");

            this.isAsyncPattern = attribute.AsyncPattern;
        }

        public bool IsAsyncPattern
        {
            get { return isAsyncPattern; }
        }
    }
}