// Kính màu cho ô bàn chơi, dựng lại theo tham số của material gốc MASTER_Glass_Fake_v008_HISM
// (đồ thị node đã bị cook mất, chỉ còn texture đầu vào và giá trị tham số):
//   - Mảnh kính: kênh G của T_NoiseCells_002_mask, độ sáng mỗi mảnh trong [Glass Shard Brightness Min, Max] = [0.7, 1.1].
//   - Vết nứt/vân trong mảnh: kênh B, làm tối theo Secondary Noise Darkness.
//   - Méo nhẹ đường mảnh: T_Distortion_001_mask (kênh G, B).
//   - Dải sáng dọc mép vùng: game gốc lấy từ atlas T_EdgeGradients / T_VertexGradients (16 tổ hợp cạnh, 16 tổ hợp góc,
//     giá trị = khoảng cách tới mép / nửa ô). Ở đây tính thẳng khoảng cách đó từ mặt nạ cạnh, kết quả như atlas.
// Đầu vào mỗi đỉnh: uv0 = toạ độ trên bàn (RawImage.uvRect), uv1 = toạ độ trong ô 0..1,
// uv2 = (mặt nạ cạnh, mặt nạ góc). Bit cạnh: 1 trái, 2 dưới, 4 phải, 8 trên. Bit góc: 1 dưới-phải, 2 dưới-trái, 4 trên-phải, 8 trên-trái.
Shader "Puzzle/UI/StainedGlass"
{
    Properties
    {
        [PerRendererData] _MainTex ("Glass Cells (G = mảnh, B = vân nứt)", 2D) = "white" {}
        _DistortTex ("Distortion", 2D) = "gray" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShardMin ("Shard Brightness Min", Float) = 0.7
        _ShardMax ("Shard Brightness Max", Float) = 1.1
        _CrackDarkness ("Crack Darkness", Range(0, 2)) = 0.35
        _DistortScale ("Distortion Scale", Float) = 1.5
        _DistortStrength ("Distortion Strength", Range(0, 0.1)) = 0.012
        _EdgeWidth ("Edge Glow Width (phần nửa ô)", Range(0.01, 1)) = 0.32
        _EdgeGlow ("Edge Glow Strength", Range(0, 1)) = 0.55
        _EdgeLift ("Edge Glow Whiteness", Range(0, 1)) = 0.6

        // Viền vát kiểu khối ngọc Block Blast: mép trên/trái sáng, mép dưới/phải tối, viền tối mảnh sát mép.
        // Để 0 hết thì shader chỉ là kính Glimmith.
        _BevelWidth ("Bevel Width (phần nửa ô)", Range(0.01, 1)) = 0.28
        _BevelLight ("Bevel Light", Range(0, 1)) = 0
        _BevelDark ("Bevel Dark", Range(0, 1)) = 0
        _RimWidth ("Rim Width (phần nửa ô)", Range(0, 0.3)) = 0.05
        _RimDark ("Rim Dark", Range(0, 1)) = 0
        _Saturation ("Saturation Boost", Range(0, 2)) = 1

        // Khối chuẩn của shader UI (stencil cho Mask, ColorMask, cắt theo RectMask2D).
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 board : TEXCOORD0;
                float2 local : TEXCOORD1;
                nointerpolation float2 masks : TEXCOORD2;
                float4 worldPosition : TEXCOORD3;
            };

            sampler2D _MainTex;
            sampler2D _DistortTex;
            fixed4 _Color;
            float4 _ClipRect;
            float _ShardMin, _ShardMax, _CrackDarkness, _DistortScale, _DistortStrength;
            float _EdgeWidth, _EdgeGlow, _EdgeLift;
            float _BevelWidth, _BevelLight, _BevelDark, _RimWidth, _RimDark, _Saturation;

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.board = v.uv0;
                o.local = v.uv1;
                o.masks = v.uv2;
                return o;
            }

            float HasBit(float mask, float bit)
            {
                return fmod(floor(mask / bit), 2.0);
            }

            // Khoảng cách tới mép vùng gần nhất, chia nửa ô (0 = sát mép, 1 = giữa ô hoặc không có mép), như atlas gốc.
            float EdgeDistance(float2 p, float sides, float corners)
            {
                float d = 1.0;
                if (HasBit(sides, 1)) d = min(d, p.x / 0.5);
                if (HasBit(sides, 2)) d = min(d, p.y / 0.5);
                if (HasBit(sides, 4)) d = min(d, (1.0 - p.x) / 0.5);
                if (HasBit(sides, 8)) d = min(d, (1.0 - p.y) / 0.5);
                if (HasBit(corners, 1)) d = min(d, length(p - float2(1, 0)) / 0.5);
                if (HasBit(corners, 2)) d = min(d, length(p - float2(0, 0)) / 0.5);
                if (HasBit(corners, 4)) d = min(d, length(p - float2(1, 1)) / 0.5);
                if (HasBit(corners, 8)) d = min(d, length(p - float2(0, 1)) / 0.5);
                return saturate(d);
            }

            // Khoảng cách tới mép theo từng phía (0 = sát mép, 1 = xa hoặc không có mép phía đó).
            float SideDistance(float2 p, float sides, float bitA, float distA, float bitB, float distB)
            {
                float d = 1.0;
                if (HasBit(sides, bitA)) d = min(d, distA / 0.5);
                if (HasBit(sides, bitB)) d = min(d, distB / 0.5);
                return saturate(d);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 warp = tex2D(_DistortTex, i.board * _DistortScale).gb - 0.5;
                float2 uv = i.board + warp * _DistortStrength;
                float4 cells = tex2D(_MainTex, uv);

                float shard = lerp(_ShardMin, _ShardMax, cells.g);
                float crack = 1.0 - _CrackDarkness * (1.0 - cells.b);
                float3 tint = i.color.rgb;
                float3 col = tint * shard * crack;

                float d = EdgeDistance(i.local, i.masks.x, i.masks.y);
                float band = 1.0 - smoothstep(0.0, _EdgeWidth, d);
                float3 glow = lerp(tint, float3(1, 1, 1), _EdgeLift);
                col = lerp(col, glow, band * _EdgeGlow);

                // Vát: phía trên + trái hứng sáng, phía dưới + phải đổ bóng; góc lõm tính chung vào bóng.
                float sides = i.masks.x;
                float dLight = SideDistance(i.local, sides, 8, 1.0 - i.local.y, 1, i.local.x);
                float dDark = min(SideDistance(i.local, sides, 2, i.local.y, 4, 1.0 - i.local.x),
                                  EdgeDistance(i.local, 0, i.masks.y));
                float light = 1.0 - smoothstep(0.0, _BevelWidth, dLight);
                float dark = 1.0 - smoothstep(0.0, _BevelWidth, dDark);
                col = lerp(col, lerp(tint, float3(1, 1, 1), 0.65), light * _BevelLight);
                col *= 1.0 - dark * _BevelDark;
                col *= 1.0 - (1.0 - smoothstep(0.0, _RimWidth, d)) * _RimDark;

                float grey = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(grey.xxx, col, _Saturation);

                fixed4 result = fixed4(col, i.color.a);
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
