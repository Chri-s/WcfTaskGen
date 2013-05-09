using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace WcfTaskGen.Classes.CustomAttributes
{
    static class CustomAttributeDataExtensions
    {
        public static IEnumerable<object> GetKnownAttributes(this IEnumerable<CustomAttributeData> data)
        {
            foreach (CustomAttributeData item in data)
            {
                object knownAttribute = GetKnownAttribute(item);

                if (knownAttribute == null)
                    continue;

                yield return knownAttribute;
            }
        }

        private static object GetKnownAttribute(CustomAttributeData data)
        {
            if (ServiceContractAttribute.IsType(data))
                return new ServiceContractAttribute(data);

            if (OperationContractAttribute.IsType(data))
                return new OperationContractAttribute(data);

            return null;
        }
    }
}