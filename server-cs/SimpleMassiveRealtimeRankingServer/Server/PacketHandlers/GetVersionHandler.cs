using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetVersionHandler : BasePacketHandler
	{
		public override void FastParseRequest(Packet ReceivedPacket)
		{
		}

		public override void Execute(Packet PacketToSend)
		{
			PacketToSend.Stream.WriteStruct(ServerManager.Version);
		}
	}
}
