using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public class Packet
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

		public PacketType Type;
		public MemoryStream Stream;
		public BinaryWriter BinaryWriter;
		public BinaryReader BinaryReader;

		public Packet(PacketType Type, byte[] Data)
		{
			_SetUp(Type, Data);
		}

		public Packet(PacketType Type)
		{
			_SetUp(Type, null);
		}

		protected void _SetUp(PacketType Type, byte[] Data)
		{
			this.Type = Type;
			if (Data == null)
			{
				this.Stream = new MemoryStream();
			}
			else
			{
				this.Stream = new MemoryStream(Data);
			}
			this.BinaryWriter = new BinaryWriter(this.Stream);
			this.BinaryReader = new BinaryReader(this.Stream);
		}

		static public Packet FromStream(Stream Stream)
		{
			var BinaryReader = new BinaryReader(Stream);
			var PacketLength = BinaryReader.ReadUInt16();
			var PacketType = BinaryReader.ReadByte();
			var PacketData = BinaryReader.ReadBytes(PacketLength);
			return new Packet((PacketType)PacketType, PacketData);
		}

		public void WritePacketTo(Stream OutputStream)
		{
			var PacketBytes = this.Stream.ReadAll(true);
			var BinaryWriter = new BinaryWriter(OutputStream);
			BinaryWriter.Write((ushort)(PacketBytes.Length));
			BinaryWriter.Write((byte)Type);
			BinaryWriter.Write(PacketBytes);
		}

		public override string ToString()
		{
			return String.Format("Packet(Type={0}, Data={1})", Type, this.Stream.ToArray().ToHexString());
		}
	}
}
