Shader "CRT/Density"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "CRT_Density"
            
            CGPROGRAM
            // UnityCustomRenderTexture.cgincをインクルードする
           #include "UnityCustomRenderTexture.cginc"

            // 頂点シェーダは決まったものを使う
           #pragma vertex CustomRenderTextureVertexShader
           #pragma fragment frag

            float sdSphere( float3 p, float s )
            {
                return length(p)-s;
            }
            
            // v2f構造体は決まったものを使う
            half4 frag(v2f_customrendertexture i) : SV_Target
            {
                //float r = 1-sdSphere(i.globalTexcoord - 0.5, -(sin(_Time.y * 10) * 0.5 + 0.5) * 0.5);
                float r = sdSphere(i.globalTexcoord - 0.5, (sin(_Time.y * 2) * 0.5 + 0.5) * 0.25);
                return r;
            }
            ENDCG
        }
    }
}
