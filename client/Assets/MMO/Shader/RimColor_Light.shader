// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
//人物皮肤带正面光照着色器
Shader "Custom/RimColor_Light" 
{
    Properties
    {
        _MainTex("main tex",2D) = "black"{}
        _RimColor("rim color",Color) = (.11,.11,.11,1)//边缘颜色
        _RimPower ("rim power",range(1,10)) = 10//边缘强度

        //==
        _Specular("Specular",Range(1.0,500.0)) = 250.0
        _Gloss("Gloss",Range(0.0,1.0)) = .2
        _Bump("Bump",2D) = "bump"{}
        _Cubemap("Cubemap",CUBE) = ""{}
        _ReflAmount("Reflection Amount",Range(0,1)) = .5
		_LightDir("lightdir",Range(0.0,1.0)) = 1.0
    }
 
    SubShader
    {
    Tags { "RenderType"="Opaque"}
    LOD 200

        Pass
        {
        	Tags{ "LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include"UnityCG.cginc"

            //==
            #pragma multi_compile_fwdbase

            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            //==
//            struct v2f
//            {
//                float4 vertex:POSITION;
//                float4 uv:TEXCOORD0;
//                float4 NdotV:COLOR;
//            };
			struct appdata 
			{
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
				fixed4 texcoord : TEXCOORD0;
				fixed4 tangent : TANGENT;//切线
			};
//			struct v2f
//            {
//                float4 vertex:POSITION;
//                float2 uv:TEXCOORD0;
//                float4 NdotV:COLOR;
//            };
 			struct v2f
            {
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
                float3 worldNormal : TEXCOORD1;//no use
                float3 lightDir : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 NdotV:COLOR;//todo old
                //
//                fixed4 t0 : TEXCOORD2;
//                fixed4 t1 : TEXCOORD3;
//                fixed4 t2 : TEXCOORD4;
                LIGHTING_COORDS(4,5) //影响到:  TRANSFER_VERTEX_TO_FRAGMENT(o); //添加上会有投影照到人物自身
            };
            sampler2D _MainTex;
            sampler2D _Bump;
            float4 _RimColor;
            float _RimPower;
            float _LightDir;////////
            //==
            float _Specular;
            float _Gloss;
//            //==
            samplerCUBE _Cubemap;
            float _ReflAmount;


            float4 _MainTex_ST;
//            v2f vert(appdata_base v)
			v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); //这里要特别说明一下UnityObjectToClipPos（v.vertex)） 方法，官方网站上说明，在写Instanced Shader时，通常情况下并不用在意顶点空间转换，因为所有内建的矩阵名字在Instanced Shader中都是被重定义过的，如果直接使用UNITY_MATRIX_MVP，会引入一个额外的矩阵乘法运算，所以推荐使用UnityObjectToClipPos / UnityObjectToViewPos函数，它们会把这一次额外的矩阵乘法优化为向量-矩阵乘法。
 				//Transform the vertex to projection space  
//				o.vertex = UnityObjectToClipPos(v.vertex);//相等于 mul(UNITY_MATRIX_MVP, v.vertex);  
				//Get the UV coordinates  
//                o.uv = v.texcoord;
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);

				//Create a rotation matrix for tangent space  
//				TANGENT_SPACE_ROTATION;
				/////
                float3 V = WorldSpaceViewDir(v.vertex);//_WorldSpaceCameraPos.xyz 相机矩阵
                V = mul(unity_WorldToObject,V);//视方向从世界到模型坐标系的转换
                o.NdotV.x = saturate(dot(v.normal,normalize(V)));//必须在同一坐标系才能正确做点乘运算 两个物体坐标系里的，法向量点积
                /////
                //==========TODO new
//                	o.worldNormal = mul(SCALED_NORMAL, (float3x3)unity_WorldToObject);//同等下面
				o.worldNormal = mul(v.normal,unity_WorldToObject);//不可前后互换
//                o.lightDir = mul((float3x3)unity_ObjectToWorld,WorldSpaceLightDir(v.vertex));
				o.lightDir = mul(unity_ObjectToWorld,ObjSpaceLightDir(v.vertex));
//				o.lightDir = mul((float3x3)unity_ObjectToWorld,ObjSpaceLightDir(v.vertex));
//				  o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));
                o.viewDir = mul((float3x3)unity_ObjectToWorld,ObjSpaceViewDir(v.vertex));
//				v.vertex.w = _LightDir;
				float3 V2 = WorldSpaceLightDir(v.vertex);
                V2 = mul(unity_WorldToObject,V);//视方向从世界到模型坐标系的转换
                o.NdotV.y = saturate(dot(v.normal,normalize(V2)));

//                float3 worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
//                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
//                fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
//                fixed3 worldBinormal = cross(worldNormal,worldTangent)*v.tangent.w;
//
//                // Case 1: The codes used by built-in shaders  
//                o.t0 = float4(worldTangent.x,worldBinormal.x,worldNormal.x,worldPos.x);
//                o.t1 = float4(worldTangent.y,worldBinormal.y,worldNormal.y,worldPos.y);
//                o.t2 = float4(worldTangent.z,worldBinormal.z,worldNormal.z,worldPos.z);
//                 // Case 2: The codes which I think are correct   
//                 float3x3 ot = mul(rotation,(float3x3)unity_WorldToObject);
//                 o.t0 = float4(ot[0].xyz,worldPos.x);
//                 o.t1 = float4(ot[1].xyz,worldPos.y);
//                 o.t2 = float4(ot[2].xyz,worldPos.z);
//                // pass lighting information to pixel shader  
                TRANSFER_VERTEX_TO_FRAGMENT(o); //添加上会有投影照到人物自身
                return o;
            }
 
            half4 frag(v2f IN):COLOR
            {
                float4 texColor = tex2D(_MainTex,IN.uv);
				//法线
//				fixed3 norm = UnpackNormal(tex2D(_Bump,IN.uv));
                //用视方向和法线方向做点乘，越边缘的地方，法线和视方向越接近90度，点乘越接近0.
                //用（1- 上面点乘的结果）*颜色，来反映边缘颜色情况
//                c.rgb += pow((1-IN.NdotV.x) ,_RimPower)* _RimColor.rgb ;
//                c.rgb += pow((IN.NdotV.x) ,_RimPower)* _RimColor.rgb ;
//				 float3 cc = pow((1-IN.NdotV.x) ,_RimPower)* _RimColor.rgb ;
				
				/////
				fixed4 nor = tex2D(_Bump,IN.uv);
				fixed3 norm ;
				norm.xy = nor.wy *2 -1;
				norm.z = sqrt(1- saturate(dot(norm.xy,norm.xy)));
				//////
//				fixed3 norm = UnpackNormal(tex2D(_Bump,IN.uv));

				 float3 rim = pow(saturate(IN.NdotV.x) ,_RimPower)* _RimColor.rgb ;
				 float3 rim2 = pow(saturate(1-IN.NdotV.x) ,_RimPower)* _RimColor.rgb ;
				 float3 rim3 = pow(saturate(1-IN.NdotV.y) ,_RimPower)* _RimColor.rgb ;
//				 texColor.rgb += saturate(dot(rim * .8, nor  )) + saturate(dot( rim2 , nor ))*1.2;// + saturate(dot( rim3*.5 , nor )) ;//+  lightDir;
//				 fixed3 lightDir = normalize(UnityWorldSpaceLightDir(IN.lightDir));
				 /////
//				 float3 worldPos = float3(IN.t0.w,IN.t1.w,IN.t2.w);
//				 fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
//				 fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
//				 //Case 1 
//				 half3 worldNormal = normalize(half3(dot(IN.t0.xyz,norm),dot(IN.t1.xyz,norm),dot(IN.t2.xyz,norm)));
//				 //Case 2
//				 worldNormal = normalize(mul(norm,float3x3(IN.t0.xyz,IN.t1.xyz,IN.t2.xyz)));



                //====== based on the ambient light
                float3 ambi = UNITY_LIGHTMODEL_AMBIENT.rgb*.5;
                //work out this distance of the light
                float atten = LIGHT_ATTENUATION(IN);
                //angle to the light
//                float3 diff = _LightColor0.rgb * saturate(dot(normalize(IN.worldNormal), normalize(IN.lightDir)))*1.6;
//				float3 diff = _LightColor0.rgb * saturate(dot(normalize(worldNormal), normalize(lightDir)))*1.6;
				float3 diff = _LightColor0.rgb * saturate(dot(normalize(IN.worldNormal), normalize(IN.lightDir)));
                float3 refl = reflect(-IN.lightDir, IN.worldNormal);//old
//				float3 refl = reflect(-IN.lightDir, norm );
//				float3 refl = reflect(-IN.lightDir, worldNormal );
                float3 spec = _LightColor0.rgb * pow(saturate(dot(normalize(refl),normalize(IN.viewDir))),_Specular) * _Gloss;
//				float3 spec = _LightColor0.rgb * pow(saturate(dot(normalize(refl),normalize(worldViewDir))),_Specular) * _Gloss;
                //product the final color

//                float3 worldView = fixed3(IN.t0.w,IN.t1.w,IN.t2.w);
//                float3 worldRefl = reflect(-worldViewDir, worldNormal);
//					float3 worldRefl = reflect(-worldViewDir, IN.worldNormal);
//                fixed3 reflCol = texCUBE(_Cubemap,worldRefl).rgb* _ReflAmount;
 				fixed3 reflCol = texCUBE(_Cubemap,refl).rgb* _ReflAmount;
                float4 col;
//                col.rgb = float3((ambi + (diff + spec) * atten) * texColor) + rim + reflCol;
//				col.rgb = float3((ambi + (diff + spec) * atten) * texColor) + rim ;//+ reflCol;
//				col.rgb = float3((ambi + (diff + spec) * atten) * texColor) + rim + reflCol;
				float3 nnorm = saturate(dot(rim3 , norm  ));
				col.rgb = float3((ambi + (diff + spec) * atten) * texColor)+texColor*.5 + nnorm ;//+ nnorm;// + reflCol;
                col.a = 1.0f;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}