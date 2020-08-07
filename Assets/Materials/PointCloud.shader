Shader "Custom/PointCloud" {
		SubShader{
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma enable_d3d11_debug_symbols

			#include "UnityCG.cginc"

			struct VertexInput {
				float4 v : POSITION;
				float4 color: COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 col : COLOR;
				float size : PSIZE;
			};

			uniform float _ScreenHeight;
			uniform float _tanFOV;
			uniform float _n;
			uniform float _r;
			uniform float _times;
			float _PointSize;

			VertexOutput vert(VertexInput v) {

				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.v);
				o.col = v.color;
				_PointSize = _times * _r * _n * _ScreenHeight / (o.pos.z * _tanFOV);
				o.size = _PointSize;
				return o;
			}

			float4 frag(VertexOutput o) : COLOR {
				return o.col;
			}

			ENDCG
			}
	}
}