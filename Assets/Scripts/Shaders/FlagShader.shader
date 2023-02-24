Shader "FlagShader"
{
	Properties
	{
		_MainTex("Base texture", 2D) = "white" {}
		_AmbientColor("Ambient Color", Color) = (1.0,1.0,1.0)
		_AmbientStrength("Ambient Strength", float) = 1.0

	}

	SubShader
	{
		Cull Off
		Tags {"LightMode" = "ForwardBase" "Queue" = "Transparent"}
		Pass
		{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			// user defined variables
			uniform float4 _AmbientColor;
			uniform float _AmbientStrength;
			sampler2D _MainTex;

			// unity 3 definitions
			// float4x4 _Object2World;
			// float4x4 _World2Object;
			// float4 _WorldSpaceLightPos0;

		 //   #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight


			// base input structs
			struct vertexInput {
				float4 vertex: POSITION;
				float3 normal: NORMAL;
				float2 uv: TEXCOORD0;
			};

			struct vertexOutput {
				float4 pos: SV_POSITION;
				float4 diff: COLOR;
				float4 ambient: COLOR1;
				float2 uv: TEXCOORD0;
				SHADOW_COORDS(1) 

			};


			// vertex functions
			vertexOutput vert(vertexInput v) {
				vertexOutput o;

				float3 normalDirection = normalize(mul(float4(v.normal, 0.0),unity_WorldToObject).xyz);
				float3 lightDirection;
				float atten = 1.0;

				lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 diffuse = atten *_LightColor0.xyz*max(0.0,dot(normalDirection, lightDirection));
				o.diff = float4(diffuse, 1.0);
				o.ambient = _AmbientColor * _AmbientStrength;

		//		v.vertex.x += cos((v.vertex.y + _Time.y * _Speed) * _Frequency) * _Amplitude * (v.vertex.y - 5);
				v.vertex.x += cos(v.vertex.y * 400 + _Time.y * 5) * 0.002 * min(1,max(0,v.vertex.y * 100));

				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv = v.uv;
				TRANSFER_SHADOW(o)


				return o;
			}

			// fragment function
			float4 frag(vertexOutput i) : COLOR
			{ 
				fixed4 base_col = tex2D(_MainTex, i.uv);
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed3 lighting = i.diff * shadow + i.ambient;
				fixed4 col = base_col;
				col.rgb *= lighting;

				return col;
			}

			ENDCG
		}

		Pass
        {
            Tags {"LightMode"="ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f { 
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
	}
}