using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;

namespace ScriptingEngine
{
    [Serializable]
    public class CompiledScript : MarshalByRefObject
    {
        internal CompiledScript(string startFile, CompilerResults compiledResults)
        {
            this.Errors = new CompilerError[compiledResults.Errors.Count];
            compiledResults.Errors.CopyTo(this.Errors, 0);
            if (this.Errors.Length == 0)
                this.Assembly = compiledResults.CompiledAssembly;
            this.StartFile = startFile;
        }
        internal CompiledScript()
        {
        }
        public Type GetCompiledType(string TypeName)
        {
            return this.Assembly.GetType(TypeName);
        }
        public object GetStaticVariable(Type Type, string Field)
        {
            return Type.GetField(Field).GetValue(null);
        }
        public object RunStaticFunction(Type CompiledType, string Method)
        {
            return CompiledType.GetMethod(Method).Invoke(null, null);
        }
        public object RunStaticFunction(Type CompiledType, string Method, params object[] Arguments)
        {
            return CompiledType.GetMethod(Method).Invoke(null, Arguments);
        }
        public void SetStaticVariable(Type Type, string Field, object Value)
        {
            Type.GetField(Field).SetValue(null, Value);
        }

        public string StartFile { get; private set; }
        public bool Success { get { return this.Assembly != null; } }
        public CompilerError[] Errors { get; private set; }
        public Assembly Assembly { get; private set; } 
    }
}
