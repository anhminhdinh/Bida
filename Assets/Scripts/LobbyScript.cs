using UnityEngine;
using System.Collections;
using System.Linq;
using Styx;

public class LobbyScript : MonoBehaviour
{

	CubeiaClient cubeia;

	// Use this for initialization
	void Start()
	{
		if (GameApplication.IsInitialized == false) {
			// go to the next scene
			Application.LoadLevel("LoginScene");
			return;
		}

		cubeia = GameApplication.cubeia;
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		if (GameApplication.IsInitialized)
			cubeia.processEvents();
	}


}
