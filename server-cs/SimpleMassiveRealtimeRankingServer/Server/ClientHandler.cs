using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using CSharpUtils.Extensions;
using System.Runtime.InteropServices;
using System.IO;
using SimpleMassiveRealtimeRankingServer.Server.PacketHandlers;
using System.Reflection;
using CSharpUtils;
using System.Threading;
using CSharpUtils.Threading;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public class ClientHandler : BaseClientHandler
	{
		ServerManager ServerManager;

		public ClientHandler(ServerManager ServerManager, TcpClient TcpClientSocket)
			: base(TcpClientSocket)
		{
			this.ServerManager = ServerManager;
		}

		override protected void HandlePacket(Packet ReceivedPacket)
		{
			var PacketToSend = new Packet(ReceivedPacket.Type);
			BasePacketHandler PacketHandler;

			switch (ReceivedPacket.Type)
			{
				case PacketType.GetElementOffset:
					PacketHandler = new GetElementOffsetHandler();
					break;
				case PacketType.GetRankingIdByName:
					PacketHandler = new GetRankingIdByNameHandler();
					break;
				case PacketType.GetRankingInfo:
					PacketHandler = new GetRankingInfoHandler();
					break;
				case PacketType.GetVersion:
					PacketHandler = new GetVersionHandler();
					break;
				case PacketType.ListElements:
					PacketHandler = new ListElementsHandler();
					break;
				case PacketType.Ping:
					PacketHandler = new PingHandler();
					break;
				case PacketType.RemoveAllElements:
					PacketHandler = new RemoveAllElementsHandler();
					break;
				case PacketType.RemoveElements:
					PacketHandler = new RemoveElementsHandler();
					break;
				case PacketType.SetElements:
					PacketHandler = new SetElementsHandler();
					break;
				default:
					throw (new NotImplementedException("Can't handle packet '" + ReceivedPacket + "'"));
			}
			//Console.WriteLine(TypeUtils.GetTypesExtending(typeof(IPacketHandler)).ToStringArray());

			PacketHandler.SetServerManager(ServerManager);
			PacketHandler.FastParseRequest(ReceivedPacket);
			int ThreadAffinity = PacketHandler.GetThreadAffinityAfterParseRequest();

			ScheduleTask(ThreadAffinity, () =>
			{
				try
				{
					PacketHandler.Execute(PacketToSend);
					PacketToSend.WritePacketTo(this.ClientNetworkStream);
				}
				catch (Exception Exception)
				{
					Console.WriteLine(Exception);
					TcpClientSocket.Close();
				}
			});
		}

		void ScheduleTask(int ThreadAffinity, Action Task)
		{
			ServerManager.CustomThreadPool.AddTask(ThreadAffinity, Task);
			//Task();
		}
	}
}
