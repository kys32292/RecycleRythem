// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TidalFlask/Foliage Wind Standard Advanced Lit Emission"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset]_BaseTexture("Base Texture", 2D) = "white" {}
		_BaseTexColorTint("Base Tex Color Tint", Color) = (1,1,1,0)
		_Roughness("Roughness", Range( 0 , 1)) = 0.8
		[Toggle(_USEVERTEXCOLORFORWIND_ON)] _Usevertexcolorforwind("Use vertex color for wind", Float) = 0
		_WindStrength("Wind Strength", Range( 0 , 1)) = 0.4
		_WindScale("Wind Scale", Range( 0 , 1)) = 0.4
		_WindSpeed("Wind Speed", Vector) = (2,1,0,0)
		_WindTintOpacity("Wind Tint Opacity", Range( 0 , 1)) = 0.25
		_WindTintContrast("Wind Tint Contrast", Range( 1 , 5)) = 1
		_WindTintScale("Wind Tint Scale", Range( 0 , 0.5)) = 0.12
		_WindTintSpeed("Wind Tint Speed", Vector) = (-3,1,0,0)
		_DistanceFadeColor("Distance Fade Color", Color) = (0.5333334,0.6745098,0.1607843,1)
		_DistanceFadeStart("Distance Fade Start", Float) = 20
		_DistanceFadeEnd("Distance Fade End", Float) = 50
		_DistanceFadeOpacity("Distance Fade Opacity", Range( 0 , 1)) = 0.8
		[Toggle(_USEEMISSIONTEXTURE_ON)] _UseEmissionTexture("Use Emission Texture", Float) = 0
		[NoScaleOffset]_EmissionTexture("Emission Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEVERTEXCOLORFORWIND_ON
		#pragma shader_feature_local _USEEMISSIONTEXTURE_ON
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			half ASEIsFrontFacing : VFACE;
			float2 uv_texcoord;
			float eyeDepth;
		};

		uniform float2 _WindSpeed;
		uniform float _WindScale;
		uniform float _WindStrength;
		uniform float4 _BaseTexColorTint;
		uniform sampler2D _BaseTexture;
		uniform float2 _WindTintSpeed;
		uniform float _WindTintScale;
		uniform float _WindTintOpacity;
		uniform float _WindTintContrast;
		uniform float4 _DistanceFadeColor;
		uniform float _DistanceFadeOpacity;
		uniform float _DistanceFadeEnd;
		uniform float _DistanceFadeStart;
		uniform sampler2D _EmissionTexture;
		uniform float _Roughness;
		uniform float _Cutoff = 0.5;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 break219 = ase_worldPos;
			float4 appendResult221 = (float4(break219.x , break219.z , 0.0 , 0.0));
			float simplePerlin2D38 = snoise( (( appendResult221 / 2 )*1.0 + float4( ( _Time.y * _WindSpeed ), 0.0 , 0.0 )).xy*_WindScale );
			simplePerlin2D38 = simplePerlin2D38*0.5 + 0.5;
			float3 break49 = ase_worldPos;
			float4 appendResult51 = (float4(( ( ( simplePerlin2D38 - 0.5 ) * _WindStrength ) + break49.x ) , break49.y , break49.z , 0.0));
			float4 temp_cast_3 = (v.texcoord.xy.y).xxxx;
			#ifdef _USEVERTEXCOLORFORWIND_ON
				float4 staticSwitch454 = v.color;
			#else
				float4 staticSwitch454 = temp_cast_3;
			#endif
			float4 lerpResult3 = lerp( float4( ase_worldPos , 0.0 ) , appendResult51 , staticSwitch454);
			float3 worldToObj1 = mul( unity_WorldToObject, float4( lerpResult3.xyz, 1 ) ).xyz;
			float3 VertexPosition77 = worldToObj1;
			v.vertex.xyz = VertexPosition77;
			v.vertex.w = 1;
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 switchResult428 = (((i.ASEIsFrontFacing>0)?(float3(0,0,1)):(float3(0,0,-1))));
			o.Normal = switchResult428;
			float2 uv_BaseTexture4 = i.uv_texcoord;
			float4 tex2DNode4 = tex2D( _BaseTexture, uv_BaseTexture4 );
			float4 BaseTexture423 = ( _BaseTexColorTint * tex2DNode4 );
			float3 ase_worldPos = i.worldPos;
			float3 break290 = ase_worldPos;
			float4 appendResult291 = (float4(break290.x , break290.z , 0.0 , 0.0));
			float2 _Vector0 = float2(2,0);
			float simplePerlin2D298 = snoise( (( appendResult291 / _Vector0.x )*1.0 + float4( ( _Time.y * ( _WindTintSpeed * 0.2 ) ), 0.0 , 0.0 )).xy*( _WindTintScale * 1.2 ) );
			simplePerlin2D298 = simplePerlin2D298*0.5 + 0.5;
			float3 break254 = ase_worldPos;
			float4 appendResult257 = (float4(break254.x , break254.z , 0.0 , 0.0));
			float simplePerlin2D263 = snoise( (( appendResult257 / _Vector0.x )*1.0 + float4( ( _Time.y * _WindTintSpeed ), 0.0 , 0.0 )).xy*_WindTintScale );
			simplePerlin2D263 = simplePerlin2D263*0.5 + 0.5;
			float clampResult300 = clamp( ( simplePerlin2D298 + simplePerlin2D263 ) , 0.0 , 1.0 );
			float lerpResult242 = lerp( 1.0 , clampResult300 , _WindTintOpacity);
			float clampResult338 = clamp( pow( lerpResult242 , _WindTintContrast ) , 0.0 , 1.0 );
			float4 CustomLightingWindTint173 = ( BaseTexture423 + ( 1.0 - clampResult338 ) );
			float cameraDepthFade83 = (( i.eyeDepth -_ProjectionParams.y - _DistanceFadeStart ) / _DistanceFadeEnd);
			float4 lerpResult84 = lerp( CustomLightingWindTint173 , _DistanceFadeColor , ( _DistanceFadeOpacity * saturate( cameraDepthFade83 ) ));
			float4 WithFade169 = lerpResult84;
			o.Albedo = WithFade169.rgb;
			float4 color466 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
			float2 uv_EmissionTexture465 = i.uv_texcoord;
			#ifdef _USEEMISSIONTEXTURE_ON
				float4 staticSwitch467 = tex2D( _EmissionTexture, uv_EmissionTexture465 );
			#else
				float4 staticSwitch467 = color466;
			#endif
			o.Emission = staticSwitch467.rgb;
			o.Metallic = 0.0;
			o.Smoothness = ( 1.0 - _Roughness );
			o.Alpha = 1;
			clip( tex2DNode4.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;79;-4987.675,83.78412;Inherit;False;3987.664;1284.837;;25;77;1;3;54;51;48;49;41;42;50;39;38;46;35;34;225;222;33;44;218;452;453;454;219;221;wind movement;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;218;-4866.7,216.851;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;44;-4218.478,603.6404;Inherit;False;Property;_WindSpeed;Wind Speed;7;0;Create;True;0;0;0;False;0;False;2,1;3,1.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;33;-4214.547,489.2257;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;225;-4173.11,212.8831;Inherit;True;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-4000.519,487.7636;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;35;-3833.183,212.6472;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-3791.021,487.5747;Inherit;False;Property;_WindScale;Wind Scale;6;0;Create;True;0;0;0;False;0;False;0.4;0.158;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;38;-3570.536,205.2992;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;39;-3273.199,212.6231;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;50;-3266.578,700.8773;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;42;-3274.06,490.4867;Inherit;False;Property;_WindStrength;Wind Strength;5;0;Create;True;0;0;0;False;0;False;0.4;0.346;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-3003.392,212.6301;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;49;-2994.414,697.8253;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;48;-2743.577,674.5073;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;51;-2582.717,698.3052;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;54;-2343.426,485.3057;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;3;-2057.604,675.2893;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TransformPositionNode;1;-1727.729,670.7514;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;91;-817.6693,1634.418;Inherit;False;2159.169;567.1646;;10;169;84;109;85;108;89;83;86;87;469;distance fade to color;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;307;-817.6674,90.59742;Inherit;False;3939.225;1352.215;2 similar noises, multiplied values to make them move differently, add to combine them, so the overall noise is not constant;34;338;336;173;337;242;243;300;157;299;263;298;296;301;262;259;260;295;261;294;302;291;257;258;306;256;305;254;255;290;253;289;342;343;424;wind movement tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;296;272.7658,234.6968;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;262;249.955,757.0774;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;84;644.0165,1828.079;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;256;-283.1165,968.5173;Inherit;False;Constant;_Vector0;Vector 0;0;0;Create;True;0;0;0;False;0;False;2,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;342;2526.963,706.3345;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;380.5717,2046.817;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-762.3336,2000.935;Inherit;False;Property;_DistanceFadeEnd;Distance Fade End;14;0;Create;True;0;0;0;False;0;False;50;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;261;251.497,1070.977;Inherit;False;Property;_WindTintScale;Wind Tint Scale;10;0;Create;True;0;0;0;False;0;False;0.12;0.1;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;54.05093,2018.626;Inherit;False;Property;_DistanceFadeOpacity;Distance Fade Opacity;15;0;Create;True;0;0;0;False;0;False;0.8;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;83;-422.847,2065.672;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-764.8425,2106.917;Inherit;False;Property;_DistanceFadeStart;Distance Fade Start;13;0;Create;True;0;0;0;False;0;False;20;10.28;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;343;2015.842,731.1666;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;89;-159.748,2067.375;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;306;-257.4308,490.7122;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;302;243.694,507.2909;Inherit;False;Constant;_Float3;Float 3;14;0;Create;True;0;0;0;False;0;False;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;290;-521.3087,240.7295;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;257;-339.8097,756.0452;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;258;-258.9025,1151.378;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;85;302.6645,1793.656;Inherit;False;Property;_DistanceFadeColor;Distance Fade Color;12;0;Create;True;0;0;0;False;0;False;0.5333334,0.6745098,0.1607843,1;0.5333334,0.6745098,0.1607843,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;157;935.1089,986.4713;Inherit;False;Property;_WindTintOpacity;Wind Tint Opacity;8;0;Create;True;0;0;0;False;0;False;0.25;0.603;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;300;1091.83,548.795;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;243;1094.326,724.3115;Inherit;False;Constant;_Float2;Float 2;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;255;-524.2955,1214.442;Inherit;False;Property;_WindTintSpeed;Wind Tint Speed;11;0;Create;True;0;0;0;False;0;False;-3,1;-3,1.6;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WorldPosInputsNode;253;-763.2582,753.0854;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;289;-760.7602,238.9007;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;294;-67.16479,234.9326;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;263;567.4981,750.5265;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;298;569.8321,230.3949;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;242;1300.584,729.4043;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;260;-62.31665,756.4098;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PowerNode;336;1581.781,736.7247;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;337;1320.778,990.5725;Inherit;False;Property;_WindTintContrast;Wind Tint Contrast;9;0;Create;True;0;0;0;False;0;False;1;1;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;291;-351.1649,234.9326;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;259;-54.27063,1197.472;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;295;-61.60278,490.6332;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;254;-523.8096,754.9143;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;305;-454.4096,508.8407;Inherit;False;Constant;_Float4;Float 4;14;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;301;440.6732,489.1624;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;338;1797.836,735.6466;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;299;876.1262,548.8877;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;452;-2724.551,1169.166;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;453;-2757.286,889.2594;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;454;-2473.174,878.8495;Inherit;False;Property;_Usevertexcolorforwind;Use vertex color for wind;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;424;2276.728,599.2063;Inherit;False;423;BaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;97;3980.543,109.9606;Inherit;False;Property;_BaseTexColorTint;Base Tex Color Tint;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;4258.405,274.9615;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;423;4467.35,270.6027;Inherit;False;BaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-1370.391,671.4094;Inherit;False;VertexPosition;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;169;1000.08,1824.628;Inherit;False;WithFade;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;419;4756.685,266.5669;Inherit;False;169;WithFade;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;466;3921.076,1085.389;Inherit;False;Constant;_Color0;Color 0;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;467;4238.595,1085.389;Inherit;False;Property;_UseEmissionTexture;Use Emission Texture;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;465;3918.647,1256.394;Inherit;True;Property;_EmissionTexture;Emission Texture;17;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;3895.286,293.8146;Inherit;True;Property;_BaseTexture;Base Texture;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;219;-4647.45,214.9486;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;221;-4477.306,209.1517;Inherit;True;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;222;-4422.718,435.7119;Inherit;False;Constant;_Vector1;Vector 1;0;0;Create;True;0;0;0;False;0;False;2,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;78;4832.567,1271.828;Inherit;False;77;VertexPosition;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;472;4917.519,1016.774;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;471;4624.755,1016.545;Inherit;False;Property;_Roughness;Roughness;3;0;Create;True;0;0;0;False;0;False;0.8;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;470;4913.149,930.2125;Inherit;False;Constant;_Float0;Float 0;20;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwitchByFaceNode;428;4142.338,693.4412;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;427;3908.319,690.7542;Inherit;False;Constant;_Vector4;Vector 4;5;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;426;3910.919,849.3542;Inherit;False;Constant;_Vector3;Vector 3;5;0;Create;True;0;0;0;False;0;False;0,0,-1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;173;2800.387,708.0618;Inherit;False;CustomLightingWindTint;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;469;234.5557,1705.898;Inherit;False;173;CustomLightingWindTint;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;473;5216.432,672.202;Float;False;True;-1;2;;0;0;Standard;TidalFlask/Foliage Wind Standard Advanced Lit Emission;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Absolute;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;225;0;221;0
WireConnection;225;1;222;1
WireConnection;34;0;33;0
WireConnection;34;1;44;0
WireConnection;35;0;225;0
WireConnection;35;2;34;0
WireConnection;38;0;35;0
WireConnection;38;1;46;0
WireConnection;39;0;38;0
WireConnection;41;0;39;0
WireConnection;41;1;42;0
WireConnection;49;0;50;0
WireConnection;48;0;41;0
WireConnection;48;1;49;0
WireConnection;51;0;48;0
WireConnection;51;1;49;1
WireConnection;51;2;49;2
WireConnection;3;0;54;0
WireConnection;3;1;51;0
WireConnection;3;2;454;0
WireConnection;1;0;3;0
WireConnection;296;0;294;0
WireConnection;296;2;295;0
WireConnection;262;0;260;0
WireConnection;262;2;259;0
WireConnection;84;0;469;0
WireConnection;84;1;85;0
WireConnection;84;2;109;0
WireConnection;342;0;424;0
WireConnection;342;1;343;0
WireConnection;109;0;108;0
WireConnection;109;1;89;0
WireConnection;83;0;87;0
WireConnection;83;1;86;0
WireConnection;343;0;338;0
WireConnection;89;0;83;0
WireConnection;306;0;255;0
WireConnection;306;1;305;0
WireConnection;290;0;289;0
WireConnection;257;0;254;0
WireConnection;257;1;254;2
WireConnection;300;0;299;0
WireConnection;294;0;291;0
WireConnection;294;1;256;1
WireConnection;263;0;262;0
WireConnection;263;1;261;0
WireConnection;298;0;296;0
WireConnection;298;1;301;0
WireConnection;242;0;243;0
WireConnection;242;1;300;0
WireConnection;242;2;157;0
WireConnection;260;0;257;0
WireConnection;260;1;256;1
WireConnection;336;0;242;0
WireConnection;336;1;337;0
WireConnection;291;0;290;0
WireConnection;291;1;290;2
WireConnection;259;0;258;0
WireConnection;259;1;255;0
WireConnection;295;0;258;0
WireConnection;295;1;306;0
WireConnection;254;0;253;0
WireConnection;301;0;261;0
WireConnection;301;1;302;0
WireConnection;338;0;336;0
WireConnection;299;0;298;0
WireConnection;299;1;263;0
WireConnection;454;1;453;2
WireConnection;454;0;452;0
WireConnection;98;0;97;0
WireConnection;98;1;4;0
WireConnection;423;0;98;0
WireConnection;77;0;1;0
WireConnection;169;0;84;0
WireConnection;467;1;466;0
WireConnection;467;0;465;0
WireConnection;219;0;218;0
WireConnection;221;0;219;0
WireConnection;221;1;219;2
WireConnection;472;0;471;0
WireConnection;428;0;427;0
WireConnection;428;1;426;0
WireConnection;173;0;342;0
WireConnection;473;0;419;0
WireConnection;473;1;428;0
WireConnection;473;2;467;0
WireConnection;473;3;470;0
WireConnection;473;4;472;0
WireConnection;473;10;4;4
WireConnection;473;11;78;0
ASEEND*/
//CHKSM=E0BC25EDE8019AA4E09AC336FB157D7391C0CCB9