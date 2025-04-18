/*

#assembly System.dll
#assembly System.Windows.Forms.dll

*/

using System;
using System.Windows.Forms;

public partial class Website
{
	public static string GenerateDatabase()
	{
	    string DatabasePath = "";
            string[] StartupStrs = Application.StartupPath.Split('\\');
            for (int i = 0; i < StartupStrs.Length - 3; i++)
                DatabasePath += StartupStrs[i] + "\\";
            DatabasePath += "Database";
	    return DatabasePath;
	}
}