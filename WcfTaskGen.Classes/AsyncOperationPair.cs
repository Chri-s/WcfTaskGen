using System;
using System.Linq;
using System.Reflection;

namespace WcfTaskGen.Classes
{
    public class AsyncOperationPair
    {
        private readonly ServiceOperation beginOperation;
        private readonly Operation endOperation;
        private Type returnType;
        private ParameterInfo[] parameters;
        private string name;
        private bool containsRefOrOutParameters;

        internal AsyncOperationPair(ServiceOperation beginOperation, Operation endOperation)
        {
            if (beginOperation == null)
                throw new ArgumentNullException("beginOperation", "beginOperation is null.");
            if (endOperation == null)
                throw new ArgumentNullException("endOperation", "endOperation is null.");

            this.beginOperation = beginOperation;
            this.endOperation = endOperation;
        }

        public ServiceOperation BeginOperation
        {
            get { return beginOperation; }
        }

        public Operation EndOperation
        {
            get { return endOperation; }
        }

        public Type ReturnType
        {
            get { return returnType; }
        }

        public ParameterInfo[] Parameters
        {
            get { return parameters; }
        }

        public string Name
        {
            get { return name; }
        }

        public bool ContainsRefOrOutParameters
        {
            get { return containsRefOrOutParameters; }
        }

        internal bool IsValidPair()
        {
            if (!BeginOperation.IsAsyncPattern)
                return false;

            if (!BeginOperation.Name.StartsWith("Begin") || !EndOperation.Name.StartsWith("End"))
                return false;

            if (BeginOperation.Name.Length != EndOperation.Name.Length + 2 || BeginOperation.Name.Length == 5 || BeginOperation.Name.Substring(5) != EndOperation.Name.Substring(3))
                return false;

            if (BeginOperation.ReturnType == null || !BeginOperation.ReturnType.Equals(typeof(IAsyncResult)))
                return false;

            if (BeginOperation.Parameters.Length < 2)
                return false;

            if (!BeginOperation.Parameters[BeginOperation.Parameters.Length - 2].ParameterType.Equals(typeof(AsyncCallback)))
                return false;

            if (!BeginOperation.Parameters[BeginOperation.Parameters.Length - 1].ParameterType.Equals(typeof(object)))
                return false;

            if (EndOperation.Parameters.Length != 1)
                return false;

            if (!EndOperation.Parameters[0].ParameterType.Equals(typeof(IAsyncResult)))
                return false;

            return true;
        }

        internal void Initialize()
        {
            returnType = EndOperation.ReturnType;
            parameters = BeginOperation.Parameters.Take(BeginOperation.Parameters.Length - 2).ToArray();
            name = BeginOperation.Name.Substring(5);
            containsRefOrOutParameters = BeginOperation.ContainsRefOrOutParameters || EndOperation.ContainsRefOrOutParameters;
        }
    }
}