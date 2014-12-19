using UnityEngine;
using System.Collections;

public class CamStart : MonoBehaviour {
#if USE_LINE
	private GameObject cue;
	private GameObject cueBall;
	private Material lineMaterial; 
#endif
	// Use this for initialization
	void Start () {
#if USE_LINE
		cue = GameObject.Find ("Cue");
		cueBall = GameObject.Find ("CueBall");
		lineMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
		                            "SubShader { Pass {" +
		                            "   BindChannels { Bind \"Color\",color }" +
		                            "   Blend SrcAlpha OneMinusSrcAlpha" +
		                            "   ZWrite Off Cull Off Fog { Mode Off }" +
		                            "} } }");
		lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
#endif
	}
	
	public float floatable = 1.0f; //This can be PixelsPerUnit, or you can change it during runtime to alter the camera.
	
	void Update ()
	{
		floatable = Screen.height / 480.0f;
		this.camera.orthographicSize = Screen.height / floatable / 2.0f;//- 0.1f;
	}

#if USE_LINE
	void OnPostRender() {
		GL.PushMatrix();
		lineMaterial.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.LINES);
		GL.Color(Color.white);
		Vector3 startVex = this.camera.WorldToViewportPoint (cue.transform.position);
		Vector3 endVex = this.camera.WorldToViewportPoint (cueBall.transform.position);
		startVex.z = 0;
		endVex.z = 0;
		Debug.Log (startVex);
		Debug.Log (endVex);
		GL.Vertex(startVex);
		GL.Vertex(endVex);
		GL.End();
		GL.PopMatrix();
	}
#endif
}