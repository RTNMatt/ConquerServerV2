/*

#include ...\inifiles.cs
#include ...\databaselink.cs

*/

using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

public partial class Website
{
	public static string font(string color, string text)
	{
		return "<font color='" + color + "'>" + text + "</font>";
	}
	public static string background(string color)
	{
		return "<body bgcolor=" + color + ">";
	}

	public static string ping(string ip, ushort port, int timeout)
	{
		bool threadFinished = false;
		string threadReturn = font("red", "Offline");
		Thread thread = new Thread(
			delegate()
			{
				try
				{
					Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					socket.Connect(ip, port);
					if (socket.Connected)
					{
						socket.Disconnect(false);
						threadReturn = font("lime", "Online");
					}
					threadFinished = true;
				}
				catch (SocketException) {}
				catch (ThreadAbortException) {}
			}
		);
		thread.Start();
		
		int end = Environment.TickCount + timeout;
		while (end > Environment.TickCount)
		{
			if (threadFinished)
				break;
			Thread.Sleep(1);
		}
		if (!threadFinished)
			thread.Abort();

		return threadReturn;
	}
	public static string ping(string ip, ushort port, int timeout, out bool isonline)
	{
		string report = ping(ip, port, timeout);
		isonline = report.Contains("Online");
		return report;
	}

	public static string WebRequest()
	{
		int Online = new IniFile(GenerateDatabase() + @"\Misc\Settings.ini").ReadInt32("Config", "PlayersOnline", 0);
		bool GameOnline;

		string r = background("black") +
			   font("yellow", "Login Server: ") + ping("127.0.0.1", 9958, 100) + "<br>" + 
			   font("yellow", "Game Server: ") + ping("127.0.0.1", 5816, 100, out GameOnline) + "<br>" +
			   font("yellow", "Registration Server: ") + font("lime", "Online") + "<br>" + // This is the registration server
			   font("white", "Players Online: ");
		if (!GameOnline)
		{
			Online = 0;
		}
		      r += font("#CC00CC", Online.ToString());
		return r;
	}
}