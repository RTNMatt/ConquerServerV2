using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ScriptingEngine
{
    public class ScriptEnvironment : MarshalByRefObject
    {
        public AppDomain Domain { get; private set; }
        public ScriptEnvironment(string friendlyName)
        {
            Domain = AppDomain.CreateDomain(friendlyName);
        }
        public ScriptEnvironment(AppDomain existingDomain)
        {
            Domain = existingDomain;
        }
        public void Execute(CrossAppDomainDelegate Callback)
        {
            Domain.DoCallBack(Callback);
        }
        public void Delete()
        {
            AppDomain.Unload(Domain);
        }

        /// <summary>
        /// Gets a type from the compiled script.
        /// </summary>
        /// <param name="TypeName">The type's name.</param>
        /// <returns></returns>
        public static Type GetCompiledType(ScriptEnvironment Environment, CompiledScript Script, string TypeName)
        {
            Type t = null;
            string s = Script.AssemblyName;
            Environment.Domain.DoCallBack(
                () =>
                {
                    Assembly.Load(s).GetType(TypeName);
                }
            );
            return t;
        }
        /*
        /// <summary>
        /// Sets a static variable inside of the script.
        /// </summary>
        /// <param name="Type">The type the static-variable is located in.</param>
        /// <param name="Field">The name of the static-variable.</param>
        /// <param name="Value">The value to set.</param>
        public void SetStaticVariable(Type Type, string Field, object Value)
        {
            FieldInfo field = Type.GetField(Field);
            field.SetValue(null, Value);
        }
        /// <summary>
        /// Gets a static variable inside of the script.
        /// </summary>
        /// <param name="Type">The type the static-variable is located in.</param>
        /// <param name="Field">The name of the static-variable.</param>
        public object GetStaticVariable(Type Type, string Field)
        {
            FieldInfo field = Type.GetField(Field);
            return field.GetValue(null);
        }
        /// <summary>
        /// Executes a static function inside of the script.
        /// </summary>
        /// <param name="CompiledType">The type that the static-function is located in.</param>
        /// <param name="Method">The name of the static function</param>
        public object RunStaticFunction(Type CompiledType, string Method)
        {
            return CompiledType.GetMethod(Method).Invoke(null, null);
        }
        /// <summary>
        /// Executes a static function inside of the script.
        /// </summary>
        /// <param name="CompiledType">The type that the static-function is located in.</param>
        /// <param name="Method">The name of the static function</param>
        /// <param name="Arguments">The parameters passed, in order.</param>
        public object RunStaticFunction(Type CompiledType, string Method, params object[] Arguments)
        {
            return CompiledType.GetMethod(Method).Invoke(null, Arguments);
        }
    }
         */
    }
}
