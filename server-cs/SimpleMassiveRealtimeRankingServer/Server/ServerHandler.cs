using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.IO;
using CSharpUtils;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public class ServerHandler
	{
		protected TcpListener TcpListener;
		protected string ListenIp;
		protected int ListenPort;
		protected int NumberOfThreads;
		protected ManualResetEvent ClientConnected;
		public ManualResetEvent IsAcceptingSocketEvent;
		public ServerManager ServerManager;

		public ServerHandler(string ListenIp, int ListenPort, int NumberOfThreads = 1)
		{
			this.ListenIp = ListenIp;
			this.ListenPort = ListenPort;
			this.NumberOfThreads = NumberOfThreads;
			this.ClientConnected = new ManualResetEvent(false);
			this.IsAcceptingSocketEvent = new ManualResetEvent(false);
			this.ServerManager = new ServerManager(NumberOfThreads);
		}

		public void ListenStart()
		{
			try
			{
				this.TcpListener = new TcpListener(IPAddress.Parse(ListenIp), ListenPort);
				this.TcpListener.Start();
				Console.WriteLine("Listening({0}:{1}),Threads({2})...", ListenIp, ListenPort, NumberOfThreads);
			}
			catch (Exception Exception)
			{
				Console.WriteLine("Can't listen to {0}:{1}...", ListenIp, ListenPort);
				throw (Exception);
			}
		}

		public void ListenStop()
		{
			this.TcpListener.Stop();
		}

		public async Task AcceptClientLoopAsync()
		{
			while (true)
			{
				HandleClientAsync(await this.TcpListener.AcceptTcpClientAsync());
			}
		}

		public async Task HandleClientAsync(TcpClient Client)
		{
			Console.WriteLine("HandleClientAsync");
			var ClientStream = Client.GetStream();

			while (Client.Connected)
			{
				// Read Packet Header (Size + Type)
				var PacketHeader = new byte[3];
				await ClientStream.ReadExactAsync(PacketHeader, 0, 3);

				// Parse Packet Header
				var PacketSize = BitConverter.ToUInt16(PacketHeader, 0);
				var PacketType = (PacketType)PacketHeader[2];

				// Read Packet Content
				var PacketBody = new byte[PacketSize];
				await ClientStream.ReadExactAsync(PacketBody, 0, PacketBody.Length);

				// Handle Packet
				await HandlePacket(Client, ClientStream, PacketType, PacketBody);
			}
		}

		public async Task HandlePacket(TcpClient Client, Stream ClientStream, PacketType Type, byte[] RequestContent)
		{
			Console.WriteLine("Handle Packet: {0}, {1}", RequestContent.Length, Type);

			byte[] ResponseHeader = new byte[3];
			byte[] ResponseContent = null;

			switch (Type)
			{
				case PacketType.GetVersion:
				{
					ResponseContent = StructUtils.StructToBytes(ServerManager.Version);
				}
				break;
				case PacketType.GetElementOffset:
				{

				}
				break;
				default: throw(new NotImplementedException("Can't handle packet '" + Type + "'"));
			}

			Array.Copy(BitConverter.GetBytes(ResponseContent.Length), ResponseHeader, 2);
			ResponseHeader[2] = (byte)Type;
			await ClientStream.WriteAsync(ResponseHeader, 0, ResponseHeader.Length);
			await ClientStream.WriteAsync(ResponseContent, 0, ResponseContent.Length);
		}
	}
}
