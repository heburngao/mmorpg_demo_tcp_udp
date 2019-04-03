// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/LightLayerFlow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _MainColor("MainColor",COLOR) = (1,1,1,1)

         //_LightDir1("lightdir1",Range(0.0,255.0)) = 0
        //_LightDir2("lightdir2",Range(0.0,255.0)) = 0
        //_LightDir3("lightdir3",Range(0.0,255.0)) = 0
        _Bump("Bump",2D) = "bump"{}
        //_GlassCover ("GlassCover",2D) = "white"{}


        _FlowTex("FlowTex",2D) = "white"{}
        _Noise("Noise",2D) = "white"{}
        _NoiseColor("NoiseColor",Color)= (1,1,1,1)
	}
	SubShader
	{
      
		Tags { "RenderType"="Opaque"  }
        //Tags { "RenderType"="Opaque" "IGNOREPROJECTOR"="true" "queue"="geometry"}
		LOD 200

		Pass
		{
        Tags{ "LightMode" = "ForwardBase"  "DisableBatching" = "false" }//不指明，会出现明暗显示问题，暗部忽明忽暗
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
				LIGHTING_COORDS(5,6)
			};

			sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _MainColor;


            sampler2D _Bump;
            //float4 _Bump_ST;

            sampler2D _GlassCover;
            float4 _GlassCover_ST;

            //==流光
            sampler2D _FlowTex;
            fixed4 _FlowTex_ST;
            sampler2D _Noise;
            fixed4 _Noise_ST;
            //=====
            fixed4 _NoiseColor;





            //float _LightDir1;
            //float _LightDir2;
            //float _LightDir3;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                //o.norm = v.normal;
                float3 worldPos = mul(o.vertex,(float3x3)unity_ObjectToWorld);//.xyz;
                TANGENT_SPACE_ROTATION;
                float3x3 ot = mul(rotation,(float3x3)unity_WorldToObject);//unity_WorldToObject);
                 o.t0 = float4(ot[0].xyz,worldPos.x);
                 o.t1 = float4(ot[1].xyz,worldPos.y);
                 o.t2 = float4(ot[2].xyz,worldPos.z);
                 o.worldPos = worldPos;

                TRANSFER_VERTEX_TO_FRAGMENT(o);
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
                //i.lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                ////////
                //i.lightDir = normalize(UnityWorldSpaceLightDir(i.vertex));
                //或
                i.lightDir = normalize(_WorldSpaceLightPos0.xyz);


                    
                 
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
                //或主贴图流动
                //float2 mainFlowUV = i.uv + _Time.y * .01 ;
                //fixed4 tex = tex2D(_MainTex,TRANSFORM_TEX(mainFlowUV,_MainTex)+.1);

                float3 viewLambert = _LightColor0.rgb * saturate( dot(normalize(i.worldNormal), normalize(cviewDir)));
                 //==flow light 流光 
                 float2 noiseUV = i.uv + _Time.y * .01 ;
                 float2 noiseTex = tex2D(_Noise , TRANSFORM_TEX(noiseUV,_Noise)+.1);//i.uv ,_Noise)+.5);
                 float2 flowUV = i.uv + _Time.y * .5 ;
                 fixed4 flowTex = tex2D(_FlowTex, TRANSFORM_TEX(flowUV.rg,_FlowTex)+ noiseTex.y*2);
                 fixed3 glass = flowTex.rgb * saturate( dot(1 - viewLambert , pow(i.worldNormal,4))) * 1*_NoiseColor;
                 //=================== _GlassCover ======
                 /*
                 float yoyo2 =  1.2;//max(0, sin(_SinTime* 25));
                 fixed3 glassCoverTex = tex2D(_GlassCover,i.uv+.1) * _CoverCol +yoyo2;
                 fixed4 flowTex = tex2D(_GlassCover, TRANSFORM_TEX(i.uv,_GlassCover)+ glassCoverTex.y*2);
                 //fixed3 glass = glassCoverTex.rgb * saturate(dot(1- lambert,pow(i.worldNormal,4)));
                 fixed3 glass = glassCoverTex.rgb * saturate(dot(1- cviewDir,pow(i.worldNormal,4))) * yoyo2 + flowTex;
                 */
                 //====================
                 //rim
                 //float3 rimFace = pow(saturate(1 - cviewDir),2);
                 //tex.rgb += rimFace;
                 //======
				 tex.rgb +=  fixed3(tex.rgb   * ( ambi  + viewLambert) *1) * _MainColor.xyz +tex.rgb*.1 + glass*1;//+ tex.rgb *lambert*2);
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
