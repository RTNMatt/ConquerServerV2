/*

#assembly System.Windows.Forms.dll
#include ...\urlhelp.cs
#include ...\inifiles.cs
#include ...\databaselink.cs

*/

using System;
using System.IO;
using System.Windows.Forms;

public partial class Website
{
	public static bool SafeString(string Query)
	{
		string unsafestr = "<>[]%$#@!^&*()-=~`/\'\"";
		foreach (char c in Query)
			if (unsafestr.Contains(c.ToString()))
				return false;
		return true;
	}
	public static string Redirect(string url)
	{
		return "<script language='JavaScript'> window.location='" + url + "'; </script>";
	}
	public static string WebRequest()
	{
		string DatabasePath = GenerateDatabase() + "\\Accounts\\";
		string Username = URLHandler.GetField("username");
		string Password = URLHandler.GetField("password");
		string Query = URLHandler.GetField("query");
		if (Query == "")
		{
			if (Username != "" && Password != "")
			{
				if (Username.Length < 16 && Password.Length < 16)
				{
					IniFile ini = new IniFile(DatabasePath + Username + ".ini");
					if (File.Exists(ini.FileName))
					{
						return Redirect("/Register.cs?Query=This account already exists.");
					}
					else
					{
						ini.WriteString("Account", "Username", Username);
						ini.WriteString("Account", "Password", Password);
						ini.WriteString("Character", "Name", "INVALIDNAME");
						DirectoryInfo info = new DirectoryInfo(DatabasePath);
						ini.WriteString("Character", "UID", 1000000 + info.GetFiles().Length);
						return Redirect("/Register.cs?Query=Your account has been created.");
					}
				}	
				else
				{
					return Redirect("/Register.cs?Query=Username or Password is too long, 15 characters at maximum.");
				}
			}
			return Redirect("/Register.cs?Query=A username or password cannot be blank.");
		}
		if (SafeString(Query))
			return Query;
		return "error";
	}
}