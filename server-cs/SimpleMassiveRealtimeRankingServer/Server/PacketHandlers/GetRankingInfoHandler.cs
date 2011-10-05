using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	public class GetRankingInfoHandler : IPacketHandler
	{
		public struct RequestStruct
		{
			public int RankingIndex;
		}

		public struct ResponseStruct
		{
			public int Result;
			public int Length;
			public ServerIndices.SortingDirection Direction;
			public int TopScore;
			public int BottomScore;
			public int MaxElements;
			//public uint TreeHeight;
		}

		public void HandlePacket(ServerManager ServerManager, Packet ReceivedPacket, Packet PacketToSend)
		{
			//Console.WriteLine(ReceivedPacket);
			var Request = ReceivedPacket.Stream.ReadStruct<RequestStruct>();
			var Index = ServerManager.ServerIndices[Request.RankingIndex];

			PacketToSend.Stream.WriteStruct(new ResponseStruct()
			{
				Result = 0,
				Length = Index.Tree.Count,
				Direction = Index.SortingDirection,
				TopScore = Index.Tree.FrontElement.ScoreValue,
				BottomScore = Index.Tree.BackElement.ScoreValue,
				MaxElements = -1,
				//TreeHeight = Index.Tree.height
			});
		}
	}

}
