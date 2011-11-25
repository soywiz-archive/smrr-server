
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public enum PacketType : byte
	{
		////////////////////////////////
		/// Misc ///////////////////////
		////////////////////////////////
		Ping = 0x00,
		GetVersion = 0x01,
		////////////////////////////////
		/// Rankings ///////////////////
		////////////////////////////////
		GetRankingIdByName = 0x10,
		GetRankingInfo = 0x11,
		////////////////////////////////
		/// Elements ///////////////////
		////////////////////////////////
		SetElements = 0x20,
		GetElementOffset = 0x21,
		ListElements = 0x22,
		RemoveElements = 0x23,
		RemoveAllElements = 0x24,
		////////////////////////////////
	}
}
