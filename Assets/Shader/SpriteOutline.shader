Shader "Custom/SpriteOutline"
{
   Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 15)) = 0
        // MỚI: Độ rộng khoảng trống bên ngoài (Pixel)
        _Padding ("Padding", Range(0, 20)) = 5 
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _Padding; // MỚI
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                // MỚI: Mở rộng đỉnh của Mesh ra bên ngoài dựa trên Padding
                float2 paddingOffset = _MainTex_TexelSize.xy * _Padding;
                IN.vertex.xy += (IN.texcoord - 0.5) * paddingOffset * 2.0;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // MỚI: Thu nhỏ tọa độ UV để hình ảnh nằm gọn bên trong Mesh đã mở rộng
                float2 paddingOffset = _MainTex_TexelSize.xy * _Padding;
                float2 newUV = (IN.texcoord - 0.5) * (1.0 + paddingOffset * 2.0) + 0.5;

                // Nếu UV nằm ngoài phạm vi 0-1 (vùng đệm), thì coi như alpha = 0
                if (newUV.x < 0 || newUV.x > 1 || newUV.y < 0 || newUV.y > 1) {
                    discard;
                }

                fixed4 c = tex2D(_MainTex, newUV) * IN.color;
                
                float2 p = _MainTex_TexelSize.xy * _OutlineWidth;
                
                // Thuật toán lấy mẫu 8 hướng (mượt hơn 4 hướng cũ)
                float a = tex2D(_MainTex, newUV + float2(p.x, 0)).a +
                          tex2D(_MainTex, newUV - float2(p.x, 0)).a +
                          tex2D(_MainTex, newUV + float2(0, p.y)).a +
                          tex2D(_MainTex, newUV - float2(0, p.y)).a +
                          tex2D(_MainTex, newUV + float2(p.x, p.y)).a +
                          tex2D(_MainTex, newUV - float2(p.x, p.y)).a +
                          tex2D(_MainTex, newUV + float2(-p.x, p.y)).a +
                          tex2D(_MainTex, newUV - float2(-p.x, p.y)).a;

                if (c.a < 0.1 && a > 0) {
                    return _OutlineColor;
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}