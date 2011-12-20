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

        public struct RemoveAllElements_ResponseStruct
        {
            public int Result;
            public uint Count;
        }

        private async Task<byte[]> HandlePacketAsync_RemoveAllElements(byte[] RequestContent)
        {
            var Request = StructUtils.BytesToStruct<RemoveAllElements_RequestStruct>(RequestContent);

            uint Count = 0;
            
            await EnqueueTaskAsync((uint)Request.RankingIndexId, () =>
            {
                var RankingIndex = ServerManager.ServerIndices[Request.RankingIndexId];
                Count = (uint)RankingIndex.Tree.Count;
                RankingIndex.RemoveAllItems();
            });

            return StructUtils.StructToBytes(new RemoveAllElements_ResponseStruct()
            {
                Result = 0,
                Count = Count,
            });
        }
    }
}
