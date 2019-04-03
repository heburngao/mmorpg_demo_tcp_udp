// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FlowLight"
{
    //流光shader
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StreamTex("StreamTexture", 2D) = "white" {}
        _StreamColor("StreamColor", Color) = (1,1,1,1)
        _StreamStrength("StreamStrength", Float) = 1
        _StreamSpeed("StreamSpeed", Range(0,1)) = 0.5
        _EnvTex("EnvTex (CubeMap)", Cube) = "_SkyBox" {}
        _EnvStrength("EnvStrength", Range(0,1)) = 0.8
        //====
        _WrapSpeed("WrapSpeed",float) = 1.0
        _WrapStrength("WrapStrength",float) = 1.0
        //====
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 RefDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _StreamTex;
            float4 _StreamTex_ST;
            half4 _StreamColor;
            float _StreamStrength;
            float _StreamSpeed;
            samplerCUBE _EnvTex;
            half _EnvStrength;
            //====
            half _WrapSpeed;
            half _WrapStrength;
            //====
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
                o.uv1 = v.uv1;

                float3 worldN = UnityObjectToWorldNormal(v.normal);
                o.RefDir = reflect(-WorldSpaceViewDir(v.vertex), worldN);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv0);
                //流光
                float2 streamUV = i.uv1 + _Time.y * float2(_StreamSpeed, 0);//缩放时间控制速度
				
                fixed4 steamCol = tex2D(_StreamTex, TRANSFORM_TEX(streamUV, _StreamTex));
                float3 steam = _StreamColor * steamCol.r * _StreamStrength;

                //====
                float2 wrapUV = i.uv0 + _Time.y * float2(-_WrapSpeed , _WrapSpeed);
                fixed4 wrapCol = tex2D(_StreamTex,TRANSFORM_TEX(wrapUV,_StreamTex));
                float3 wrap = _StreamColor * wrapCol.r * _WrapStrength;
                //====
                //环境贴图反射
                float3 reflection = texCUBE(_EnvTex, i.RefDir).rgb * _EnvStrength;
//                float3 o = col.rgb + steam + reflection + wrap;
				float3 o = col.rgb + steamCol.rgb*.2 + reflection + wrapCol.rgb*.2;
//				float3 o = col.rgb + steam  + wrap;
                return fixed4(o, 1);
            }
            ENDCG
        }
    }
}