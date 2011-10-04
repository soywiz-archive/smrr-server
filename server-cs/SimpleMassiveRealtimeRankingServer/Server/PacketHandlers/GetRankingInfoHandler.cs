using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetRankingInfoHandler : IPacketHandler
	{
		struct RequestStruct
		{
			//[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 64)]
			//internal string RankingName;
			uint RankingIndex;
		}

		struct ResponseStruct
		{
			internal uint Result;
			//internal uint Index;
			internal uint Length;
			internal ServerIndices.SortingDirection Direction;
			internal uint TopScore;
			internal uint BottomScore;
			internal int MaxElements;
			internal uint TreeHeight;
		}

		public void HandlePacket(Packet ReceivedPacket, Packet PacketToSend)
		{
			//Console.WriteLine(ReceivedPacket);
			var Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
			var Response = new ResponseStruct();
			//Console.WriteLine("'" + Request.RankingName + "'");
			Response.Result = 0;
			PacketToSend.Stream.WriteStruct(Response);
		}
	}

}
