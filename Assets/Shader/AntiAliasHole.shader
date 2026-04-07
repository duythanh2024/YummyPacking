Shader "UI/AntiAliasHole"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (0,0,0,0.7) // Chỉnh độ tối ở đây
        _HolePosition ("Hole Position (Screen Space)", Vector) = (0,0,0,0)
        _HoleRadius ("Hole Radius (Pixels)", Float) = 100
        _Softness ("Edge Softness (Pixels)", Float) = 2 // Độ mịn cạnh
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 texcoord : TEXCOORD0; fixed4 color : COLOR; float4 screenPos : TEXCOORD1; };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _HolePosition;
            float _HoleRadius;
            float _Softness;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                // Lấy tọa độ màn hình thực tế
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. Chuyển tọa độ màn hình về Pixel
                float2 screenCoord = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
                
                // 2. Tính khoảng cách từ Pixel hiện tại đến tâm lỗ
                float dist = distance(screenCoord, _HolePosition.xy);

                // 3. Tính toán độ Alpha bằng smoothstep để làm mịn cạnh
                // Trong vùng HoleRadius, alpha = 0 (trong suốt)
                // Trong vùng HoleRadius + Softness, alpha chuyển từ 0 sang _Color.a
                float alphaMask = smoothstep(_HoleRadius, _HoleRadius + _Softness, dist);

                // Lấy màu của Panel và áp dụng Alpha Mask
                fixed4 col = _Color;
                col.a *= alphaMask;

                return col;
            }
            ENDCG
        }
    }
}