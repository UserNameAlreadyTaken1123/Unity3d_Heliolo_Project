
Shader "Mobile/GetDirty" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (Dirt)", 2D) = "white" {}
		_Amount ("Amount", Range(0, 1.0)) = 0.0
		_DirtColor ("Dirt Color", Color) = (0.39,0.23,0.0,0.0)
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		//Cull Off
		//LOD 250

		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		float _Amount;
		float4 _DirtColor;

		struct Input 
		{
			float2 uv_MainTex;
		};
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			fixed dirtBase = c.a;
			fixed4 dirt = fixed4(_DirtColor.r-dirtBase, _DirtColor.g-dirtBase, _DirtColor.b-dirtBase, 1 - dirtBase) * _Amount;
			o.Albedo = lerp(c.rgb, dirt.rgb, dirt.a);
		}
		ENDCG  
	}

	FallBack "Mobile/Diffuse"
}