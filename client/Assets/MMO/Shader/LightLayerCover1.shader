// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'unity_World2Shadow' with 'unity_WorldToShadow'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/LightLayerCover1"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

         //_LightDir1("lightdir1",Range(0.0,255.0)) = 0
        //_LightDir2("lightdir2",Range(0.0,255.0)) = 0
        //_LightDir3("lightdir3",Range(0.0,255.0)) = 0
        _Bump("Bump",2D) = "bump"{}
        _GlassCover ("GlassCover",2D) = "white"{}
        _CoverCol("CoverCol",COLOR) = (1,1,1,1)
	}
	SubShader
	{
      
		Tags { "RenderType"="Opaque"  }
        //Tags { "RenderType"="Opaque" "IGNOREPROJECTOR"="true" "queue"="geometry"}
		LOD 200

		Pass
		{
        Tags{ "LightMode" = "ForwardBase"  }//不指明，会出现明暗显示问题，暗部忽明忽暗
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
                fixed4 tangent : TANGENT;//切线
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 worldNormal : NORMAL0;
				float3 lightDir :TEXCOORD1;
                //float3 norm : NORMAL1;
                float3 worldPos :texcoord2;
                fixed4 t0 : TEXCOORD3;
                fixed4 t1 : TEXCOORD4;
                fixed4 t2 : TEXCOORD5;

                //fixed3 _LightCoord : TEXCOORD6;
                //fixed4 _ShadowCoord : TEXCOORD7;
				LIGHTING_COORDS(5,6)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;


            sampler2D _Bump;
            //float4 _Bump_ST;

            sampler2D _GlassCover;
            fixed4 _CoverCol;

            //float _LightDir1;
            //float _LightDir2;
            //float _LightDir3;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                //o.norm = v.normal;
                float3 worldPos = mul(o.vertex,(float3x3)unity_ObjectToWorld);//.xyz;
                //TANGENT_SPACE_ROTATION;
                //或
                float3 binormal = cross(v.normal, v.tangent.xyz)*v.tangent.w;
                float3x3 rotation = float3x3 (v.tangent.xyz,binormal,v.normal);

                float3x3 ot = mul(rotation,(float3x3)unity_WorldToObject);//unity_WorldToObject);
                 o.t0 = float4(ot[0].xyz,worldPos.x);
                 o.t1 = float4(ot[1].xyz,worldPos.y);
                 o.t2 = float4(ot[2].xyz,worldPos.z);
                 o.worldPos = worldPos;

                TRANSFER_VERTEX_TO_FRAGMENT(o);
                //或 下方存在兼容问题
                //o._LightCoord = mul(_LightMatrix0,mul(unity_ObjectToWorld,v.vertex)).xyz;
                //o._ShadowCoord = mul(unity_WorldToShadow[0],mul(unity_ObjectToWorld,v.vertex));

                UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

                fixed4 nor = tex2D(_Bump,i.uv);
                fixed3 norm ;
                norm.xy = nor.wy *2 -1;
                norm.z = sqrt(1- saturate(dot(norm.xy,norm.xy)));

            //float3 worldPos = mul(unity_ObjectToWorld,i.vertex).xyz;//float3(_LightDir1,_LightDir2,_LightDir3);
                //float3 worldPos =  float3(_LightDir1,_LightDir2,_LightDir3);

                //float3 worldPos = float3(i.t0.w, i.t1.w, i.t2.w); //xxxxx


               

                //float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                //或
                //float3 worldViewDir = mul((float3x3)_Object2World,(ObjSpaceViewDir(i.vertex)));
                //或
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.vertex));

                //================================================================================
                float3 cviewDir = normalize(ObjSpaceViewDir(i.vertex));
                float cviewNor = saturate(dot(i.worldNormal,normalize(cviewDir)));//相对当前视角的法线夹角
                //================================================================================


                //i.lightDir = mul(_Object2World, ObjSpaceLightDir(i.vertex));
                //或
                i.lightDir = normalize(UnityWorldSpaceLightDir(i.vertex));
                //或
                //i.lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));




                    
                 
                //i.worldNormal =  normalize(mul((norm) , _Object2World));//mul(i.norm,unity_WorldToObject);//;
                //或
                   i.worldNormal = normalize(mul(norm,float3x3(i.t0.xyz,i.t1.xyz,i.t2.xyz)));//效果最好
                //或
                //i.worldNormal =  normalize( dot(UnityObjectToWorldNormal(norm), worldViewDir));


				float3 ambi = UNITY_LIGHTMODEL_AMBIENT.rgb;
				float3 lambert = _LightColor0.rgb * saturate( dot(normalize(i.worldNormal), normalize(i.lightDir)));
                 

                 float atten = LIGHT_ATTENUATION(i);//自身投影
				// sample the texture
				fixed4 tex = tex2D(_MainTex, i.uv);
                 //=================== _GlassCover ======
                 float yoyo2 =   max(0, sin(_SinTime* 25));
                 fixed3 glassCoverTex = tex2D(_GlassCover,i.uv) * _CoverCol +yoyo2;
                 //fixed3 glass = glassCoverTex.rgb * saturate(dot(1- lambert,pow(i.worldNormal,4)));
                 fixed3 glass = glassCoverTex.rgb * saturate(dot(1- cviewDir,pow(i.worldNormal,4))) * yoyo2;
                 //====================
                 //rim
                 //float3 rimFace = pow(saturate(1 - cviewDir),2);
                 //tex.rgb += rimFace;
                 //======
				 tex.rgb =  fixed3(tex.rgb   * ( ambi  + lambert) *1)+tex.rgb*.1 + glass*.3;//+ tex.rgb *lambert*2);
                 //tex.rgb = fixed3(tex.rgb   * ( ambi + atten*.5 + lambert) );
				tex.a = 1;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, tex);
				return tex;
			}
			ENDCG
		}
    }
         Fallback "Diffuse"//如果不写这个，就不显示投影
        //Fallback "VertexLit"
}
