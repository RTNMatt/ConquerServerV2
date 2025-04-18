using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConquerServer_v2.Database;
using ScriptingEngine;

namespace ConquerServer_v2
{
    class Program
    {
        static Website webservice;
        static void Main(string[] args)
        {
            Console.Title = "Conquer Server - Website";

            ScriptExtension extend = new ScriptExtension("Website", "cs");
            extend.AddPreprocess("#new_assembly System.Core.dll");
            extend.AddPreprocess("#assembly System.dll");
            extend.AddNamespace("using System.Net;");
            extend.AddVariable("public static HttpListenerContext Context;");

#if !DEDICATED
            webservice = new Website("http://192.168.1.67:9956/");
#else
            webservice = new Website("http://192.168.1.67:9956/");
#endif
            webservice.Engine.Extension = extend;
            webservice.PublicHtml = ServerDatabase.Path + "\\public_html";
            Console.WriteLine($"ServerDatabase.Path = {ServerDatabase.Path}");
            Website.MimeTypesFile = ServerDatabase.Path + "\\public_html\\mimetypes.ini";
            webservice.Enabled = true;

            while (true)
                Console.ReadLine();
        }
    }
}
