using UnityEngine;
using System.Collections;


public class BallUpdate : MonoBehaviour {
	public Texture2D tex;
	private float[] rot = {0, 0.7071f, -0.7071f, 0};
	private float x1 = -100.0f, y1 = -100.0f;
	// Use this for initialization
	void Start () {
#if !USE_TEX		
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		block.AddTexture("_MainTex",tex);
		renderer.SetPropertyBlock(block);
#endif
		updateImage ();
	}

	void rotate(float[] q, float x, float y, float angle){
		float n = (float)Mathf.Sqrt(x*x + y*y);
		float s = (float)Mathf.Sin(0.5f*angle)/n;
		float q2x = x*s;
		float q2y = y*s;
		float q2w = (float)Mathf.Cos(0.5f*angle);
		float dx, dy, dz, dw;
		dx = q[0] * q2w + q[3] * q2x - q[2] * q2y;
		dy = q[1] * q2w + q[3] * q2y + q[2] * q2x;
		dz = q[2] * q2w + q[0] * q2y - q[1] * q2x;
		dw = q[3] * q2w - q[0] * q2x - q[1] * q2y;
		q[0] = dx;
		q[1] = dy;
		q[2] = dz;
		q[3] = dw;
	}
	
#if USE_TEX
	public void rotate(float[] q, float[] v){
		float vx, vy, vz, vw;
		vx = (q[3] * v[0] + q[1] * v[2] - q[2] * v[1]);
		vy = (q[3] * v[1] + q[2] * v[0] - q[0] * v[2]);
		vz = (q[3] * v[2] + q[0] * v[1] - q[1] * v[0]);
		vw = (-q[0] * v[0] - q[1] * v[1] - q[2] * v[2]);
		v[0] = vx * q[3] - vw * q[0] - vy * q[2] + vz * q[1];
		v[1] = vy * q[3] - vw * q[1] - vz * q[0] + vx * q[2];
		v[2] = vz * q[3] - vw * q[2] - vx * q[1] + vy * q[0];
	}
#endif	
	
	public void normalize(float[] q){
		float len = (float)Mathf.Sqrt(q[0]*q[0] + q[1]*q[1] + q[2]*q[2] + q[3]*q[3]);
		q[0] /= len;
		q[1] /= len;
		q[2] /= len;
		q[3] /= len;
	}

	// Update is called once per frame
	void Update () {
		if (x1 == -100.0f)
			x1 = transform.position.x;
		if (y1 == -100.0f)
			y1 = transform.position.y;

		float dx = transform.position.x - x1;
		float dy = transform.position.y - y1;
		
		float len = (float)Mathf.Sqrt(dx*dx + dy*dy)*0.5f;
		if (len > 0.01f){
			rotate(rot, dy/len, -dx/len,len*0.05f);
			normalize(rot);
			updateImage();
		}
		
		x1 = transform.position.x;
		y1 = transform.position.y;
	}

#if USE_TEX
	Color trans = new Color(0, 0, 0, 0);
#endif
	void updateImage() {
#if USE_TEX
		int BALL_SIZE = tex.width / 2;
		int TEX_SIZE = tex.width;
		Texture2D texture = new Texture2D(BALL_SIZE, BALL_SIZE);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;

		float[] v = new float[3];
		for(int ix = 0; ix < BALL_SIZE; ix++){
			for(int iy = 0; iy < BALL_SIZE; iy++){
				float x = (2*ix - BALL_SIZE)/(float)BALL_SIZE;
				float y = (2*iy - BALL_SIZE)/(float)BALL_SIZE;
				float z2 = (x*x + y*y); 
				if (z2 < 1.0f){
					float z = (float)Mathf.Sqrt(1.0f - z2);
					v[0] = x;
					v[1] = y;
					v[2] = z;
					
					rotate(rot, v);
					float theta = (float)Mathf.Acos(v[2]);
					float phi = (float)(Mathf.Atan2(v[1], v[0]) + Mathf.PI);
					int ty = (int)((theta/Mathf.PI)*TEX_SIZE);
					int dty = (int)(0.5f*(phi/Mathf.PI)*TEX_SIZE);
					int tx = TEX_SIZE - dty;
					tx &= TEX_SIZE-1;
					ty &= TEX_SIZE-1;
					texture.SetPixel(ix,iy,tex.GetPixel(tx, ty));
				} else {
					texture.SetPixel(ix,iy,trans);
				}
			}
		}
		texture.Apply ();
		MaterialPropertyBlock block = new MaterialPropertyBlock();
		block.AddTexture("_MainTex",texture);
		renderer.SetPropertyBlock(block);
#else
		renderer.material.SetVector ("_Rotation", new Vector4(rot[0], rot[1], rot[2], rot[3]));
#endif
	}
}
