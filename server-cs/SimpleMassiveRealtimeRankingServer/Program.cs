using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Containers.RedBlackTree;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using CSharpUtils;
using CSharpUtils.Extensions;
using System.IO;
using CSharpUtils.Streams;
using SimpleMassiveRealtimeRankingServer.Server;
using CSharpUtils.Getopt;
using System.Reflection;
using System.Diagnostics;

namespace SimpleMassiveRealtimeRankingServer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
                string Revision = "<Unknown>";
                string Datetime = "<Unknown>";
                try
                {
                    Datetime = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleMassiveRealtimeRankingServer.DATETIME").ReadAllContentsAsString();
                }
                catch
                {
                }

                try
                {
                    Revision = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimpleMassiveRealtimeRankingServer.ORIG_HEAD").ReadAllContentsAsString();
                }
                catch
                {
                }

                //FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().GetName().ToString());
                //Console.WriteLine(myFileVersionInfo);
                //Console.WriteLine(Datetime);

                //Console.WriteLine(Version.Build);

                
                string BindIp = "0.0.0.0";
				int BindPort = 9777;
				int NumberOfThreads = Environment.ProcessorCount;

				var Getopt = new Getopt(args);
				Getopt.AddRule("-v", () =>
				{
					Console.WriteLine(new ServerManager().Version);
					Environment.Exit(0);
				});
				Getopt.AddRule("-i", (string Value) => { BindIp = Value; });
				Getopt.AddRule("-p", (int Value) => { BindPort = Value; });
				Getopt.AddRule("-t", (int Value) => { NumberOfThreads = Value; });
				Getopt.AddRule(new string[] { "-h", "-?", "--help" }, () =>
				{
					Console.WriteLine("Simple Massive Realtime Ranking Server - {0} - Carlos Ballesteros Velasco - 2011-2011", new ServerManager().Version);
#if NET_4_5
                    Console.WriteLine("Compiled with .NET 4.5 Async support.");
#else
                    Console.WriteLine("Compiled with old .NET 4.0 (no async support).");
#endif
                    Console.WriteLine("Compiled git Version: {0} | Build time: {1}", Revision, Datetime);
                    Console.WriteLine("Lastest version: https://github.com/soywiz/smrr-server");
					Console.WriteLine("");
					Console.WriteLine("Parameters:");
					Console.WriteLine("    -i    Sets the binding ip. Example: -i=192.168.1.1");
					Console.WriteLine("    -p    Sets the binding port. Example: -p=7777");
					Console.WriteLine("    -t    Sets the number of partition threads. Example: -t=8");
					Console.WriteLine("");
					Console.WriteLine("    -v    Shows the version of the server");
					Console.WriteLine("    -h    Shows this help");
					Console.WriteLine("");
					Console.WriteLine("Examples:");
					Console.WriteLine("    SimpleMassiveRealtimeRankingServer -i=0.0.0.0 -p=7777");
					Environment.Exit(-1);
				});
				Getopt.Process();

				var ServerHandler = new ServerHandler(BindIp, BindPort, NumberOfThreads);
#if NET_4_5
				ServerHandler.AcceptClientLoopAsync().Wait();
#else
                ServerHandler.AcceptClientLoop();
#endif
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				Environment.Exit(-1);
			}
		}
	}
}
