Shader "Unlit/VAT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [NonModifiableTextureData]
        _VAT ("VAT", 2D) = "white" {}
        
        [PerRendererData]_FrameCount("Frame Count", Float) = 100
        [PerRendererData]_VertexCount("Vertex Count", Float) = 100
        [PerRendererData]_FrameRate("Frame Rate", Float) = 3
        [PerRendererData]_MinPos("Min Pos", Vector) = (-1,-1,-1,-1)
        [PerRendererData]_MaxPos("Max Pos", Vector) = (1,1,1,1)
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

            float _FrameCount;
            float _VertexCount;
            float _FrameRate;
            float3 _MinPos;
            float3 _MaxPos;

            sampler2D _VAT;
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _TimeOffset)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v, uint vid : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float timeOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeOffset);

                uint width = min(2048, _VertexCount);
                float rowFrame = ceil((float)_VertexCount / width);
                float height = rowFrame * _FrameCount;

                // 半 texel 偏移：Point 采样落在 texel 边界时浮点舍入可能取错列/错帧
                float x = (vid % width + 0.5) / width;
                // 先按周期取模再换算帧号，避免长时间运行后 _Time.y*frameRate 的大数精度损失
                float frame = fmod(_Time.y + timeOffset, _FrameCount / _FrameRate) * _FrameRate;
                
                float row = floor(frame) * rowFrame + floor(vid / width);
                float y = (row + 0.5) / height;
                float3 pos = tex2Dlod(_VAT, float4(x, y, 0, 0)).rgb;

                pos = pos * (_MaxPos - _MinPos) + _MinPos;
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
