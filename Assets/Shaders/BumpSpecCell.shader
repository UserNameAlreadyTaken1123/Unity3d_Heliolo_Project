Shader "Example/Custom/Toon" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
 
      #pragma surface surf Lambert
      #pragma lighting ToonRamp exclude_path:prepass
      struct Input {
          float2 uv_MainTex;
          float3 worldRefl;
      };
      sampler2D _MainTex;
      samplerCUBE _Cube;
      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * texCUBE (_Cube, IN.worldRefl).rgb * 2;
      }
 
inline half4 LightingToonRamp(SurfaceOutput s, half atten)
    {
        half4 c;
        c.rgb = s.Albedo * _LightColor0.rgb  * (atten * 2);
        c.a = 0;
        return c;
    }
 
 
      ENDCG
    }
    Fallback "Diffuse"
  }