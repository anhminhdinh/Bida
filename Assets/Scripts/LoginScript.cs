using UnityEngine;
using System.Collections;
using System.Linq;
using Styx;

public class LoginScript : MonoBehaviour
{

	CubeiaClient cubeia;

	// Use this for initialization
	void Start()
	{
		cubeia = new CubeiaClient();
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		cubeia.processEvents();
	}

	public void login()
	{
		cubeia.TestLogin();
	}
}
