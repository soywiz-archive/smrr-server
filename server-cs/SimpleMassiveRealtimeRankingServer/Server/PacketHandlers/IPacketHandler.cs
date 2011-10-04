using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public interface IPacketHandler
	{
		void HandlePacket(Packet ReceivedPacket, Packet PacketToSend);
	}
}
