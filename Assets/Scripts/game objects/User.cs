using UnityEngine;
using System.Collections;

public class User
{
	public int id;
	public int ag;
	public string name;
	public int lq;
	public int vip;
	public string avatar_url;


	public void updateData(int id, int ag, string name, int lq) {
		this.id = id;
		this.ag = ag;
		this.name = name;
		this.lq = lq;
	}
}

