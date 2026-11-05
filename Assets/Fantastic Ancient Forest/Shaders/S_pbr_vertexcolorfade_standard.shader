// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TidalFlask/PBR Vertex Color Fade Standard"
{
	Properties
	{
		_BaseTexture("Base Texture", 2D) = "white" {}
		_BaseTexColorTint("Base Tex Color Tint", Color) = (1,1,1,0)
		_PBRTexture("PBR Texture", 2D) = "white" {}
		_NormalTexture("Normal Texture", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Range( 0 , 5)) = 1
		_RoughnessMin("Roughness Min", Range( 0 , 1)) = 0.5
		_RoughnessMax("Roughness Max", Range( 0 , 1)) = 1
		_VertexMaskContrast("Vertex Mask Contrast", Range( 1 , 5)) = 1
		_BaseTexFadeColor("Base Tex Fade Color", Color) = (1,0,0,0)
		_RoughnessFadeValue("Roughness Fade Value", Range( 0 , 1)) = 0.8
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _NormalTexture;
		uniform float4 _NormalTexture_ST;
		uniform float _NormalStrength;
		uniform float _VertexMaskContrast;
		uniform float4 _BaseTexColorTint;
		uniform sampler2D _BaseTexture;
		uniform float4 _BaseTexture_ST;
		uniform float4 _BaseTexFadeColor;
		uniform sampler2D _PBRTexture;
		uniform float4 _PBRTexture_ST;
		uniform float _RoughnessFadeValue;
		uniform float _RoughnessMin;
		uniform float _RoughnessMax;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalTexture = i.uv_texcoord * _NormalTexture_ST.xy + _NormalTexture_ST.zw;
			float4 temp_cast_0 = (_VertexMaskContrast).xxxx;
			float4 VertexColorMask26 = saturate( pow( i.vertexColor , temp_cast_0 ) );
			float3 lerpResult38 = lerp( UnpackScaleNormal( tex2D( _NormalTexture, uv_NormalTexture ), _NormalStrength ) , float3(0,0,1) , VertexColorMask26.rgb);
			o.Normal = lerpResult38;
			float2 uv_BaseTexture = i.uv_texcoord * _BaseTexture_ST.xy + _BaseTexture_ST.zw;
			float4 lerpResult24 = lerp( ( _BaseTexColorTint * tex2D( _BaseTexture, uv_BaseTexture ) ) , _BaseTexFadeColor , VertexColorMask26);
			o.Albedo = lerpResult24.rgb;
			float2 uv_PBRTexture = i.uv_texcoord * _PBRTexture_ST.xy + _PBRTexture_ST.zw;
			float4 tex2DNode3 = tex2D( _PBRTexture, uv_PBRTexture );
			o.Metallic = tex2DNode3.r;
			float lerpResult28 = lerp( tex2DNode3.g , _RoughnessFadeValue , VertexColorMask26.r);
			float clampResult18 = clamp( lerpResult28 , _RoughnessMin , _RoughnessMax );
			o.Smoothness = ( 1.0 - clampResult18 );
			float lerpResult29 = lerp( tex2DNode3.b , 1.0 , VertexColorMask26.r);
			o.Occlusion = lerpResult29;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;60;-1266.333,367.5232;Inherit;False;1248.528;460.1655;normal;4;38;39;4;62;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;56;-2308.471,-123.9684;Inherit;False;1039.374;343.7828;vertex color mask;5;45;43;26;22;44;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;55;-958.0569,-233.2048;Inherit;False;870.416;467.1959;base color;5;1;20;21;25;24;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;54;-653.4368,1316.385;Inherit;False;937.7052;352.3607;roughness;6;16;17;28;33;18;35;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;53;-587.9376,928.4709;Inherit;False;513.3313;311.8745;AO;3;29;34;31;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;27;-573.2468,258.5597;Inherit;False;26;VertexColorMask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-908.0569,6.991044;Inherit;True;Property;_BaseTexture;Base Texture;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-839.2673,-180.7335;Inherit;False;Property;_BaseTexColorTint;Base Tex Color Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-569.283,7.896793;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;45;-1728.471,82.81439;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;43;-1921.323,82.56952;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;26;-1511.097,83.52554;Inherit;False;VertexColorMask;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;22;-2163.043,-73.96838;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;44;-2258.471,106.8144;Inherit;False;Property;_VertexMaskContrast;Vertex Mask Contrast;7;0;Create;True;0;0;0;False;0;False;1;1;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;24;-265.6409,7.499761;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;38;-278.8059,417.5232;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1216.333,468.651;Inherit;False;Property;_NormalStrength;Normal Strength;4;0;Create;True;0;0;0;False;0;False;1;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-600.3807,1474.561;Inherit;False;Property;_RoughnessMin;Roughness Min;5;0;Create;True;0;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-603.4368,1554.562;Inherit;False;Property;_RoughnessMax;Roughness Max;6;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;28;-259.6136,1366.385;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;18;-76.13496,1507.745;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;35;106.2683,1507.077;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;29;-252.6063,1007.58;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-912.2146,933.973;Inherit;True;Property;_PBRTexture;PBR Texture;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-895.7355,420.2379;Inherit;True;Property;_NormalTexture;Normal Texture;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;62;-795.8005,638.9426;Inherit;False;Constant;_Vector0;Vector 0;10;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;31;-537.9376,1127.347;Inherit;False;26;VertexColorMask;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-466.4496,980.7717;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-597.7147,1389.548;Inherit;False;Property;_RoughnessFadeValue;Roughness Fade Value;9;0;Create;True;0;0;0;False;0;False;0.8;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;25;-559.7875,-183.2048;Inherit;False;Property;_BaseTexFadeColor;Base Tex Fade Color;8;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;63;426.7993,8.760297;Float;False;True;-1;2;;0;0;Standard;TidalFlask/PBR Vertex Color Fade Standard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;21;0;20;0
WireConnection;21;1;1;0
WireConnection;45;0;43;0
WireConnection;43;0;22;0
WireConnection;43;1;44;0
WireConnection;26;0;45;0
WireConnection;24;0;21;0
WireConnection;24;1;25;0
WireConnection;24;2;27;0
WireConnection;38;0;4;0
WireConnection;38;1;62;0
WireConnection;38;2;27;0
WireConnection;28;0;3;2
WireConnection;28;1;33;0
WireConnection;28;2;31;0
WireConnection;18;0;28;0
WireConnection;18;1;16;0
WireConnection;18;2;17;0
WireConnection;35;0;18;0
WireConnection;29;0;3;3
WireConnection;29;1;34;0
WireConnection;29;2;31;0
WireConnection;4;5;39;0
WireConnection;63;0;24;0
WireConnection;63;1;38;0
WireConnection;63;3;3;1
WireConnection;63;4;35;0
WireConnection;63;5;29;0
ASEEND*/
//CHKSM=3E177BFAEA203EDA2F54B5B9AB50353D177151B3