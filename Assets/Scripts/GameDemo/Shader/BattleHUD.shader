// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "FPS/HUD/BattleHUD" 
{
	Properties
	{
		_Color_BloodBg ("Blood Bg", Color) = (1, 1, 1, 1)
		_Color_Blood ("Blood", Color) = (1, 1, 1, 1)

		_Progress_A ("Progress A", Range(0, 1)) = 1

		_F("Factor",Range(0.01,0.03)) = 0.02
		_Scale("Scale",Vector) = (1,1,1)

	}

	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		
		Cull Back ZTest Off ZWrite Off
		//Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color_BloodBg;
			fixed4 _Color_Blood;

			fixed _Progress_A;
			fixed _F;
			fixed3 _Scale;

			float4 RotateAroundYInDegrees(float4 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float4(mul(m, vertex.xz), vertex.yw).xzyw;
			}

			float4 RotateAroundXInDegrees(float4 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				float2 yz = mul(m, vertex.yz);
				return float4(vertex.x, yz.x, yz.y, vertex.w);
			}


			v2f vert (a2v v)
			{
				/*v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
				*/

				v2f output;

				float4x4 mv = UNITY_MATRIX_MV;

				// First colunm.
				mv._m00 = _Scale.x;
				mv._m10 = 0.0f;
				mv._m20 = 0.0f;

				// Second colunm.
				mv._m01 = 0.0f;
				mv._m11 = _Scale.y;
				mv._m21 = 0.0f;

				// Thrid colunm.
				mv._m02 = 0.0f;
				mv._m12 = 0.0f;
				mv._m22 = _Scale.z;

				output.vertex = mul(UNITY_MATRIX_P, mul(mv, v.vertex));

				output.uv = v.uv;

				return output;

				
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed x = 1 - i.uv.x;
				
				fixed a = abs(i.uv.x - 0.5);
				a = smoothstep(0.0001,0.5-0.0001,a);
				fixed d = sign(i.uv.x - 0.5);

				x = step(i.uv.x - d * a * _F, _Progress_A );
				return (1-x) *_Color_BloodBg  + x * _Color_Blood;

				/*fixed4 col = x <= _Progress_A ? _Color_Blood : _Color_BloodBg;
				return col;*/
			}
			ENDCG
		}
	}
	
	FallBack "Diffuse"
}
