using System;
using System.Collections.Generic;
using System.Globalization;

public partial class URLHandler
{
       public static Dictionary<string, string> URLData;
        private static string GetFieldId(string Field)
        {
            return Field.Substring(0, Field.IndexOf('='));
        }
        private static string GetFieldValue(string Field)
        {
            int pos = Field.IndexOf('=') + 1;
            return Field.Substring(pos, Field.Length - pos);
        }
        private static string[] GetURLData(string str)
        {
            int pos = str.IndexOf('?');
            if (pos > -1)
            {
                pos += 1;
                string tmp = str.Substring(pos, str.Length - pos);
                string[] res = tmp.Split('&');
                for(int i = 0; i < res.Length; i++)
                {
                    int index = 0;
                    char fill;
                    while ((index = res[i].IndexOf('%')) > -1)
                    {
                        tmp = res[i].Substring(index + 1, 2);
                        fill = (char)byte.Parse(tmp, NumberStyles.HexNumber);
                        res[i] = res[i].Remove(index, 3);
                        res[i] = res[i].Insert(index, fill.ToString());
                    }
		    res[i] = res[i].ToLower();
                }
                return res;
            }
            return new string[0];
        }
	public static void Initialize()
	{
		string URL = Website.Context.Request.Url.ToString();
		string[] Data = GetURLData(URL);
		URLData = new Dictionary<string, string>(Data.Length);
		foreach (string tmp in Data)
		{
			URLData.Add(GetFieldId(tmp), GetFieldValue(tmp));	
		}
	}
	public static string GetField(string FieldId)
	{
		string retn;
		if (URLData.TryGetValue(FieldId, out retn))
			return retn;
		return "";
	}
}