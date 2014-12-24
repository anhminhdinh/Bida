#define _AUTO_PLAY
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;
using SimpleJSON;
using com.cubeia.firebase.io.protocol;

enum GAMESTATE {START, AIM, REPLAY, SHOOTING, SHOOT, WAIT};

public class Ball {
	public List<float> orgx = new List<float>();
	public List<float> orgy = new List<float>();
	public List<float> destx = new List<float>();
	public List<float> desty = new List<float>();
	public List<bool> st = new List<bool>();
}

public class GameController : MonoBehaviour {
	public GameObject ball;


	private GameObject cue;
	private CubeiaClient cubeia;
	private GameObject cueHead;
	private GameObject[] GOS;
	private Dictionary<string,Ball> GOSDict = new Dictionary<string, Ball>();
	private GAMESTATE gameState = GAMESTATE.START;
	private VectorLine shootLine;
	private VectorLine reflectLine;
	private VectorLine ballLine;
	private Rect ShootRect = new Rect(Screen.width - 100, Screen.height - 80, 80, 60);

	private const float DELAY = 0.200f;
	private float cueLength = 0;
	private float updateBallsTimer = 0;
	private float lastUpdateBallsTimer = 0;
	private float timeToUpdateBalls = 0.0f;
	// Use this for initialization
	void Start () {
		Application.targetFrameRate = -1;
		cubeia = new CubeiaClient ();
		cubeia.gameController = this;
		Material newMat = Resources.Load("VectrosityDemos/Materials/VectorLineGlow", typeof(Material)) as Material;
		shootLine = new VectorLine("ShootLine", new Vector2[2], Color.white, newMat, 8.0f, LineType.Continuous);
		reflectLine = new VectorLine("ReflectLine", new Vector2[2], Color.white, newMat, 4.0f, LineType.Continuous);
		ballLine = new VectorLine("BallLine", new Vector2[2], Color.white, newMat, 8.0f, LineType.Continuous);
		cue = GameObject.Find ("Cue");
		cueHead = GameObject.Find ("Head");
		cueLength = cue.renderer.bounds.size.y;
		cue.transform.Rotate (0, 0, -90);

		ArrangeBalls ();

		GOS = GameObject.FindGameObjectsWithTag ("Ball");
		foreach (GameObject GO in GOS) {
			GOSDict.Add(GO.name, new Ball());
		}
		Debug.Log (GOS.Length);
		Invoke ("PlaceCueAtStart", 0.1f);
		cubeia.TestLogin ();
	}

	void ArrangeBalls() {
		Vector3 cv = new Vector3 (-120.0f, -42.0f, 0);
		transform.position = cv;

		Vector3 ov = new Vector3 (120.0f, -42.0f, 0);
		//1
		GameObject newball = GameObject.Find ("Ball1");
		newball.transform.position = ov;
		float r = newball.renderer.bounds.size.x;// + 1.0f;

		//2, 3
		Vector3 vb = ov;
		vb.x += r*Mathf.Sqrt(3)/2.0f;
		vb.y = ov.y - r / 2.0f;
		newball = GameObject.Find ("Ball2");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball3");
		newball.transform.position = vb;

		//4, 5, 6
		vb.x += r*Mathf.Sqrt(3)/2.0f;
		vb.y = ov.y - r;
		newball = GameObject.Find ("Ball4");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball5");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball6");
		newball.transform.position = vb;

		//7, 8, 9, 10
		vb.x += r*Mathf.Sqrt(3)/2.0f;
		vb.y = ov.y - r * 1.5f;
		newball = GameObject.Find ("Ball7");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball8");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball9");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball10");
		newball.transform.position = vb;

		//11, 12, 13, 14, 15
		vb.x += r*Mathf.Sqrt(3)/2.0f;
		vb.y = ov.y - r * 2.0f;
		newball = GameObject.Find ("Ball11");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball12");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball13");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball14");
		newball.transform.position = vb;
		vb.y += r;
		newball = GameObject.Find ("Ball15");
		newball.transform.position = vb;
	}

	// Update is called once per frame
	void Update () {
		if (GUIUtility.hotControl != 0)
			return;
#if !UNITY_EDITOR	
		if (Input.touchCount > 0) {
			if (gameState == GAMESTATE.SHOOT)
				return;
			Vector2 touchPosition = Input.GetTouch(0).position;
			if (Input.GetTouch(0).phase == TouchPhase.Moved) {
				Vector2 delta = Input.GetTouch(0).deltaPosition;
				Vector2 cuePos = Camera.main.WorldToScreenPoint (transform.position);
				Vector2 prevPos = touchPosition - delta;
				float angle = Vector2.Angle(prevPos - cuePos, touchPosition - cuePos);
				Vector3 cross = Vector3.Cross(touchPosition - cuePos, prevPos - cuePos);
				
				if (cross.z > 0)
					angle = 360 - angle;
				cue.transform.Rotate(0, 0, angle);
				Vector3 v = cueHead.transform.position - cue.transform.position;
				Vector3 t = transform.position + v;
				Vector2 newScreenPos = Camera.main.WorldToScreenPoint (t);
				PlaceCue(newScreenPos);
			}
		}
#else
		if (Input.GetMouseButtonDown (0)) {
			if (gameState == GAMESTATE.SHOOT)
				return;
			Vector2 touchPosition = Input.mousePosition;
			PlaceCue(touchPosition);
		}

		if (Input.GetMouseButtonDown (1)) {
			Shot ();
		}
#endif
		if (gameState == GAMESTATE.REPLAY) {
			if (updateBallsTimer <= 0) {
				updateBallsTimer = DELAY;
				timeToUpdateBalls = 0;
				foreach (GameObject GO in GOS) {
					Ball ball = GOSDict[GO.name];
					if (ball.destx.Count > 0) {
						ball.destx.RemoveAt(0);
						ball.desty.RemoveAt(0);
						ball.orgx.RemoveAt(0);
						ball.orgy.RemoveAt(0);
						ball.st.RemoveAt(0);
					}
				}
			} else {
				updateBallsTimer -= Time.deltaTime;
				timeToUpdateBalls += Time.deltaTime;
				foreach (GameObject GO in GOS) {
					Ball ball = GOSDict[GO.name];
					if (ball.destx.Count > 0) {
						float[] dx = ball.destx.ToArray();
						float[] dy = ball.desty.ToArray();
						Vector2 newPos = new Vector2(dx[0], dy[0]);

						float[] ox = ball.orgx.ToArray();
						float[] oy = ball.orgy.ToArray();
						Vector2 oldPos = new Vector2(ox[0], oy[0]);
						Vector2 curPos = newPos - oldPos;
						curPos*=timeToUpdateBalls / DELAY;
						curPos += oldPos;
						GO.rigidbody2D.MovePosition(curPos);

						bool[] st = ball.st.ToArray();
						GO.SetActive(st[0]);
					}
				}
			}
			return;
		} else if (gameState != GAMESTATE.SHOOT)
			return;
		if (updateBallsTimer <= 0) {
			updateBallsTimer = DELAY;
			var jsonData = new JSONClass ();
			jsonData ["evt"] = "balls";
			foreach (GameObject GO in GOS) {
				string ballname = GO.name;
				jsonData [ballname] = GO.name;
				float x = GO.transform.position.x;
				float y = GO.transform.position.y;
				bool st = GO.activeInHierarchy;
				jsonData [ballname + "x"].AsFloat = x;
				jsonData [ballname + "y"].AsFloat = y;
				jsonData [ballname + "st"].AsBool = st;

//				Ball ball = GOSDict[ballname];
//				ball.destx.Add(x);
//				ball.desty.Add(y); 
//				ball.st.Add(st);
//				if (ball.destx.Count != 1) {
//					float[] dx = ball.destx.ToArray();
//					float[] dy = ball.desty.ToArray();
//					ball.orgx.Add(dx[ball.destx.Count - 2]);
//					ball.orgy.Add(dy[ball.desty.Count - 2]);
//				}
			}
			cubeia.sendDataGame(jsonData);
		} else {
			updateBallsTimer -= Time.deltaTime;
//			Debug.Log(updateBallsTimer);
		}

		foreach (GameObject GO in GOS) {
			if (GO.activeInHierarchy && !GO.rigidbody2D.IsSleeping ())
				return;
		}
		gameState = GAMESTATE.AIM;
		foreach (GameObject GO in GOS) {
			if (GO.name == "CueBall")
				continue;
			if (!GO.activeInHierarchy)
				continue;
			Vector3 newPosition = GO.transform.position; 
			Vector2 newScreenPos = Camera.main.WorldToScreenPoint (newPosition);
			PlaceCue (newScreenPos);
			#if AUTO_PLAY
			Invoke ("Shot", 1);
			#endif
			break;
		}
	}

	void PlaceCueAtStart() {
		Vector3 dVec = new Vector3 (100, 0);
		Vector3 newPosition = transform.position + dVec; 
		Vector2 newScreenPos = Camera.main.WorldToScreenPoint (newPosition);
		PlaceCue (newScreenPos);
#if AUTO_PLAY
		Invoke ("Shot", 1);
#endif
	}

	void PlaceCue(Vector2 touchPosition) {
		ballLine.active = false;
		cue.SetActive(true);
		Vector3 v = Camera.main.ScreenToWorldPoint(touchPosition);
		v.z = 0;

		float dy = v.y - transform.position.y;
		float dx = v.x - transform.position.x;
		float angle = 0;
		if (dx != 0) {
			angle = Mathf.Atan(dy/dx);
		}
		cue.transform.rotation = Quaternion.AngleAxis(angle*180/Mathf.PI, Vector3.forward);
		if (dx < 0)
			cue.transform.Rotate(0, 0, 90);
		else
			cue.transform.Rotate(0, 0, -90);
		Vector3 tv = v - transform.position;
		tv.Normalize();
		Vector3 dv = tv * (cueLength*0.5f + renderer.bounds.size.x);
		cue.transform.position = transform.position - dv;
		shootLine.active = true;
		reflectLine.active = true;
		Vector3 originTransform = transform.position + tv * 10.0f;
		Vector2 origin = new Vector2 (originTransform.x, originTransform.y);
		Vector2 direction = new Vector2 (dv.x, dv.y);
		RaycastHit2D hit = Physics2D.Raycast(origin, direction);// + tv * (renderer.bounds.size.x + 0.0001f)
		Vector3 hitPos = new Vector3(hit.point.x, hit.point.y, 0);
		float angleV = Vector3.Angle(hit.normal, tv);
		Vector3 cueScreen = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 hitScreen = Camera.main.WorldToScreenPoint(hitPos);
		shootLine.points2[0] = new Vector2(cueScreen.x, cueScreen.y);
		shootLine.points2[1] = new Vector2(hitScreen.x, hitScreen.y);
		Vector3 hitNor = new Vector3(hit.normal.x, hit.normal.y, 0);
		if (hit.collider.gameObject.tag == "Hole")
			reflectLine.active = false;
		else if (hit.collider.gameObject.tag == "Bank") {
			Vector3 reflect = Vector3.Reflect(tv, hitNor);
			Vector3 dPos = tv * 10.0f / Mathf.Abs (Mathf.Cos(Mathf.PI*angleV/180));
			hitPos -= dPos;
			Vector3 nv = hitPos + reflect*100.0f;
			Vector3 refScreen = Camera.main.WorldToScreenPoint(nv);
			reflectLine.active = true;
			hitScreen = Camera.main.WorldToScreenPoint(hitPos);
			shootLine.points2[1] = new Vector2(hitScreen.x, hitScreen.y);
			reflectLine.points2 [0] = shootLine.points2[1];
			reflectLine.points2 [1] = new Vector2 (refScreen.x, refScreen.y);
		} else {
			Vector3 reflect = tv - hitNor*Mathf.Cos(Mathf.PI*angleV/180);
			Vector3 nv = hitPos + reflect*100.0f;
			Vector3 refScreen = Camera.main.WorldToScreenPoint(nv);
			reflectLine.active = true;
			reflectLine.points2 [0] = shootLine.points2[1];
			reflectLine.points2 [1] = new Vector2 (refScreen.x, refScreen.y);
		}
		if (hit.collider != null && hit.collider.gameObject.transform.parent != null && hit.collider.gameObject.transform.parent.tag == "Ball") {
			ballLine.active = true;

			Vector3 hitTrans = hit.transform.position - hitNor*renderer.bounds.size.x*0.5f;
			Vector3 hitTransNor = hitTrans - hitNor*Mathf.Abs(Mathf.Cos(Mathf.PI*angleV/180))*100.0f;
			Vector3 hitTrans2D = Camera.main.WorldToScreenPoint(hitTrans);
			Vector3 hitTransNor2D = Camera.main.WorldToScreenPoint(hitTransNor);
			ballLine.points2[0] = new Vector2(hitTrans2D.x, hitTrans2D.y);
			ballLine.points2[1] = new Vector2(hitTransNor2D.x, hitTransNor2D.y);
			ballLine.Draw();
		}
		shootLine.Draw();
		reflectLine.Draw ();
	}
	
	void FixedUpdate() {
		cubeia.processEvents ();
		if (gameState == GAMESTATE.WAIT) {
			float timeToUpdate = lastUpdateBallsTimer - updateBallsTimer;
			timeToUpdateBalls += Time.fixedDeltaTime;
			foreach (GameObject GO in GOS) {
				Ball ball = GOSDict[GO.name];
//				Vector2 newPos = new Vector2(ball.destx, ball.desty);
//				if ((GO.transform.position.x != newPos.x) || (GO.transform.position.y != newPos.y)) {
//					Vector2 curPos = new Vector2(ball.orgx, ball.orgy);
//					if (timeToUpdateBalls < timeToUpdate) {
//						Vector2 deltaPos = newPos - curPos;
//						deltaPos *= timeToUpdateBalls / timeToUpdate;
//						curPos += deltaPos;
//						GO.rigidbody2D.MovePosition(curPos);
//						Debug.Log("Smooth");
//					} else {
//						Debug.Log("Lag");
//						GO.rigidbody2D.MovePosition(newPos);
//					}
//				}
			}
			return;
		}

	}
	
	private void OnGUI() {
		if (GUI.Button (ShootRect, "Shot")) {
			Shot();
		}
	}

	void Shot() {
		if (gameState == GAMESTATE.SHOOT)
			return;
		updateBallsTimer = DELAY;
		foreach (GameObject GO in GOS) {
			Ball ball = GOSDict[GO.name];
			ball.orgx.Clear();
			ball.orgx.Add(GO.transform.position.x);
			ball.orgy.Clear();
			ball.orgy.Add(GO.transform.position.y);
			ball.st.Clear();
//			ball.st.Add(GO.activeInHierarchy);
			ball.destx.Clear();
			ball.desty.Clear();
		}
		gameState = GAMESTATE.SHOOT;
		shootLine.active = false;
		reflectLine.active = false;
		ballLine.active = false;
		cue.SetActive (false);
		Vector2 v = new Vector2(transform.position.x - cue.transform.position.x, transform.position.y - cue.transform.position.y);
		v.Normalize();
		rigidbody2D.velocity = v * 800.0f;

//		rigidbody2D.AddForce(-v*6400.0f, ForceMode2D.Force);
	}

	public void handleBallsUpdate(GameTransportPacket data) {
		string jsonGameTransportPacket = System.Text.Encoding.UTF8.GetString (data.gamedata);
		Debug.Log ("GameTransportPacket: " + jsonGameTransportPacket);
		
		var gameData = JSONNode.Parse (jsonGameTransportPacket);
		string evt = gameData ["evt"];
		if (evt == "balls") {
			timeToUpdateBalls = 0;
			if (lastUpdateBallsTimer == 0.0f) {
				updateBallsTimer = Time.fixedTime - DELAY;
				lastUpdateBallsTimer = Time.fixedTime;
			} else {
				updateBallsTimer = lastUpdateBallsTimer;
				lastUpdateBallsTimer = Time.fixedTime;
			}
			gameState = GAMESTATE.WAIT;
			foreach (GameObject GO in GOS) {
				string ballname = GO.name;
				float x = gameData[ballname + "x"].AsFloat;
				float y = gameData[ballname + "y"].AsFloat;
				bool st = gameData[ballname + "st"].AsBool;
				GO.SetActive(st);
				Ball ball = GOSDict[ballname];
				ball.destx.Add(x);
				ball.desty.Add(y); 
				if (ball.destx.Count == 1) {
					ball.orgx.Add (GO.transform.position.x);
					ball.orgy.Add (GO.transform.position.y);
				} else {
					ball.orgx.Add(ball.destx[ball.destx.Count - 2]);
					ball.orgy.Add(ball.desty[ball.desty.Count - 2]);
				}
			}
		}
	}
}
