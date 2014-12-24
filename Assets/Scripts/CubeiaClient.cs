using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using Styx;
using com.cubeia.firebase.io.protocol;
using StyxTest;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class CubeiaClient
{
	// const stuff
	public readonly string HOST = "203.162.166.19";
	public readonly int PORT = 4123;
	public readonly int operatorID = 110;
	public readonly string accesstoken = "123456";
	public readonly int levelId = 0;

	// logic variable stuff
	public int gameId = GameType.TEST;
	public int tableId;
	public RoomGame currentRoom;
	public RoomGame[] roomList;
	public List<TableGame> tableList;

	// network stuff
	private Socket socket;
	private ManualResetEvent connected;
	private StyxSerializer serializer;
	private volatile Queue<ProtocolObject> protocalObjectQueue;

	public GameController gameController;

	public CubeiaClient()
	{
		Debug.LogWarning("CubeiaClient's construction");
		connected = new ManualResetEvent(false);
		protocalObjectQueue = new Queue<ProtocolObject>();
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		serializer = new StyxSerializer(new ProtocolObjectFactory());

		// initialize network stuff
		tableList = new List<TableGame>();
	}

	public void processEvents()
	{
		if (protocalObjectQueue.Any()) {
			ProtocolObject protocolObject = protocalObjectQueue.Dequeue();
			handlePacket(protocolObject);
		}
	}

	public void TestLogin()
	{
		connected.Reset();
		socket.BeginConnect(HOST, PORT, new System.AsyncCallback(onConnect), socket);
		connected.WaitOne();
		StyxPacketInfo packetInfo = new StyxPacketInfo();
		packetInfo.socket = socket;
		socket.BeginReceive(packetInfo.headerBuffer, 0, StyxPacketInfo.HEADER_SIZE, 0, new System.AsyncCallback(Read_Callback), packetInfo);		
		
		LoginRequestPacket loginRequestPacket = new LoginRequestPacket();
		// TODO: doan nay cua mini
		var jsonUser = new JSONClass();
		jsonUser ["userid"].AsInt = 2;
		jsonUser ["username"] = "simulator";
		jsonUser ["gameid"].AsInt = gameId;

		
		loginRequestPacket.user = jsonUser.ToString();
		loginRequestPacket.password = "63BC7C70-6109-4B3A-95C6-E4C57F486827";//login.pwd;
		loginRequestPacket.operatorid = operatorID;
		sendPacket(loginRequestPacket);
	}

	private void sendPacket(ProtocolObject protocolObject)
	{
		Debug.Log("---------> send: " + protocolObject.ToString());
		byte[] buffer = serializer.pack(protocolObject);
		socket.Send(buffer, 0, buffer.Length, SocketFlags.None);

		if (protocolObject is ServiceTransportPacket) {
			ServiceTransportPacket sp = (ServiceTransportPacket)protocolObject;
			string serviceData = System.Text.Encoding.UTF8.GetString(sp.servicedata);
			Debug.Log("send: " + serviceData);
		}
		if (protocolObject is GameTransportPacket) {
			GameTransportPacket gp = (GameTransportPacket)protocolObject;
			string gameData = System.Text.Encoding.UTF8.GetString(gp.gamedata);
			Debug.Log("send: " + gameData);
		}
	}
	
	private void onConnect(System.IAsyncResult ar)
	{
		connected.Set();
		Socket s = (Socket)ar.AsyncState;
		s.EndConnect(ar);
	}

	private bool sendService(string str)
	{
		ServiceTransportPacket servicePacket = new ServiceTransportPacket();
		servicePacket.idtype = 1;
		servicePacket.pid = 11111;
		servicePacket.seq = 1;
		servicePacket.service = "com.athena.services.api.ServiceContract";
		servicePacket.servicedata = getBytesUTF8(str);
		sendPacket(servicePacket);
		return true;
	}

	public void sendDataGame(JSONClass ojbect)
	{
		GameTransportPacket gameTransportPacket = new GameTransportPacket();
		gameTransportPacket.pid = GameApplication.user.id;
		gameTransportPacket.tableid = tableId;
		byte[] gdata = getBytes(ojbect.ToString());
		gameTransportPacket.gamedata = gdata;
		sendPacket(gameTransportPacket);
	}

	private void Read_Callback(System.IAsyncResult ar)
	{
		StyxPacketInfo packetInfo = (StyxPacketInfo)ar.AsyncState;
		Socket s = packetInfo.socket;
		
		int read = s.EndReceive(ar);
		
		if (read > 0) {
			if (packetInfo.state == StyxPacketInfo.STATE.header) {
				packetInfo.readHeader();
				packetInfo.state = StyxPacketInfo.STATE.packet;
				s.BeginReceive(packetInfo.packetBuffer, StyxPacketInfo.HEADER_SIZE, packetInfo.packetLength - StyxPacketInfo.HEADER_SIZE, 0,
				               new System.AsyncCallback(Read_Callback), packetInfo);
			} else {
				packetInfo.bytesLeft -= read;
				if (packetInfo.bytesLeft == 0) {
					handlePacketBuffer(packetInfo.packetBuffer);
					StyxPacketInfo newPacketInfo = new StyxPacketInfo();
					newPacketInfo.socket = s;
					s.BeginReceive(newPacketInfo.headerBuffer, 0, StyxPacketInfo.HEADER_SIZE, 0, new System.AsyncCallback(Read_Callback), newPacketInfo);
				} else {
					s.BeginReceive(packetInfo.packetBuffer, packetInfo.packetLength - packetInfo.bytesLeft, packetInfo.bytesLeft, 0, new AsyncCallback(Read_Callback), packetInfo);
				}
			}
		}		
	}
	
	private void handlePacketBuffer(byte[] buffer)
	{
		ProtocolObject protocolObject = serializer.unpack(buffer);
		protocalObjectQueue.Enqueue(protocolObject);
	}

	/// <summary>
	/// Process all packet from cubeia server.
	/// </summary>
	/// <param name="protocolObject">Protocol object.</param>
	
	public void handlePacket(ProtocolObject protocolObject)
	{
		Debug.Log("<--------- receive: " + protocolObject.ToString());

		switch (protocolObject.classId()) {
			case LoginResponsePacket.CLASSID:		
				handleLoginResponsePacket((LoginResponsePacket)protocolObject);
				break;
			case TableSnapshotListPacket.CLASSID:
				handleTableSnapshotListPacket((TableSnapshotListPacket)protocolObject);
				break;
			case ServiceTransportPacket.CLASSID:
				ServiceTransportPacketProcessor.handleServiceTransportPacket((ServiceTransportPacket)protocolObject);
				break;
			case GameTransportPacket.CLASSID:
			gameController.handleBallsUpdate((GameTransportPacket)protocolObject);
//				GameTransportPacketProcessor.handleGameTransportPacket((GameTransportPacket)protocolObject);
				break;
			case JoinResponsePacket.CLASSID:
				handleJoinResponsePacket((JoinResponsePacket)protocolObject);
				break;
			case LeaveResponsePacket.CLASSID:
			handleLeaveResponsePacket((LeaveResponsePacket)protocolObject);
				break;
			case PingPacket.CLASSID:
				handlePingPacket((PingPacket)protocolObject);
				break;
		}
	}

	public void handleJoinResponsePacket(JoinResponsePacket joinResponsePacket)
	{
		if (joinResponsePacket.status == Enums.JoinResponseStatus.OK) {
			// go to the game scene
			tableId = joinResponsePacket.tableid;
//			if (Application.loadedLevelName.Equals("GameScene") == false) {
//				Application.LoadLevel("GameScene");
//			}
		}
	}

	void handleLeaveResponsePacket (LeaveResponsePacket leaveResponsePacket)
	{
		if (leaveResponsePacket.status == Enums.ResponseStatus.OK) {
			// leave to the game scene
			if (Application.loadedLevelName.Equals("GameScene")) {
				Application.LoadLevel("LobbyScene");
			}
		}
	}

	private void handlePingPacket(PingPacket pingPacket)
	{
		// reply pingPacket
		int id = pingPacket.id;
		sendPacket(new PingPacket(id));
	}

	private void handleTableSnapshotListPacket(TableSnapshotListPacket tableSnapshotListPacket)
	{
		unsubcribeRoom(currentRoom);
		tableList.Clear();

		GameObject tableListObject = GameObject.Find("TableList");
		TableListControl tableListControl = (TableListControl)tableListObject.GetComponent(typeof(TableListControl));

		foreach (TableSnapshotPacket tableSnapshotPacket in tableSnapshotListPacket.snapshots) {
			TableGame tableGame = new TableGame(tableSnapshotPacket);
			if (tableGame.capacity > 0) {
				tableList.Add(tableGame);
			}
		}

		// update to the view
		tableListControl.updateListItem(tableList);
	}
	
	private void handleLoginResponsePacket(LoginResponsePacket loginResponsePacket)
	{
		if (loginResponsePacket.status == Enums.ResponseStatus.OK &&
			loginResponsePacket.pid > 0) {
			Debug.LogWarning("Succeed to login LangQuat server");
			// make a referrence to the CubeiaClient
			GameApplication.cubeia = this;

			GameApplication.user = new User();
			GameApplication.user.id = loginResponsePacket.pid;

			// TODO: for test
			sendSelectGame();

			// go to the next scene
//			Application.LoadLevel("LobbyScene");
		}
	}

	private byte[] getBytesUTF8(string str)
	{
		byte[] arr = System.Text.Encoding.UTF8.GetBytes(str);
		using (MemoryStream output = new MemoryStream()) {
			output.Write(arr, 0, arr.Length);
			output.Flush();
			arr = output.ToArray();
		}
		return arr;
	}

	private byte[] getBytes(string str)
	{
		byte[] arr = System.Text.Encoding.Default.GetBytes(str);
		using (MemoryStream output = new MemoryStream()) {
			output.Write(arr, 0, arr.Length);
			output.Flush();
			arr = output.ToArray();
		}
		return arr;
	}

	private byte[] ObjectToByteArray(object obj)
	{
		if (obj == null)
			return null;
		BinaryFormatter bf = new BinaryFormatter();
		using (MemoryStream ms = new MemoryStream()) {
			bf.Serialize(ms, obj);
			return ms.ToArray();
		}
	}

	///
	/// 
	/// 
	/// 

	private void sendSelectGame()
	{
		var jsonData = new JSONClass();
		jsonData ["evt"] = "selectG";
		jsonData ["gameid"].AsInt = gameId;
		
		sendService(jsonData.ToString());
	}

	public void sendCreateGame()
	{
		CreateTableRequestPacket createTableRequestPacket = new CreateTableRequestPacket();
		createTableRequestPacket.gameid = gameId;
		createTableRequestPacket.seats = (byte)4;
		createTableRequestPacket.seq = 1;
		createTableRequestPacket.parameters.Clear();
		
		createTableRequestPacket.parameters.Add(new Param("gameId", (byte)Enums.ParameterType.INT, ObjectToByteArray(gameId)));
		createTableRequestPacket.parameters.Add(new Param("Name", (byte)Enums.ParameterType.STRING, ObjectToByteArray("nhao` zo^")));
		createTableRequestPacket.parameters.Add(new Param("Mark", (byte)Enums.ParameterType.INT, ObjectToByteArray(100)));
		createTableRequestPacket.parameters.Add(new Param("gaAGmeId", (byte)Enums.ParameterType.INT, ObjectToByteArray(1000)));
		createTableRequestPacket.parameters.Add(new Param("Vip", (byte)Enums.ParameterType.INT, ObjectToByteArray(0)));
		createTableRequestPacket.parameters.Add(new Param("Player", (byte)Enums.ParameterType.INT, ObjectToByteArray(4)));
		
		sendPacket(createTableRequestPacket);
	}

	// TODO: for test
	public void createTable()
	{
		var jsonData = new JSONClass();
		jsonData ["evt"] = "ctable";
		jsonData ["mark"].AsInt = 1;
		
		sendService(jsonData.ToString());
	}

	public void quickJoinTable()
	{
		var jsonData = new JSONClass();
		jsonData ["evt"] = "searchT";
		jsonData ["gameid"].AsInt = gameId;
		jsonData ["mark"].AsInt = 20;

		sendService(jsonData.ToString());
	}

	public void sendSelectR(RoomGame room)
	{
		var jsonData = new JSONClass();
		jsonData ["evt"] = "selectR";
		jsonData ["gameid"].AsInt = gameId;
		jsonData ["id"].AsInt = room.id;
		
		sendService(jsonData.ToString());
	}

	public void unsubcribeRoom(RoomGame room)
	{
		string lobbyAddress = levelId + "/" + room.id;
		if (lobbyAddress.Length > 0) {
			LobbyUnsubscribePacket packet = new LobbyUnsubscribePacket();
			packet.type = Enums.LobbyType.REGULAR;
			packet.gameid = gameId;
			packet.address = lobbyAddress;
			sendPacket(packet);
		}
	}

	public void subcribeRoom(RoomGame room)
	{
		string lobbyAddress = levelId + "/" + room.id;
		LobbySubscribePacket packet = new LobbySubscribePacket();
		packet.type = Enums.LobbyType.REGULAR;
		packet.gameid = gameId;
		packet.address = lobbyAddress;
		sendPacket(packet);
	}

	public void sendLeaveTable ()
	{
		var jsonData = new JSONClass();
		jsonData ["evt"] = "ltable";
		jsonData ["tableid"].AsInt = tableId;
		
		sendService(jsonData.ToString());
	}

	public void sendJoinTable (int tableID)
	{
		var jsonData = new JSONClass();
		jsonData ["evt"] = "jtable";
		jsonData ["tableid"].AsInt = tableID;
		
		sendService(jsonData.ToString());
	}
}

