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

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public class ClientHandler : BaseClientHandler
	{
		public ClientHandler(TcpClient TcpClientSocket)
			: base(TcpClientSocket)
		{
		}

		override protected void HandlePacket(Packet ReceivedPacket)
		{
			var PacketToSend = new Packet(ReceivedPacket.Type);
			IPacketHandler PacketHandler;

			switch (ReceivedPacket.Type)
			{
				case Packet.PacketType.GetVersion:
					PacketHandler = new GetVersionHandler();
					break;
				case Packet.PacketType.GetRankingInfo:
					PacketHandler = new GetRankingInfoHandler();
					break;
				default:
					throw (new NotImplementedException("Can't handle packet '" + ReceivedPacket + "'"));
			}
			//Console.WriteLine(TypeUtils.GetTypesExtending(typeof(IPacketHandler)).ToStringArray());
			PacketHandler.HandlePacket(ReceivedPacket, PacketToSend);
			PacketToSend.WritePacketTo(this.ClientNetworkStream);
		}
	}
}
