using UnityEngine;

// Statics for holding the connection to the SFS server end
// Can then be queried from the entire game to get the connection

public class GameApplication : MonoBehaviour
{
	private static GameApplication mInstance;
	private static CubeiaClient cubeiaClient;

	public static CubeiaClient cubeia {
		get {
			if (mInstance == null) {
				mInstance = new GameObject("GameApplication").AddComponent(typeof(GameApplication)) as GameApplication;
			}
			return cubeiaClient;
		}
		set {
			if (mInstance == null) {
				mInstance = new GameObject("GameApplication").AddComponent(typeof(GameApplication)) as GameApplication;
			}
			cubeiaClient = value;
		} 
	}
	
	public static User user{ get; set; }

	public static bool IsInitialized {
		get { 
			return (cubeiaClient != null && user != null); 
		}
	}
	
	// Handle disconnection automagically
	// ** Important for Windows users - can cause crashes otherwise
	void OnApplicationQuit()
	{ 
		//		if (cubeiaClient.IsConnected) {
		//			cubeiaClient.Disconnect();
		//		}
	} 
}