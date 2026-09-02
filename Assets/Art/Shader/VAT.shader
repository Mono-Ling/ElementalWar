Shader "Unlit/VAT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _VAT;
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _FrameCount)
            UNITY_DEFINE_INSTANCED_PROP(float, _VertexCount)
            UNITY_DEFINE_INSTANCED_PROP(float, _FrameRate)
            UNITY_DEFINE_INSTANCED_PROP(float3, _MinPos)
            UNITY_DEFINE_INSTANCED_PROP(float3, _MaxPos)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v, uint vid : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float vertexCount = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexCount);
                float frameCount = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameCount);
                float frameRate = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameRate);
                float3 minPos = UNITY_ACCESS_INSTANCED_PROP(Props, _MinPos);
                float3 maxPos = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxPos);

                // 半 texel 偏移：Point 采样落在 texel 边界时浮点舍入可能取错列/错帧
                float x = (vid + 0.5) / vertexCount;
                // 先按周期取模再换算帧号，避免长时间运行后 _Time.y*frameRate 的大数精度损失
                float frame = fmod(_Time.y, frameCount / frameRate) * frameRate;
                float y = (floor(frame) + 0.5) / frameCount;
                float3 pos = tex2Dlod(_VAT, float4(x, y, 0, 0)).rgb;

                pos = pos * (maxPos - minPos) + minPos;
                float4 vertex = float4(pos, v.vertex.w);
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
