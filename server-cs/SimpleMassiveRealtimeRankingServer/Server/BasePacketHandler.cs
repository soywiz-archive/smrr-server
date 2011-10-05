using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMassiveRealtimeRankingServer.Server.PacketHandlers
{
	abstract public class BasePacketHandler
	{
		protected ServerManager ServerManager;

		virtual public int GetThreadAffinityAfterParseRequest()
		{
			return 0;
		}

		public void HandlePacket(ServerManager ServerManager, Packet ReceivedPacket, Packet PacketToSend)
		{
			this.SetServerManager(ServerManager);
			this.FastParseRequest(ReceivedPacket);
			//int ThreadAffinity = this.GetThreadAffinityAfterParseRequest();
			this.Execute(PacketToSend);
		}

		public void SetServerManager(ServerManager ServerManager)
		{
			this.ServerManager = ServerManager;
		}

		abstract public void FastParseRequest(Packet ReceivedPacket);
		abstract public void Execute(Packet PacketToSend);
	}
}
