
Shader "GetDirty/GetDirty with Bump + Spec Type 3"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DirtMap("Dirt (Normalmap)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_Wetness("Wetness", Range(0.00, 1)) = 0.078125
		_Dirtiness("Dirtiness", Range(0, 1)) = 0.0
		_BumpHeight("Bump Height", Range(0, 1)) = 1.0
		_Transparency("Transparency", Range(0, 1)) = 0.0
		_DirtColor("Dirt Color", Color) = (0.39,0.23,0.0,0.0)
	}

		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			//Cull Off
			LOD 250

			CGPROGRAM
			#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd novertexlights halfasview

			sampler2D _MainTex;
			sampler2D _DirtMap;
			sampler2D _BumpMap;
			fixed _Dirtiness;
			fixed _BumpHeight;
			fixed _Transparency;
			fixed3 _DirtColor;
			fixed _Wetness;

			inline fixed4 LightingMobileBlinnPhong(SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
			{
				fixed diff = max(0, dot(s.Normal, lightDir));
				fixed nh = max(0, dot(s.Normal, halfDir));
				fixed spec = pow(nh, s.Specular * 128) * s.Gloss;

				fixed4 c;
				c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
				c.a = 0.0;
				return c;
			}

			struct Input
			{
				fixed2 uv_MainTex;
				fixed2 uv_BumpMap;
				fixed2 uv_DirtMap;
			};

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 diffuse = tex2D(_MainTex, IN.uv_MainTex);
				fixed4 dirtBase = tex2D(_DirtMap, IN.uv_MainTex);

				if (dirtBase.a > _Dirtiness)
				{
					o.Albedo = diffuse.rgb;
					o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
					o.Gloss = .1;//diffuse.a;
					o.Specular = .1;//diffuse.a;
				}
				else
				{
					o.Albedo = lerp(_DirtColor, diffuse, _Transparency);
					fixed4 bumpCol = tex2D(_BumpMap, IN.uv_MainTex);
					fixed4 outputBump = lerp(bumpCol, dirtBase, _BumpHeight);//max(bumpCol, dirtBase * _BumpHeight);
					o.Normal = UnpackNormal(outputBump);
					o.Gloss = _Wetness;
					o.Specular = _Wetness;
				}
			}
			ENDCG
		}

			FallBack "Mobile/VertexLit"
}