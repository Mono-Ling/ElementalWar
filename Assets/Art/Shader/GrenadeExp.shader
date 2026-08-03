Shader "Unlit/GrenadeExp"
{
    Properties
    {
        _Color ("Color",Color) = (1,1,1,1)
        _Progress ("Progress",Range(0,1)) = 1
        _Alpha ("Alpha",Range(0,1)) = 0.5
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
            };

            struct v2f
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // fixed4 _Color;
            // float _Progress;
            // float3 _CenterPoint;
            float _Alpha;
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4,_Color)
            UNITY_DEFINE_INSTANCED_PROP(float,_Progress)
            UNITY_DEFINE_INSTANCED_PROP(float3,_CenterPoint)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                float3 centerPoint = UNITY_ACCESS_INSTANCED_PROP(Props, _CenterPoint);
                float progress = UNITY_ACCESS_INSTANCED_PROP(Props, _Progress);

                float3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 dir = wPos - centerPoint;
                float len = length(dir);
                dir = normalize(dir);
                len = lerp(0, len, progress);
                wPos = centerPoint + dir * len;
                o.vertex = mul(UNITY_MATRIX_VP,float4(wPos,1));

                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                return fixed4(color.rgb, _Alpha);
            }
            ENDCG
        }
    }
}
