using UnityEngine;
using System.Collections;
using com.cubeia.firebase.io.protocol;
using SimpleJSON;

public class GameTransportPacketProcessor
{
		public static void handleGameTransportPacket (GameTransportPacket data)
		{	
				CubeiaClient cubeia = GameApplication.cubeia;

				cubeia.tableId = data.tableid;
		
				string jsonGameTransportPacket = System.Text.Encoding.UTF8.GetString (data.gamedata);
				Debug.Log ("GameTransportPacket: " + jsonGameTransportPacket);
		
				var gameData = JSONNode.Parse (jsonGameTransportPacket);
				string evt = gameData ["evt"];
		
				if (evt.Equals ("ctable")) {
						var jsonData = new JSONClass ();
						jsonData ["evt"] = "play";
						jsonData ["data"] = "nguyenhaian";
			
						cubeia.sendDataGame (jsonData);
				} else if (evt.Equals ("play")) {
						//			String str = "";
						//			foreach (byte b in data.gamedata)
						//				str += (byte)b + ", ";
						//
						//			Debug.LogWarning(str);
						//			Debug.LogWarning("length: "+data.gamedata.Length);
						//			sendPacket(data);
				} else if (evt.Equals ("ltable")) {
					// khi gui ltable len, neu tra ve LeaveResponsePacket thi back ra LobbyScene
					// truong hop khac thi tra ve ten nguoi thoat ban choi
					// => day nguoi do ra khoi ban choi
				}

		
		}
}

