using System;
using System.Linq;
using System.Reflection;

namespace WcfTaskGen.Classes.CustomAttributes
{
    class OperationContractAttribute
    {
        private readonly bool asyncPattern = false;

        public OperationContractAttribute(CustomAttributeData data)
        {
            if (!IsType(data))
                throw new ArgumentException();

            asyncPattern = data.NamedArguments.Where(na => na.MemberInfo.Name == "AsyncPattern").Select(na => (bool)na.TypedValue.Value).DefaultIfEmpty(false).FirstOrDefault();
        }

        public bool AsyncPattern
        {
            get { return asyncPattern; }
        }

        public static bool IsType(CustomAttributeData data)
        {
            Type type = data.Constructor.DeclaringType;

            return (type.Namespace == "System.ServiceModel" && type.Name == "OperationContractAttribute");
        }
    }
}