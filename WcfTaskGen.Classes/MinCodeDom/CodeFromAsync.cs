using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfTaskGen.Classes.MinCodeDom
{
    public class CodeFromAsync
    {
        // FromAsync(Func<AsyncCallback, Object, IAsyncResult>, Action<IAsyncResult>, Object)
        public CodeFromAsync()
        {
            Parameters = new List<CodeParameter>();
        }

        public CodeFromAsync(string beginMethodName, string endMethodName)
            : this()
        {
            BeginMethodName = beginMethodName;
            EndMethodName = endMethodName;
        }

        public CodeFromAsync(string beginMethodName, string endMethodName, CodeTypeReference returnType, IEnumerable<CodeParameter> parameters)
            : this()
        {
            BeginMethodName = beginMethodName;
            EndMethodName = endMethodName;
            ReturnType = returnType;
            Parameters.AddRange(parameters);
        }

        public string BeginMethodName { get; set; }
        public string EndMethodName { get; set; }
        public CodeTypeReference ReturnType { get; set; }
        public List<CodeParameter> Parameters { get; private set; }
    }
}