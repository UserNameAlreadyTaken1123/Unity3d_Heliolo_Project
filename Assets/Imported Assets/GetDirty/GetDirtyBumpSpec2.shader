
Shader "Mobile/GetDirty with Bump + Spec Type 2" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (Dirt)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}	
		_Wetness ("Wetness", Range (0.03, 1)) = 0.078125
		_Amount ("Amount", Range(0, 0.8)) = 0.0
		_DirtColor ("Dirt Color", Color) = (0.39,0.23,0.0,0.0)
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		//Cull Off
		LOD 250

		CGPROGRAM
		#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview novertexlights

		inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
		{
			fixed diff = max (0, dot (s.Normal, lightDir));
			fixed nh = max (0, dot (s.Normal, halfDir));
			fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
			fixed4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten*2);
			c.a = 0.0;
			return c;
		}

		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _Amount;
		float4 _DirtColor;
		half _Wetness;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			fixed dirtBase = c.a;
			fixed4 dirt = fixed4(_DirtColor.r-dirtBase, _DirtColor.g-dirtBase, _DirtColor.b-dirtBase, 1 - dirtBase) * _Amount;
			
			if (dirtBase+.25 > _Amount)
			{
				o.Albedo = c.rgb;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
				o.Gloss = _Wetness;
			}
			else
			{
				o.Albedo = lerp(c.rgb, dirt.rgb, _Amount);
				fixed4 bumpCol = tex2D(_BumpMap, IN.uv_MainTex);
				dirt = fixed4(dirt.a, dirt.a, dirt.a, dirt.a);
				fixed4 outputBump = max(bumpCol, dirt);
				o.Normal = UnpackNormal(outputBump);			
				o.Gloss = _Wetness;
			}
			o.Specular = _Wetness;
		}
		ENDCG  
	}

	FallBack "Mobile/Diffuse"
}