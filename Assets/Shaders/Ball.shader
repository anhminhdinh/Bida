Shader "Custom/Ball" 
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Rotation ("Rotation", Vector) = (0,0,0,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 xbox360 gles
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
//			#pragma target 3.0
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			
			float3 rotate(float4 q, float3 v){
				float vx, vy, vz, vw;
				vx = (q.w * v.x + q.y * v.z - q.z * v.y);
				vy = (q.w * v.y + q.z * v.x - q.x * v.z);
				vz = (q.w * v.z + q.x * v.y - q.y * v.x);
				vw = (-q.x * v.x - q.y * v.y - q.z * v.z);
				v.x = vx * q.w - vw * q.x - vy * q.z + vz * q.y;
				v.y = vy * q.w - vw * q.y - vz * q.x + vx * q.z;
				v.z = vz * q.w - vw * q.z - vx * q.y + vy * q.x;
				return v;
			}
			
			float4 _Rotation;
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
//				float2 coord = IN.texcoord;
//				float x = 2*coord.x - 1;
//				float y = 2*coord.y - 1;
//				float z2 = x*x + y*y;
//				fixed4 c0 = {0 , 0, 0, 0};
//				const float PI = 3.1415926;
//				if (z2 < 1.0f) {
//					float z = (float)sqrt(1.0f - z2);
//					float3 v = {x, y, z};										
//					v = rotate(_Rotation, v);
//					
//					float theta = (float)acos(v.z);
//					float phi = (float)(atan2(v.y, v.x) + PI);
//					coord.y = theta/PI;
//					coord.x = 1 - phi/(PI*2);
//					//int ty = (int)((theta/PI)*TEX_SIZE);
//					//int tx = TEX_SIZE - (int)((phi/(Math.PI*2))*TEX_SIZE);
//				}
//				fixed4 c = tex2D(_MainTex, coord) * IN.color;
//				float delta = 0.2f;
//				float alpha = smoothstep(1.0f-delta, 1.0f, sqrt(z2));
//            	c = c*(1 - alpha) + c0*alpha;//mix(c, c0, alpha);
//				return c;
			}
		ENDCG
		}
	}
}
