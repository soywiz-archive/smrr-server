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
        public struct RemoveAllElements_RequestStruct
        {
            public int RankingIndexId;
        }

        private async Task<byte[]> HandlePacket_RemoveAllElements(byte[] RequestContent)
        {
            var Request = StructUtils.BytesToStruct<RemoveAllElements_RequestStruct>(RequestContent);
            
            await EnqueueTask((uint)Request.RankingIndexId, () =>
            {
                var RankingIndex = ServerManager.ServerIndices[Request.RankingIndexId];
                RankingIndex.RemoveAllItems();
            });

            return new byte[0];
        }
    }
}
