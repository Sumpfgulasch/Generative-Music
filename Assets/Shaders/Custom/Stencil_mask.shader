// Stencil buffer shader - WRITE
//
// muss als erstes gerendert werden (vor obj mit read-shader)

Shader "Universal Render Pipeline/Custom/Stencil_mask"
{
    SubShader
	{
		Tags {"RenderType" = "Opaque" "Queue" = "Geometry+1"}
		//LOD 200
		ColorMask 0
		Pass 
		{
			// write
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGINCLUDE


			
			



			struct appdata {
				float4 vertex : POSITION;
				};
			struct v2f {
				float4 pos : SV_POSITION;
				};
			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			half4 frag(v2f i) = SV_Target{
				return half4(1,1,1,1);
			}
			ENDCG
		}
	}
}
