using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class RemoveAllElementsHandler : BasePacketHandler
	{
		public struct RequestStruct
		{
			public int RankingIndexId;
		}

		RequestStruct Request;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
		}

		public override void Execute(Packet PacketToSend)
		{
			ServerManager.ServerIndices[Request.RankingIndexId].RemoveAllItems();
		}
	}
}
