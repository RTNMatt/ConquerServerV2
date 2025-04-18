using System;
using System.Net;
using System.IO;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Globalization;
using ConquerServer_v2.Database;
using ScriptingEngine;

namespace ConquerServer_v2
{
    // Represents a lightweight embedded web server that can serve static files and execute C# scripts dynamically.
    public class Website
    {
        private static IniFile MimeTypes;   // Stores MIME type mappings loaded from an INI file.
        private ScriptEngine ScriptEngine;  // Responsible for compiling and executing C# script files (.cs).
        private HttpListener Http;  // Native HTTP listener that handles incoming web requests.
        private Thread WorkerThread;    // Dedicated thread for continuously handling HTTP requests.
        private bool m_Enabled; // Indicates whether the server is running.

        // Static constructor to initialize static members
        static Website()
        {
            MimeTypes = new IniFile();
        }

        // Extracts the file extension from a URL (excluding query string)
        public static string FileExtention(string str)
        {
            int pos = str.LastIndexOf('.');
            string tmp = str.Substring(pos, str.Length - pos);
            if (tmp.Contains("?"))
                tmp = tmp.Substring(0, tmp.IndexOf('?'));
            return tmp;
        }
        // Extracts the file path from the full URL and converts slashes to Windows format
        public static string FileAndPath(string str)
        {
            string tmp = str.Remove(0, 7);
            int pos = tmp.IndexOf('/') + 1;
            tmp = tmp.Substring(pos, tmp.Length - pos).Replace('/', '\\');
            if (tmp.Contains("?"))
                tmp = tmp.Substring(0, tmp.IndexOf('?'));
            return tmp;
        }
        // Determines the MIME type to use for a response based on file extension
        public string GetMimeType(string URL)
        {
            string Ext = FileExtention(URL);
            return MimeTypes.ReadString("Types", Ext, "text/html");
        }

        // Static property for setting or getting the path to the MIME types INI file
        public static string MimeTypesFile
        {
            get { return MimeTypes.FileName; }
            set { MimeTypes.FileName = value; }
        }
        // Property for setting or getting the public HTML folder where content is served from
        public string PublicHtml
        {
            get { return ScriptEngine.BuildPath; }
            set
            {
                ScriptEngine.BuildPath = value;
            }
        }
        // Property to enable or disable the server
        public bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                if (m_Enabled != value)
                {
                    m_Enabled = value;
                    if (m_Enabled)
                    {
                        Http.Start();                       // Start listening
                        WorkerThread = new Thread(Worker);  // Begin handling requests
                        WorkerThread.Start();
                    }
                    else
                    {
                        Http.Stop();                        // Stop listening
                        WorkerThread.Abort();               // Kill the worker thread
                    }
                }
            }
        }
        // Returns the active script engine
        public ScriptEngine Engine { get { return ScriptEngine; } }

        // Background worker thread that handles HTTP requests
        private void Worker()
        {
            while (m_Enabled)
            {
                HttpListenerContext Context = Http.GetContext();        // Wait for an incoming request
                Stream Output = Context.Response.OutputStream;
                try
                {
                    string URL = Context.Request.Url.ToString();
                    Context.Response.ContentType = GetMimeType(URL);    // Set response MIME type

                    string FileName = PublicHtml + FileAndPath(URL);    // Construct full path to requested file
                    if (File.Exists(FileName))
                    {
                        if (FileName.EndsWith(".cs"))                   // If it's a C# script
                        {
                            lock (this)                                 // Synchronize script execution
                            {
                                CompiledScript Script = ScriptEngine.CompileScript(FileName);    // Compile the script

                                string output;

                                if (Script.Errors.Length > 0)
                                {
                                    // Send back compilation errors
                                    output = "";
                                    foreach (CompilerError error in Script.Errors)
                                        output += error.ToString() + "\r\n";
                                    Output.Write(Encoding.ASCII.GetBytes(output), 0, output.Length);
                                }
                                else
                                {
                                    // Execute the compiled script
                                    Type website = Script.GetCompiledType("Website");
                                    Script.SetStaticVariable(website, "Context", Context);
                                    /* REINITIALIZE */
                                    // Look for and invoke Initialize methods in the compiled script
                                    foreach (Type type in Script.Assembly.GetTypes())
                                    {
                                        MethodInfo init = type.GetMethod("Initialize");
                                        if (init != null)
                                        {
                                            if (init.GetParameters().Length == 0)
                                                init.Invoke(null, null);
                                            else
                                                init.Invoke(null, new object[] { Context });
                                        }
                                    }
                                    /**/
                                    // Run the static WebRequest method and write its output
                                    output = Script.RunStaticFunction(website, "WebRequest") as string;
                                    Output.Write(Encoding.ASCII.GetBytes(output), 0, output.Length);
                                }
                            }
                        }
                        else
                        {
                            // Serve static files
                            byte[] binary = File.ReadAllBytes(FileName);
                            Output.Write(binary, 0, binary.Length);
                        }
                    }
                    else
                    {
                        // File not found message
                        string reply = "Oops, it would seem this file doesn't exist!";
                        byte[] binary = Encoding.ASCII.GetBytes(reply);
                        Output.Write(binary, 0, binary.Length);
                    }
                }
                catch (Exception e)
                {
                    // Error handling - return exception info in response
                    try
                    {
                        string dump = e.ToString();
                        byte[] binary = Encoding.ASCII.GetBytes(dump);
                        Output.Write(binary, 0, binary.Length);
                    }
                    catch { }
                }
                try
                {
                    Output.Close(); // Ensure stream is closed properly
                }
                catch { }
            }
        }
        // Constructor that sets up the HTTP listener and scripting engine
        public Website(params string[] Bind)
        {
            MimeTypes = new IniFile(null);          // Load MIME types from default (null)
            Http = new HttpListener();
            ScriptEngine = new ScriptEngine("C#");
            foreach (string Prefix in Bind)
            {
                Http.Prefixes.Add(Prefix);          // e.g., http://localhost:8080/
            }
        }
    }
}