using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMassiveRealtimeRankingServer.Server
{
    public partial class ServerHandler
    {
#if NET_4_5
        async private Task<byte[]> HandlePacketAsync_Ping(byte[] RequestContent)
        {
            return new byte[0];
        }
#else
		private byte[] HandlePacket_Ping(byte[] RequestContent)
		{
			return new byte[0];
		}
#endif
    }
}
