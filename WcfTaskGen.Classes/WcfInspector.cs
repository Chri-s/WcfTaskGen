using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WcfTaskGen.Classes.CustomAttributes;

namespace WcfTaskGen.Classes
{
    public class WcfInspector
    {
        private readonly Assembly assembly;
        private readonly List<InterfaceDescription> interfaceDescriptions = new List<InterfaceDescription>();

        public WcfInspector(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly", "assembly is null.");

            this.assembly = assembly;
        }

        public IList<InterfaceDescription> InterfaceDescriptions
        {
            get { return interfaceDescriptions; }
        }

        public void SearchAllInterfaces()
        {
            foreach (Type type in assembly.GetTypes())
            {
                AddType(type);
            }
        }

        public bool SearchType(string name)
        {
            Type type = assembly.GetType(name, false);

            if (type == null)
                return false;

            AddType(type);
            return true;
        }

        public void AddType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type", "type is null.");

            if (type.IsNestedPrivate || type.IsNestedFamily)
                return;

            if (type.IsAbstract && !type.IsInterface)
                return;

            if (!type.GetCustomAttributesData().GetKnownAttributes().Any(cad => cad.GetType().Equals(typeof(ServiceContractAttribute))))
                return;

            if (interfaceDescriptions.Any(i => i.Type == type))
                return;

            interfaceDescriptions.Add(new InterfaceDescription(type));
        }
    }
}