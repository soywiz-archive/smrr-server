module smr.server;

import std.socket;
import std.stream;
import std.stdio;
import std.conv;
import std.process;
import core.memory;

import smr.user;
import smr.userstats;
import smr.utils;

class SmrClient {
	bool alive;
	public Socket socket;
	ubyte[] data;
	SmrServer smrServer;
	
	public this(Socket socket, SmrServer smrServer) {
		this.smrServer = smrServer;
		this.socket = socket;
		this.alive = true;
		this.socket.blocking = false;
	}
	
	void init() {
		//this.socket.send("Hello World!\r\n");
	}
	
	bool isAlive() {
		return this.alive && (socket !is null) && socket.isAlive;
	}
	
	bool receive() {
		scope ubyte[] temp = new ubyte[1024];
		//writefln("%s %s", socket, this);
		
		int totalReceivedLength = socket.receive(temp);
		if (totalReceivedLength <= 0) {
			alive = false;
			return false;
		}
		data ~= temp[0..totalReceivedLength];
		handleData();
		
		/*
		int totalReceivedLength = 0;
		while (true) {
			int receivedLength = socket.receive(temp);
			totalReceivedLength += receivedLength;
			if (receivedLength <= 0) {
				break;
			}
			data ~= temp[0..receivedLength];
		}
		handleData();
		*/
		//writefln("LEN: %d", data.length);
		//return (totalReceivedLength > 0);
		return true;
	}
	
	void close() {
		this.alive = false;
		this.socket.close();
		//this.socket = null;
	}
	
	static enum PacketType : ubyte {
		Ping               = 0,
		SetUsers           = 1,
		LocateUserPosition = 2,
		ListItems          = 3,
	}
	
	void sendPacket(PacketType packetType, ubyte[] data = []) {
		ushort packetSize = cast(ushort)data.length;
		assert (packetSize == data.length);
		
		//scope temp = new ubyte[packetSize.sizeof + 1 + data.length]
		
		scope temp = TA(packetSize) ~ TA(packetType) ~ data;
		socket.send(temp);
		//writefln("sendPacket(%d)", packetType);
	}
	
	void handlePacket_SetUsers(ubyte[] data) {
		struct Request {
			uint userId;
			uint scoreIndex;
			int scoreTimestamp;
			int scoreValue;
		}
		
		struct Response {
		}
		
		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));
		
		int count = data.length / Request.sizeof;

		Response response;

		for (int n = 0; n < count; n++) {
			Request  request = (cast(Request *)data.ptr)[n];
			//smrServer.userStats.setUser(User.create(request.userId).setScore(request.scoreValue, request.scoreTimestamp, request.scoreIndex));
			smrServer.userStats.setUserRanking(request.scoreIndex, request.userId, request.scoreTimestamp, request.scoreValue);
		}
					
		sendPacket(PacketType.SetUsers, TA(response));
	}
	
	void handlePacket_LocateUserPosition(ubyte[] data) {
		struct Request {
			uint userId;
			uint scoreIndex;
		}
		
		struct Response {
			uint position;
		}
		
		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));

		Request  request = *(cast(Request *)data.ptr);
		Response response;
		
		response.position = smrServer.userStats.locateById(request.scoreIndex, request.userId);
		
		sendPacket(PacketType.LocateUserPosition, TA(response));
	}
	
	void handlePacket_ListItems(ubyte[] data) {
		struct Request {
			uint scoreIndex;
			uint offset;
			uint count;
		}
		
		struct ResponseEntry {
			uint position;
			uint userId;
			uint score;
			uint timestamp;
		}

		if (data.length < Request.sizeof) throw(new Exception("Invalid packet size"));

		//writefln("[0]"); stdout.flush();
		Request  request = *(cast(Request *)data.ptr);
		scope response = new MemoryStream();
		int k = request.offset;
		//writefln("[1]"); stdout.flush();
		foreach (User user; smrServer.userStats.getRankingTree(request.scoreIndex).all().skip(request.offset).limit(request.count)) {
			//writefln("[2]"); stdout.flush();
			ResponseEntry responseEntry;
			responseEntry.userId = user.userId;
			responseEntry.position = k;
			responseEntry.score = user.scores[request.scoreIndex].score;
			responseEntry.timestamp = user.scores[request.scoreIndex].timestamp;
			response.write(TA(responseEntry));
			k++;
			//writefln("[3]"); stdout.flush();
		}
		
		sendPacket(PacketType.ListItems, response.data);
	}
	
	void handlePacket(PacketType packetType, ubyte[] data) {
		//writefln("HandlePacket(%d:%s)", packetType, to!string(packetType));
		try {
			switch (packetType) {
				case PacketType.Ping:
					sendPacket(PacketType.Ping, []);
				break;
				case PacketType.ListItems:
					handlePacket_ListItems(data);
					//scope s = new MemoryStream();
					//s.
				break;
				case PacketType.SetUsers:
					handlePacket_SetUsers(data);
				break;
				case PacketType.LocateUserPosition:
					handlePacket_LocateUserPosition(data);
				break;
				default:
					throw(new Exception(std.string.format("Invalid packet 0x%02X", packetType)));
					this.close();
				break;
			}
		} catch (Throwable o) {
			writefln("ERROR: %s", o);
			this.close();
		}
		//writefln("HandlePacket(%d:/%s)", packetType, to!string(packetType));
	}
	
	void handleData() {
		ushort packetSize;
		uint completedCount = 0;
		//writefln("[1]");
		while (data.length >= packetSize.sizeof) {
			packetSize = *cast(typeof(packetSize) *)data.ptr;
			
			int packetTotalLength = packetSize.sizeof + 1 + packetSize;
			
			//writefln("[2] %d, %d", packetSize, data.length);
			if (data.length >= packetTotalLength) {
				//writefln("[3]");
				handlePacket(cast(PacketType)data[packetSize.sizeof], data[packetSize.sizeof + 1..packetTotalLength].dup);
				data = data[packetTotalLength..$].dup;
				completedCount++;
			} else {
				break;
			}
		}

		if (completedCount == 0) {
			//writefln("Not completed (%d)", data.length);
		} else {
			//writefln("Completed(%d)(%d)", completedCount, data.length);
		}
		/*
		int index;
		if ((index = std.string.indexOf(cast(string)data, "\n")) != -1) {
			string line = cast(string)data[0..index].dup;
			data = data[index + 1..$].dup;
			writefln("handleData:'%s'", line);
		}
		*/
	}
}

class SmrServer : TcpSocket {
	SmrClient[Socket] clients;
	UserStats userStats;

	this(string bindIp = "127.0.0.1", ushort bindPort = 9777) {
		userStats = new UserStats();
		blocking = false;
		bind(new InternetAddress(bindIp, bindPort));
		listen(1024);
		writefln("Listening at %s:%d", bindIp, bindPort);
	}
	
	void acceptLoop() {
		Socket socketClient;
		scope SocketSet readSet = new SocketSet();
		//scope SocketSet writeSet = new SocketSet();
		//scope SocketSet errorSet = new SocketSet();
		while (true) {
			readSet.add(this);
			foreach (socket; clients.keys) {
				readSet.add(socket);
			}
			//int count = Socket.select(readSet, writeSet, errorSet);
			int count = Socket.select(readSet, null, null);
			
			if (readSet.isSet(this)) {
				socketClient = accept();
				if (socketClient !is null) {
					SmrClient rankingClient = new SmrClient(socketClient, this);
					clients[socketClient] = rankingClient; 
					rankingClient.init();
					//rankingClient.receive();
				}
			}

			readSockets:;			

			foreach (Socket socket, SmrClient client; clients) {
				if (readSet.isSet(socket)) {
					//writefln("readSet");
					client.receive();
					if (!client.isAlive) {
						//writefln("removed!");
						clients.remove(socket);
						
						// We will perform a small collection when a client disconnected.
						// The idea es not to rely on the GC in the future deallocating
						// all the reserved memory manually. But that will be in a future
						// version.
						GC.minimize();
						goto readSockets;					
					}
					/*
					if (!client.receive()) {
					}
					*/
				}
				/*
				if (writeSet.isSet(socket)) {
					writefln("writeSet");
				}
				if (errorSet.isSet(socket)) {
					writefln("errorSet");
				}
				*/
			}
			
			readSet.reset();
			//writeSet.reset();
			//errorSet.reset();
		}
	}
	
	static void main() {
		system(std.string.format("taskkill /F /IM rbtree_with_stats.exe /FI \"PID ne %d\" > NUL 2> NUL", std.process.getpid));
		
		SmrServer socketServer = new SmrServer();
		socketServer.acceptLoop();
	}
}
