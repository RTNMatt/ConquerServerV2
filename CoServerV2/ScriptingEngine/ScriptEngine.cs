using System;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;

namespace ScriptingEngine
{
    public class ScriptEngine : MarshalByRefObject
    {
        private CodeDomProvider Compiler;
        private ScriptExtension Extend;
        private string m_BuildPath;
        private static string newFramework = @"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\";
        private string m_Language;

        private List<StaticVariable> StaticVariables;
        private Dictionary<string, CompiledScript> PreCompiled;

        /// <summary>
        /// The path to where your *.cs, or *.vb files are located
        /// </summary>
        public string BuildPath
        {
            get { return m_BuildPath; }
            set
            {
                if (!value.EndsWith("\\"))
                {
                    value += "\\";
                }
                if (!Directory.Exists(value))
                    throw new ArgumentException("The directory specified does not exist.");
                m_BuildPath = value;
                if (Extend != null)
                {
                    string _out = m_BuildPath + "global." + Extend.FileExt;
                    File.WriteAllText(_out, Extend.ToString(m_Language));
                }
            }
        }
        /// <summary>
        /// Allows you to include a pre-requisite module into every script.
        /// </summary>
        public ScriptExtension Extension
        {
            get { return Extend; }
            set { Extend = value; }
        }
        /// <summary>
        /// Declares where your .NET 3.5 folder path is
        /// </summary>
        public static string NewFrameworkPath
        {
            get { return newFramework; }
            set
            {
                if (Directory.Exists(value))
                {
                    newFramework = value;
                    if (!newFramework.EndsWith("\\"))
                        newFramework += "\\";
                }
                else
                    throw new ArgumentException("The directory specified does not exist.");
            }
        }
        /// <summary>
        /// Create a new instance of the Hybrid Scripting Engine
        /// </summary>
        /// <param name="Language">This parmeter should either be, "C#" or "VB"</param>
        public ScriptEngine(string Language)
        {
            if (Language.Equals("C#", StringComparison.OrdinalIgnoreCase))
                Compiler = new CSharpCodeProvider();
            else if (Language.Equals("VB", StringComparison.OrdinalIgnoreCase))
                Compiler = new VBCodeProvider();
            else throw new ArgumentException("Language");
            m_Language = Language;
            StaticVariables = new List<StaticVariable>();
            PreCompiled = new Dictionary<string, CompiledScript>();
        }

        private void PreProcessFiles(ref List<string> CompiledFiles, ref CompilerParameters cParams)
        {
            string tmp;
            foreach (string FileName in CompiledFiles)
            {
                using (StreamReader Reader = new StreamReader(FileName))
                {
                    while ((tmp = Reader.ReadLine()) != null)
                    {
                        if (tmp.StartsWith("using") || tmp.StartsWith("Imports"))
                            break;
                        string[] data = tmp.Split(' ');
                        switch (data[0].ToLower().TrimStart('/', '\'', ' '))
                        {
                            #region #assembly
                            case "#assembly":
                                {
                                    string sub_data = tmp.Remove(0, tmp.IndexOf(' ') + 1).ToLower();
                                    if (!cParams.ReferencedAssemblies.Contains(sub_data))
                                    {
                                        cParams.ReferencedAssemblies.Add(sub_data);
                                    }
                                    break;
                                }
                            #endregion
                            #region #new_assembly
                            case "#new_assembly":
                                {
                                    string sub_data = tmp.Remove(0, tmp.IndexOf(' ') + 1).ToLower();
                                    if (!cParams.ReferencedAssemblies.Contains(sub_data))
                                    {
                                        cParams.ReferencedAssemblies.Add(newFramework + sub_data);
                                    }
                                    break;
                                }
                            #endregion
                            #region #compiler
                            case "#compiler":
                                {
                                    if (cParams.CompilerOptions != null)
                                    {
                                        if (!cParams.CompilerOptions.Contains(data[1] + " "))
                                            cParams.CompilerOptions += data[1] + " ";
                                    }
                                    else
                                        cParams.CompilerOptions += data[1] + " ";
                                    break;
                                }
                            #endregion
                        }
                    }
                }
            }
        }
        private Assembly CompileFiles(string[] CompiledFiles, CompilerParameters cParams, out CompilerResults res)
        {
            res = Compiler.CompileAssemblyFromFile(cParams, CompiledFiles);
            if (res.Errors.Count == 0)
            {
                Assembly asm = res.CompiledAssembly;
                foreach (StaticVariable var in StaticVariables)
                {
                    Type type = asm.GetType(var.Type);
                    FieldInfo field = type.GetField(var.Field);
                    field.SetValue(null, var.Value);
                }
                return asm;
            }
            return null;
        }
        private void GetIncludeFiles(string StartFile, ref List<string> Files)
        {
            using (StreamReader rdr = new StreamReader(StartFile, Encoding.UTF8))
            {
                string read;
                while ((read = rdr.ReadLine()) != null)
                {
                    if (read.StartsWith("using") || read.StartsWith("Imports"))
                        break;
                    string[] data = read.Split(' ');
                    switch (data[0].ToLower().TrimStart('/', '\'', ' '))
                    {
                        case "#include":
                            {
                                string tmp2 = read.Remove(0, data[0].Length + 1);
                                tmp2 = tmp2.Replace("...\\", BuildPath).ToLower();
                                if (!Files.Contains(tmp2))
                                {
                                    Files.Add(tmp2);
                                    GetIncludeFiles(tmp2, ref Files);
                                }
                                break;
                            }
                        case "#alias":
                            {
                                Files.Remove(StartFile);
                                goto case "#include";
                            }
                        case "#includedir":
                            {
                                string tmp2 = read.Remove(0, data[0].Length + 1);
                                tmp2 = tmp2.Replace("...\\", BuildPath).ToLower();
                                string[] tmp3 = tmp2.Split('*');
                                foreach (string file in Directory.GetFiles(tmp3[0]))
                                {
                                    string ffile = file.ToLower();
                                    if (ffile.EndsWith(tmp3[1], StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!Files.Contains(ffile))
                                        {
                                            Files.Add(ffile);
                                            GetIncludeFiles(ffile, ref Files);
                                        }
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }
        private string GetMD5Hash(List<String> Files)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Files.Count; i++)
            {
                byte[] hash = x.ComputeHash(File.ReadAllBytes(Files[i]));
                sb.EnsureCapacity(sb.Length + hash.Length);
                for (int i2 = 0; i2 < hash.Length; i2++)
                    sb.Append(hash[i2].ToString("X2"));
            }
            return sb.ToString();
        }
        private CompiledScript CompileInEnvironment(string startFile, List<string> CompiledFiles)
        {
            CompilerResults res;
            CompilerParameters cParams = new CompilerParameters();
            cParams.GenerateExecutable = false;
            cParams.GenerateInMemory = true;
            cParams.TreatWarningsAsErrors = false;

            PreProcessFiles(ref CompiledFiles, ref cParams);
            CompileFiles(CompiledFiles.ToArray(), cParams, out res);

            CompiledScript cs = new CompiledScript(startFile, res);
            return cs;
        }

        /// <summary>
        /// Compiles a script located in the bin (build path), and returns the compiled version.
        /// </summary>
        /// <param name="FileName">The file of the script to compile</param>
        public CompiledScript CompileScriptFromBin(string FileName)
        {
            return CompileScript(BuildPath + FileName);
        }

        /// <summary>
        /// Compiles a script, and returns the compiled version.
        /// </summary>
        /// <param name="FileName">The file of the script to compile</param>
        public CompiledScript CompileScript(string FileName)
        {
            lock (PreCompiled)
            {
                List<string> CompiledFiles = new List<string>();
                if (Extend != null)
                    CompiledFiles.Add((BuildPath + "global." + Extend.FileExt).ToLower());
                FileName = FileName.ToLower();

                CompiledFiles.Add(FileName);
                GetIncludeFiles(FileName, ref CompiledFiles);
                string hash = GetMD5Hash(CompiledFiles);

                CompiledScript Compiled = new CompiledScript();
                if (PreCompiled.TryGetValue(hash, out Compiled))
                {
                    Compiled = PreCompiled[hash];
                }
                else
                {
                    Compiled = CompileInEnvironment(FileName, CompiledFiles);
                    if (Compiled.Success)
                    {
                        PreCompiled.Add(hash, Compiled);
                    }
                }
                return Compiled;
            }
        }
        /// <summary>
        /// Registers a static variable, this variable will be set if existing in the script,
        /// as soon as the script is compiled.
        /// </summary>
        /// <param name="TypeName">The type the static-variable is located in.</param>
        /// <param name="FieldName">The static-variables name.</param>
        /// <param name="Value">The value to set inplace.</param>
        public void RegisterGlobalVariable(string TypeName, string FieldName, object Value)
        {
            lock (StaticVariables)
            {
                StaticVariable var = new StaticVariable();
                var.Type = TypeName;
                var.Field = FieldName;
                var.Value = Value;

                RemoveGlobalVariable(TypeName, FieldName);
                StaticVariables.Add(var);
            }
        }
        /// <summary>
        /// Removes a global variable previously set by RegisterGlobalVariable()
        /// Note: This does not set the static-variables that were set by RegisterGlobalVariable()
        /// to null, or dispose of them, you will need to do this yourself.
        /// </summary>
        /// <param name="TypeName">The type the static-variable is located in.</param>
        /// <param name="FieldName">The field name of the static-variable.</param>
        public void RemoveGlobalVariable(string TypeName, string FieldName)
        {
            for (int i = 0; i < StaticVariables.Count; i++)
            {
                StaticVariable tmp_var = StaticVariables[i];
                if (tmp_var.Type == TypeName)
                {
                    if (tmp_var.Field == FieldName)
                    {
                        StaticVariables.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}