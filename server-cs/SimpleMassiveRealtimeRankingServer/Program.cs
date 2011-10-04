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

namespace SimpleMassiveRealtimeRankingServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var ServerHandler = new ServerHandler("127.0.0.1", 9777);
			ServerHandler.ListenStart();
			ServerHandler.AcceptClientLoop();
		}
	}
}
