using UnityEngine;
using System.Collections;
using com.cubeia.firebase.io.protocol;
using SimpleJSON;

public class ServiceTransportPacketProcessor
{
	public static void handleServiceTransportPacket(ServiceTransportPacket serviceTransportPacket)
	{
		User user = GameApplication.user;
		CubeiaClient cubeia = GameApplication.cubeia;

		string jsonServiceTransportPacket = System.Text.Encoding.UTF8.GetString(serviceTransportPacket.servicedata);
		Debug.Log("ServiceTransportPacket: " + jsonServiceTransportPacket);
		
		var serviceData = JSONNode.Parse(jsonServiceTransportPacket);
		string evt = serviceData ["evt"];

		if (evt.Equals("0")) {
			// TODO: doan nay cua game mini
			var data = JSONNode.Parse(serviceData ["data"]);
			user.id = data ["userid"].AsInt;
			user.name = data ["username"];
			user.ag = data ["gold"].AsInt;
			user.vip = data ["vip"].AsInt;


		} else if (evt.Equals("2") || evt.Equals("getLR")) {
			JSONArray data = JSONNode.Parse(serviceData ["data"]).AsArray;
			Debug.Log(string.Format("list rooms size : {0}", data.Count));

//			GameObject roomListObject = GameObject.Find("RoomList");
//			RoomListControl roomListControl = (RoomListControl)roomListObject.GetComponent(typeof(RoomListControl));

			cubeia.roomList = new RoomGame[data.Count];
			for (int i = 0; i < data.Count; i++) {
				bool isFree = false;
				if (i == data.Count - 1)
					isFree = true;
				RoomGame roomGame = new RoomGame(
					data [i] ["Id"].AsInt,
					data [i] ["Name"],
					data [i] ["MaxTable"].AsInt,
					data [i] ["CurPlay"].AsInt,
					data [i] ["MaxPlay"].AsInt,
					data [i] ["CurTable"].AsInt,
					isFree
				);
				cubeia.roomList [i] = roomGame;
				Debug.Log("Parsed RoomGame: " + roomGame.toString());

				// update to the view
//				roomListControl.AddNewItem(roomGame);
			}

			// vao room
			// cubeia.unsubcribeRoom(cubeia.currentRoom);
			cubeia.currentRoom = cubeia.roomList [0];
			cubeia.sendSelectR(cubeia.currentRoom);
			
		} else if (evt.Equals("3") || evt.Equals("selectR")) {
			// sau khi nhan tin hieu vao room, subcribe room nay
//			cubeia.subcribeRoom(cubeia.currentRoom);
			cubeia.quickJoinTable();

			//			sendService("{\"evt\"=\"searchT\",\"gameid\"=8006}");
			//			sendCreateGame();
		}

	}
	// Use this for initialization


}

