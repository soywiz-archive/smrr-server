
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public enum PacketType : byte
	{
		////////////////////////////////
        /// Information ////////////////
		////////////////////////////////
		Ping = 0x00,
		GetVersion = 0x01,
        GetServerInfo = 0x02,
		////////////////////////////////
		/// Rankings ///////////////////
		////////////////////////////////
		GetRankingIdByName = 0x10,
		GetRankingInfo = 0x11,
        GetRankingNameById = 0x12,
		////////////////////////////////
		/// Elements ///////////////////
		////////////////////////////////
		SetElements = 0x20,
		GetElement = 0x21,
		ListElements = 0x22,
		RemoveElements = 0x23,
		RemoveAllElements = 0x24,
		////////////////////////////////
	}
}
