using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WcfTaskGen.Classes.CustomAttributes
{
    class ServiceContractAttribute
    {
        public ServiceContractAttribute(CustomAttributeData data)
        {
            if (!IsType(data))
                throw new ArgumentException();
        }

        public static bool IsType(CustomAttributeData data)
        {
            Type type = data.Constructor.DeclaringType;

            return (type.Namespace == "System.ServiceModel" && type.Name == "ServiceContractAttribute");
        }
    }
}