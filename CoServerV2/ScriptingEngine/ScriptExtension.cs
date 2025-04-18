using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ScriptingEngine
{
    public class ScriptExtension
    {
        private StringWriter Namespaces;
        private StringWriter MainClassData;
        private StringWriter Footer;
        private StringWriter Preprocessor;
        private string m_StaticClass;
        internal string FileExt;

        public void AddNamespace(string declaration)
        {
            Namespaces.WriteLine(declaration);
        }
        public void AddFunction(string declaration)
        {
            MainClassData.WriteLine(declaration);
        }
        public void AddVariable(string declaration)
        {
            MainClassData.WriteLine(declaration);
        }
        public void AddPreprocess(string declaration)
        {
            Preprocessor.WriteLine(declaration);
        }
        public void AddExternalClass(string declaration)
        {
            Footer.WriteLine(declaration);
        }

        public ScriptExtension(string StaticClass, string Ext)
        {
            Namespaces = new StringWriter();
            MainClassData = new StringWriter();
            Preprocessor = new StringWriter();
            Footer = new StringWriter();
            m_StaticClass = StaticClass;
            FileExt = Ext;
        }
        public string ToString(string Language)
        {
            string file = null;
            if (Language.Equals("C#", StringComparison.OrdinalIgnoreCase))
            {
                file =
                "/*\r\n" +
                " ( HYBRID SCRIPT ENGINE 2009 GENERATED HEADER ) \r\n" +
                Preprocessor.ToString() +
                "*/\r\n" +
                "using System;\r\n" +
                Namespaces.ToString() +
                "public partial class " + m_StaticClass + "\r\n" +
                "{\r\n" +
                    MainClassData.ToString() +
                "}\r\n" +
                Footer.ToString();
            }
            else if (Language.Equals("VB", StringComparison.OrdinalIgnoreCase))
            {
                file =
                "'\r\n" +
                "' ( HYBRID SCRIPT ENGINE 2009 GENERATED HEADER ) \r\n" +
                "'\r\n" +
                Preprocessor.ToString() +
                "Imports System\r\n" +
                Namespaces.ToString() +
                "Partial Public Class " + m_StaticClass + "\r\n" +
                    MainClassData.ToString() +
                "End Class\r\n" +
                Footer.ToString();
            }
            else throw new ArgumentException("Language");
            return file;
        }
        ~ScriptExtension()
        {
            Namespaces.Close();
            MainClassData.Close();
            Footer.Close();
            Preprocessor.Close();
        }
    }
}
