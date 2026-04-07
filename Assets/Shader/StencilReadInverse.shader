Shader "UI/StencilReadInverse"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (0,0,0,0.5)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Stencil
        {
            Ref 1
            Comp NotEqual // Chỉ vẽ ở những nơi KHÔNG có giá trị 1
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            SetTexture [_MainTex] { constantColor [_Color] Combine texture * constant }
        }
    }
}