using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;

namespace SimpleMassiveRealtimeRankingServer.Server
{
    public partial class ServerHandler
    {
        public struct GetRankingNameById_RequestStruct
        {
            public int RankingIndex;
        }

#if NET_4_5
        async private Task<byte[]> HandlePacketAsync_GetRankingNameById(byte[] RequestContent)
        {
            var Request = StructUtils.BytesToStruct<GetRankingNameById_RequestStruct>(RequestContent);
            return Encoding.UTF8.GetBytes(ServerManager.ServerIndices[Request.RankingIndex].IndexName);
        }
#else
		private byte[] HandlePacket_GetRankingNameById(byte[] RequestContent)
		{
			var Request = StructUtils.BytesToStruct<GetRankingNameById_RequestStruct>(RequestContent);
			return Encoding.UTF8.GetBytes(ServerManager.ServerIndices[Request.RankingIndex].IndexName);
		}
#endif
    }
}
