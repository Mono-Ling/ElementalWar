Shader "Unlit/ElementShield"
{
    Properties
    {
        _Color ("Color",Color) = (1,1,1,1)
        _Alpha ("Alpha",Range(0,1)) = 0.5
        _Edge ("Edge",Range(0,0.5)) = 0.2
        _Low ("Low",Range(0,1)) = 0.2
        _Height ("Height",Range(0,1)) = 0.8
        _ReflectZero ("Refrect Zero", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 view : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            float _Alpha;
            float _Edge;
            float _Low;
            float _Height;
            float _ReflectZero;
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4,_Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = normalize(UnityObjectToWorldDir(v.normal));
                
                float3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.view = normalize(_WorldSpaceCameraPos - wPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                clip(i.uv.y - _Low);
                clip(_Height - i.uv.y);

                float hightEdgeNum = smoothstep(_Height - _Edge, _Height, i.uv.y);
                float lowEdgeNum = smoothstep(_Low, _Low + _Edge, i.uv.y);

                float f = _ReflectZero + (1 - _ReflectZero) * pow(1 - dot(i.view, i.normal),5);

                float alpha = lerp(0, _Alpha, f);
                alpha = lerp(alpha, 0, hightEdgeNum);
                alpha = lerp(0, alpha, lowEdgeNum);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                return fixed4(color.rgb, alpha);
            }
            ENDCG
        }
    }
}