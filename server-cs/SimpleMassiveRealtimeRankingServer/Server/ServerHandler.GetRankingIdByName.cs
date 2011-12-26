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
#if NET_4_5
        private async Task<byte[]> HandlePacketAsync_GetRankingIdByName(byte[] RequestContent)
        {
            var RankingName = Encoding.UTF8.GetString(RequestContent, 0, RequestContent.Length - 1);
		    int RankingIndex = ServerManager.ServerIndices[RankingName].IndexId;
            return StructUtils.StructToBytes((int)RankingIndex);
        }
#else
		private byte[] HandlePacket_GetRankingIdByName(byte[] RequestContent)
		{
			var RankingName = Encoding.UTF8.GetString(RequestContent, 0, RequestContent.Length - 1);
			int RankingIndex = ServerManager.ServerIndices[RankingName].IndexId;
			return StructUtils.StructToBytes((int)RankingIndex);
		}
#endif
    }
}
