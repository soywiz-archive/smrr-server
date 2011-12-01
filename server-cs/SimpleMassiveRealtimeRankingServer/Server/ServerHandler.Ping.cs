using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMassiveRealtimeRankingServer.Server
{
    public partial class ServerHandler
    {
        private async Task<byte[]> HandlePacket_Ping(byte[] RequestContent)
        {
            return new byte[0];
        }
    }
}
