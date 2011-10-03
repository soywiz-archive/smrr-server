using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

namespace CSharpUtilsSandBox.Server
{
	public class ServerHandler
	{
		protected TcpListener TcpListener;
		protected string ListenIp;
		protected int ListenPort;
		protected ManualResetEvent ClientConnected;
		public ManualResetEvent IsAcceptingSocketEvent;

		public ServerHandler(string ListenIp, int ListenPort)
		{
			this.ListenIp = ListenIp;
			this.ListenPort = ListenPort;
			this.ClientConnected = new ManualResetEvent(false);
			this.IsAcceptingSocketEvent = new ManualResetEvent(false);
		}

		public void ListenStart()
		{
			try
			{
				this.TcpListener = new TcpListener(IPAddress.Parse(ListenIp), ListenPort);
				this.TcpListener.Start();
				Console.WriteLine("Listening {0}:{1}...", ListenIp, ListenPort);
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

		public void AcceptClientLoop()
		{
			while (true)
			{
				AcceptClient();
			}
		}

		public void AcceptClient()
		{
			if (this.TcpListener == null)
			{
				//this.ListenStart();
				throw(new MethodAccessException("Can't call AcceptClient without calling ListenStart first"));
			}

			Console.WriteLine("Waiting for a connection...");
			ClientConnected.Reset();
			IsAcceptingSocketEvent.Reset();
			this.TcpListener.BeginAcceptTcpClient((AcceptState) =>
			{
				var AcceptedTcpClient = (AcceptState.AsyncState as TcpListener).EndAcceptTcpClient(AcceptState);
				var ClientHandler = new ClientHandler(AcceptedTcpClient.Client);
				ClientHandler.StartReceivingData();

				ClientConnected.Set();
			}, this.TcpListener);
			IsAcceptingSocketEvent.Set();
			ClientConnected.WaitOne();
		}
	}
}
