using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using WcfTaskGen.Classes.CustomAttributes;
using WcfTaskGen.Classes.MinCodeDom;

namespace WcfTaskGen.Classes
{
    public class InterfaceDescription
    {
        private readonly Type type;
        private readonly List<ServiceOperation> serviceOperations = new List<ServiceOperation>();
        private readonly List<ServiceOperation> asyncPatternServiceOperations = new List<ServiceOperation>();
        private readonly List<AsyncOperationPair> asyncOperationPairs = new List<AsyncOperationPair>();
        private readonly List<Operation> endOperations = new List<Operation>();
        private readonly Modifiers modifier;

        internal InterfaceDescription(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type", "type is null.");

            this.type = type;
            this.modifier = Type.IsPublic ? Modifiers.Public : Modifiers.Internal;
            Initialize();
        }

        public Type Type
        {
            get { return this.type; }
        }

        public bool IsPublic
        {
            get { return this.type.IsPublic; }
        }

        public Modifiers Modifier
        {
            get { return this.modifier; }
        }

        public List<ServiceOperation> ServiceOperations
        {
            get { return this.serviceOperations; }
        }

        public List<ServiceOperation> AsyncPatternServiceOperations
        {
            get { return this.asyncPatternServiceOperations; }
        }

        public List<AsyncOperationPair> AsyncOperationPairs
        {
            get { return asyncOperationPairs; }
        }

        private void Initialize()
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.IsPrivate)
                    continue;

                OperationContractAttribute attribute = (OperationContractAttribute)method.GetCustomAttributesData().GetKnownAttributes().OfType<OperationContractAttribute>().FirstOrDefault();

                if (attribute == null)
                {
                    if (method.Name.StartsWith("End") && !endOperations.Any(o => o.Name == method.Name))
                        endOperations.Add(new Operation(method, this));

                    continue;
                }

                ServiceOperation serviceOperation = new ServiceOperation(method, attribute, this);
                serviceOperations.Add(serviceOperation);

                if (serviceOperation.IsAsyncPattern)
                    asyncPatternServiceOperations.Add(serviceOperation);
            }

            foreach (ServiceOperation beginOperation in asyncPatternServiceOperations)
            {
                if (beginOperation.Name.Length < 6)
                    continue;

                string endName = "End" + beginOperation.Name.Substring(5);
                Operation endOperation = endOperations.FirstOrDefault(eo => eo.Name == endName);

                if (endOperation == null)
                    continue;

                AsyncOperationPair pair = new AsyncOperationPair(beginOperation, endOperation);
                if (!pair.IsValidPair())
                    continue;

                pair.Initialize();
                asyncOperationPairs.Add(pair);
            }
        }
    }
}