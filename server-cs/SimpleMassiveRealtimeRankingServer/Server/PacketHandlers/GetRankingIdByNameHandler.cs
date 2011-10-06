﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetRankingIdByNameHandler : BasePacketHandler
	{
		string RankingName;

		public override void FastParseRequest(Packet ReceivedPacket)
		{
			this.RankingName = ReceivedPacket.Stream.ReadStringz();
		}

		public override void Execute(Packet PacketToSend)
		{
			int RankingIndex = ServerManager.ServerIndices[RankingName].IndexId;
			PacketToSend.Stream.WriteStruct((int)RankingIndex);
		}
	}
}