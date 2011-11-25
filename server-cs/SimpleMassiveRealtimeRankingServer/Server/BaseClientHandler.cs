using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	abstract public class BaseClientHandler
	{
		public TcpClient TcpClientSocket;
		public NetworkStream ClientNetworkStream;
		public ProduceConsumeBuffer<byte> DataBuffer;
		protected List<ArraySegment<byte>> InternalSocketBuffers;

		public BaseClientHandler(TcpClient TcpClientSocket)
		{
			this.TcpClientSocket = TcpClientSocket;
			this.ClientNetworkStream = new NetworkStream(TcpClientSocket.Client);
			this.InternalSocketBuffers = new List<ArraySegment<byte>>();
			this.InternalSocketBuffers.Add(new ArraySegment<byte>(new byte[1024]));
			this.DataBuffer = new ProduceConsumeBuffer<byte>();
		}

		public void StartReceivingData()
		{
			if (this.TcpClientSocket.Connected && this.TcpClientSocket.Client.Connected)
			{
				this.TcpClientSocket.Client.BeginReceive(this.InternalSocketBuffers, SocketFlags.None, this.HandleDataReceived, null);
			}
		}

		protected void HandleDataReceived(IAsyncResult AsyncResult)
		{
			try
			{
				int ReadedBytes = this.TcpClientSocket.Client.EndReceive(AsyncResult);
				{
					DataBuffer.Produce(this.InternalSocketBuffers[0].Array, 0, ReadedBytes);
					TryHandlePacket(this.DataBuffer);
				}
				if (ReadedBytes > 0)
				{
					StartReceivingData();
				}
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
		}

		abstract protected void HandlePacket(Packet ReceivedPacket);

		protected void TryHandlePacket(ProduceConsumeBuffer<byte> DataBuffer)
		{
			if (DataBuffer.ConsumeRemaining >= 3)
			{
				var PeekData = DataBuffer.ConsumePeek(3);
				var PacketSize = PeekData[0] | (PeekData[1] << 8);
				var PacketType = (PacketType)PeekData[2];
				var RealPacketSize = 2 + 1 + PacketSize;

				//Console.WriteLine("RealPacketSize={0}", RealPacketSize);

				if (DataBuffer.ConsumeRemaining >= RealPacketSize)
				{
					DataBuffer.Consume(3);
					var PacketData = DataBuffer.Consume(PacketSize);
					//Console.WriteLine(PacketData.ToHexString());
					HandlePacket(new Packet(PacketType, PacketData));
				}
			}
		}
	}
}
