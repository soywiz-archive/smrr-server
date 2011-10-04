using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetVersionHandler : IPacketHandler
	{
		public void HandlePacket(Packet ReceivedPacket, Packet PacketToSend)
		{
			PacketToSend.BinaryWriter.Write((uint)1);
		}
	}
}
