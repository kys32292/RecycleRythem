// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TidalFlask/Water Depth Standard"
{
	Properties
	{
		_Opacity("Opacity", Range( 0 , 1)) = 0.4
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.9
		_Depth("Depth", Float) = -0.3
		_WaterColorDeep("Water Color Deep", Color) = (0.127214,0.09344961,0.7924528,0)
		_WaterColorShallow("Water Color Shallow", Color) = (0.2671324,0.8207547,0.8099126,0)
		[HDR]_FoamAndRipplesEmission("Foam And Ripples Emission", Color) = (0.509434,0.509434,0.509434,0)
		_NormalTexture("Normal Texture", 2D) = "bump" {}
		_NormalTiling("Normal Tiling", Range( 1 , 5)) = 1
		_NormalSpeed("Normal Speed", Range( 0 , 1)) = 0.1
		_NormalStrength("Normal Strength", Range( 0 , 1)) = 0.4
		_NormalDirection1("Normal Direction 1", Vector) = (0.4,0,0,0)
		_NormalDirection2("Normal Direction 2", Vector) = (-0.2,0,0,0)
		_FoamEdgeDistance("Foam Edge Distance", Range( 0 , 0.5)) = 0.06
		_FoamEdgeGradContrast("Foam Edge Grad Contrast", Range( 1 , 10)) = 10
		_FoamEdgeNoiseContrast("Foam Edge Noise Contrast", Range( 1 , 10)) = 1
		_FoamSpeed("Foam Speed", Range( 0 , 0.5)) = 0.25
		_FoamPanDirection("Foam Pan Direction", Vector) = (-1,0.5,0,0)
		_FoamNoiseTiling("Foam Noise Tiling", Range( 1 , 10)) = 1
		_RipplesTexture("Ripples Texture", 2D) = "white" {}
		_RipplesTIling("Ripples TIling", Range( 0 , 1)) = 0.2
		_RipplesSpeed("Ripples Speed", Range( 0 , 5)) = 1
		_RipplesDissolve("Ripples Dissolve", Range( 0 , 1)) = 0.7
		_RipplesTransparency("Ripples Transparency", Range( 0 , 1)) = 0.4
		_RipplesNormalMaskValue("Ripples Normal Mask Value", Range( 0 , 1)) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
		};

		uniform sampler2D _NormalTexture;
		uniform float2 _NormalDirection1;
		uniform float _NormalSpeed;
		uniform float _NormalTiling;
		uniform float _NormalStrength;
		uniform float2 _NormalDirection2;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FoamEdgeDistance;
		uniform float _FoamEdgeGradContrast;
		uniform float2 _FoamPanDirection;
		uniform float _FoamSpeed;
		uniform float _FoamNoiseTiling;
		uniform float _FoamEdgeNoiseContrast;
		uniform float4 _WaterColorDeep;
		uniform float4 _WaterColorShallow;
		uniform float _Depth;
		uniform sampler2D _RipplesTexture;
		uniform float _RipplesSpeed;
		uniform float _RipplesTIling;
		uniform float _RipplesDissolve;
		uniform float _RipplesTransparency;
		uniform float _RipplesNormalMaskValue;
		uniform float4 _FoamAndRipplesEmission;
		uniform float _Smoothness;
		uniform float _Opacity;


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float4 appendResult48 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 World_Space_Tile49 = appendResult48;
			float4 temp_output_200_0 = ( World_Space_Tile49 / 10.0 );
			float2 panner187 = ( 1.0 * _Time.y * ( _NormalDirection1 * _NormalSpeed ) + ( temp_output_200_0 * _NormalTiling ).xy);
			float3 tex2DNode114 = UnpackScaleNormal( tex2D( _NormalTexture, panner187 ), _NormalStrength );
			float2 panner188 = ( 1.0 * _Time.y * ( _NormalDirection2 * ( _NormalSpeed * 3.0 ) ) + ( temp_output_200_0 * ( _NormalTiling * 3.0 ) ).xy);
			float3 tex2DNode181 = UnpackScaleNormal( tex2D( _NormalTexture, panner188 ), _NormalStrength );
			float3 Normals197 = BlendNormals( tex2DNode114 , tex2DNode181 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth128 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth128 = abs( ( screenDepth128 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FoamEdgeDistance ) );
			float4 temp_output_227_0 = ( World_Space_Tile49 / 10.0 );
			float2 panner232 = ( 1.0 * _Time.y * ( _FoamPanDirection * ( _FoamSpeed * 3.0 ) ) + temp_output_227_0.xy);
			float simplePerlin2D386 = snoise( panner232*_FoamNoiseTiling );
			simplePerlin2D386 = simplePerlin2D386*0.5 + 0.5;
			float2 panner397 = ( 1.0 * _Time.y * ( ( _FoamPanDirection * float2( -1.1,-1.1 ) ) * ( ( _FoamSpeed * 0.8 ) * 3.0 ) ) + temp_output_227_0.xy);
			float simplePerlin2D403 = snoise( panner397*( _FoamNoiseTiling * 0.75 ) );
			simplePerlin2D403 = simplePerlin2D403*0.5 + 0.5;
			float clampResult175 = clamp( ( ( ( 1.0 - distanceDepth128 ) * _FoamEdgeGradContrast ) + ( ( simplePerlin2D386 * simplePerlin2D403 ) * ( _FoamEdgeNoiseContrast * 10.0 ) ) ) , 0.0 , 1.0 );
			float Foam133 = clampResult175;
			float3 lerpResult415 = lerp( Normals197 , float3(0,0,1) , Foam133);
			o.Normal = lerpResult415;
			float screenDepth272 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth272 = abs( ( screenDepth272 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			float4 lerpResult121 = lerp( _WaterColorDeep , _WaterColorShallow , saturate( ( 1.0 - distanceDepth272 ) ));
			float4 Base_Color125 = lerpResult121;
			o.Albedo = ( Base_Color125 + Foam133 ).rgb;
			float temp_output_345_0 = ( _RipplesSpeed * 0.01 );
			float2 temp_cast_5 = (( temp_output_345_0 * -0.8 )).xx;
			float2 panner326 = ( 1.0 * _Time.y * temp_cast_5 + ( World_Space_Tile49 * ( _RipplesTIling * 1.5 ) ).xy);
			float2 temp_cast_7 = (temp_output_345_0).xx;
			float2 panner320 = ( 1.0 * _Time.y * temp_cast_7 + ( World_Space_Tile49 * _RipplesTIling ).xy);
			float lerpResult360 = lerp( ( saturate( pow( ( ( tex2D( _RipplesTexture, panner326 ).r * tex2D( _RipplesTexture, panner320 ).r ) * 20.0 ) , ( _RipplesDissolve * 100.0 ) ) ) * _RipplesTransparency ) , 0.0 , saturate( ( ( tex2DNode114.r * tex2DNode181.r ) * ( _RipplesNormalMaskValue * 2000.0 ) ) ));
			float4 Ripples312 = ( max( lerpResult360 , Foam133 ) * _FoamAndRipplesEmission );
			o.Emission = Ripples312.rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = _Opacity;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;285;128,-1664;Inherit;False;4857.05;1086.572;;33;302;372;312;299;384;369;290;341;371;370;368;360;366;367;340;295;319;318;359;315;321;323;330;329;325;335;345;320;326;324;317;328;314;Ripples;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;50;128,-4000;Inherit;False;816.4476;247.9346;Comment;3;47;48;49;World Space UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;231;128,-448;Inherit;False;2870.649;1424.903;Comment;29;387;230;132;131;383;386;379;175;133;130;128;129;227;229;224;232;235;233;234;236;394;395;397;400;402;403;404;407;409;Edge Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;198;128,-2816;Inherit;False;3069.942;1016.891;Comment;21;193;192;201;184;185;200;182;183;186;196;114;181;180;188;187;191;189;194;190;197;195;Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;127;128,-3632;Inherit;False;1862.404;699.7971;Comment;8;274;272;118;119;281;125;121;374;Base Color;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;47;176,-3952;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;48;432,-3952;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BlendNormalsNode;195;2528,-2400;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;119;1040,-3312;Inherit;False;Property;_WaterColorShallow;Water Color Shallow;4;0;Create;True;0;0;0;False;0;False;0.2671324,0.8207547,0.8099126,0;0.01960784,0,0.6901961,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;272;512,-3104;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;197;2944,-2400;Inherit;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector2Node;190;1184,-1968;Float;False;Property;_NormalDirection2;Normal Direction 2;11;0;Create;True;0;0;0;False;0;False;-0.2,0;-1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;1488,-1968;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;1488,-2624;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;187;1744,-2720;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;720,-2064;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;720,-2720;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;200;512,-2320;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;201;208,-2320;Inherit;False;Constant;_Float0;Float 0;17;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;193;1488,-2320;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;118;1040,-3504;Inherit;False;Property;_WaterColorDeep;Water Color Deep;3;0;Create;True;0;0;0;False;0;False;0.127214,0.09344961,0.7924528,0;0.07058824,0.9764706,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;182;208,-2720;Inherit;False;49;World Space Tile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;224,-144;Inherit;False;49;World Space Tile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;227;528,-144;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;49;640,-3952;Inherit;False;World Space Tile;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DepthFade;128;1248,-272;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;281;816,-3104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;121;1344,-3504;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;125;1664,-3504;Inherit;False;Base Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;374;1104,-3104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;130;1680,-272;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;379;1920,-272;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;1920,-144;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;188;1744,-2064;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;864,192;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;232;1104,192;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;394;864,608;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;397;1104,608;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;386;1536,192;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;403;1536,608;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;407;1792,192;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;409;1728,-48;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;132;1424,-48;Inherit;False;Property;_FoamEdgeNoiseContrast;Foam Edge Noise Contrast;14;0;Create;True;0;0;0;False;0;False;1;1;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;383;1424,-144;Inherit;False;Property;_FoamEdgeGradContrast;Foam Edge Grad Contrast;13;0;Create;True;0;0;0;False;0;False;10;1;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;234;640,336;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;402;640,608;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1.1,-1.1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;395;640,752;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;400;464,752;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;404;1376,448;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.75;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;175;2320,-144;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;230;2144,-144;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;2704,-144;Inherit;False;Foam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;274;272,-3104;Inherit;False;Property;_Depth;Depth;2;0;Create;True;0;0;0;False;0;False;-0.3;-4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;912,-272;Inherit;False;Property;_FoamEdgeDistance;Foam Edge Distance;12;0;Create;True;0;0;0;False;0;False;0.06;0.25;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;233;224,336;Inherit;False;Property;_FoamSpeed;Foam Speed;15;0;Create;True;0;0;0;False;0;False;0.25;0.04;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;387;1088,448;Inherit;False;Property;_FoamNoiseTiling;Foam Noise Tiling;17;0;Create;True;0;0;0;False;0;False;1;5;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;114;2128,-2720;Inherit;True;Property;_TextureSample0;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.2;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;180;1744,-2432;Inherit;True;Property;_NormalTexture;Normal Texture;6;0;Create;True;0;0;0;False;0;False;None;None;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;181;2128,-2064;Inherit;True;Property;_TextureSample1;Texture Sample 0;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.2;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;185;1008,-2320;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;192;1184,-2320;Inherit;False;Property;_NormalSpeed;Normal Speed;8;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;189;1184,-2624;Float;False;Property;_NormalDirection1;Normal Direction 1;10;0;Create;True;0;0;0;False;0;False;0.4,0;1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;235;224,192;Float;False;Property;_FoamPanDirection;Foam Pan Direction;16;0;Create;True;0;0;0;False;0;False;-1,0.5;-1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;314;1408,-944;Inherit;True;Property;_TextureSample4;Texture Sample 2;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;328;1792,-1200;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;317;1008,-944;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;324;1008,-1472;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;326;1184,-1344;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;320;1184,-832;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;345;496,-816;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;335;1008,-1328;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;325;816,-1328;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;2000,-1200;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;330;2000,-944;Inherit;False;Constant;_Float5;Float 5;29;0;Create;True;0;0;0;False;0;False;20;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;323;1408,-1472;Inherit;True;Property;_TextureSample5;Texture Sample 2;18;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;321;192,-816;Inherit;False;Property;_RipplesSpeed;Ripples Speed;20;0;Create;True;0;0;0;False;0;False;1;0.02;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;315;1408,-1200;Inherit;True;Property;_RipplesTexture;Ripples Texture;18;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;359;192,-1472;Inherit;False;49;World Space Tile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;318;192,-928;Inherit;False;Property;_RipplesTIling;Ripples TIling;19;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;319;2768,-1200;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;295;2464,-1200;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;340;3312,-1200;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;367;3312,-1440;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;300;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;366;2992,-1440;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;360;3632,-1200;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;368;3632,-1440;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;370;3328,-1568;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2000;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;371;2544,-816;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;341;3280,-816;Inherit;False;Property;_RipplesTransparency;Ripples Transparency;22;0;Create;True;0;0;0;False;0;False;0.4;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;290;2240,-816;Inherit;False;Property;_RipplesDissolve;Ripples Dissolve;21;0;Create;True;0;0;0;False;0;False;0.7;5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;369;2992,-1568;Inherit;False;Property;_RipplesNormalMaskValue;Ripples Normal Mask Value;23;0;Create;True;0;0;0;False;0;False;1;300;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;384;4208,-1200;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;299;4384,-1200;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;312;4704,-1200;Inherit;False;Ripples;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;372;3968,-816;Inherit;False;133;Foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;302;4320,-816;Inherit;False;Property;_FoamAndRipplesEmission;Foam And Ripples Emission;5;1;[HDR];Create;True;0;0;0;False;0;False;0.509434,0.509434,0.509434,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;126;3712,-2800;Inherit;False;125;Base Color;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;3712,-2672;Inherit;False;133;Foam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;311;3952,-2800;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;199;3712,-2528;Inherit;False;197;Normals;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;417;3712,-2400;Inherit;False;Constant;_Vector0;Vector 0;25;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;415;3952,-2528;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;313;3712,-2192;Inherit;False;312;Ripples;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;113;3712,-2080;Inherit;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;0;False;0;False;0.9;0.9;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;176;3712,-1984;Inherit;False;Property;_Opacity;Opacity;0;0;Create;True;0;0;0;False;0;False;0.4;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;184;720,-2320;Inherit;False;Property;_NormalTiling;Normal Tiling;7;0;Create;True;0;0;0;False;0;False;1;1;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;229;224,0;Inherit;False;Constant;_float1;float 1;19;0;Create;True;0;0;0;False;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;418;4560,-2800;Float;False;True;-1;2;;0;0;Standard;TidalFlask/Water Depth Standard;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;196;2128,-2384;Inherit;False;Property;_NormalStrength;Normal Strength;9;0;Create;True;0;0;0;False;0;False;0.4;0.2;0;1;0;1;FLOAT;0
WireConnection;48;0;47;1
WireConnection;48;1;47;3
WireConnection;195;0;114;0
WireConnection;195;1;181;0
WireConnection;272;0;274;0
WireConnection;197;0;195;0
WireConnection;194;0;190;0
WireConnection;194;1;193;0
WireConnection;191;0;189;0
WireConnection;191;1;192;0
WireConnection;187;0;183;0
WireConnection;187;2;191;0
WireConnection;186;0;200;0
WireConnection;186;1;185;0
WireConnection;183;0;200;0
WireConnection;183;1;184;0
WireConnection;200;0;182;0
WireConnection;200;1;201;0
WireConnection;193;0;192;0
WireConnection;227;0;224;0
WireConnection;227;1;229;0
WireConnection;49;0;48;0
WireConnection;128;0;129;0
WireConnection;281;0;272;0
WireConnection;121;0;118;0
WireConnection;121;1;119;0
WireConnection;121;2;374;0
WireConnection;125;0;121;0
WireConnection;374;0;281;0
WireConnection;130;0;128;0
WireConnection;379;0;130;0
WireConnection;379;1;383;0
WireConnection;131;0;407;0
WireConnection;131;1;409;0
WireConnection;188;0;186;0
WireConnection;188;2;194;0
WireConnection;236;0;235;0
WireConnection;236;1;234;0
WireConnection;232;0;227;0
WireConnection;232;2;236;0
WireConnection;394;0;402;0
WireConnection;394;1;395;0
WireConnection;397;0;227;0
WireConnection;397;2;394;0
WireConnection;386;0;232;0
WireConnection;386;1;387;0
WireConnection;403;0;397;0
WireConnection;403;1;404;0
WireConnection;407;0;386;0
WireConnection;407;1;403;0
WireConnection;409;0;132;0
WireConnection;234;0;233;0
WireConnection;402;0;235;0
WireConnection;395;0;400;0
WireConnection;400;0;233;0
WireConnection;404;0;387;0
WireConnection;175;0;230;0
WireConnection;230;0;379;0
WireConnection;230;1;131;0
WireConnection;133;0;175;0
WireConnection;114;0;180;0
WireConnection;114;1;187;0
WireConnection;114;5;196;0
WireConnection;181;0;180;0
WireConnection;181;1;188;0
WireConnection;181;5;196;0
WireConnection;185;0;184;0
WireConnection;314;0;315;0
WireConnection;314;1;320;0
WireConnection;328;0;323;1
WireConnection;328;1;314;1
WireConnection;317;0;359;0
WireConnection;317;1;318;0
WireConnection;324;0;359;0
WireConnection;324;1;325;0
WireConnection;326;0;324;0
WireConnection;326;2;335;0
WireConnection;320;0;317;0
WireConnection;320;2;345;0
WireConnection;345;0;321;0
WireConnection;335;0;345;0
WireConnection;325;0;318;0
WireConnection;329;0;328;0
WireConnection;329;1;330;0
WireConnection;323;0;315;0
WireConnection;323;1;326;0
WireConnection;319;0;295;0
WireConnection;295;0;329;0
WireConnection;295;1;371;0
WireConnection;340;0;319;0
WireConnection;340;1;341;0
WireConnection;367;0;366;0
WireConnection;367;1;370;0
WireConnection;366;0;114;1
WireConnection;366;1;181;1
WireConnection;360;0;340;0
WireConnection;360;2;368;0
WireConnection;368;0;367;0
WireConnection;370;0;369;0
WireConnection;371;0;290;0
WireConnection;384;0;360;0
WireConnection;384;1;372;0
WireConnection;299;0;384;0
WireConnection;299;1;302;0
WireConnection;312;0;299;0
WireConnection;311;0;126;0
WireConnection;311;1;134;0
WireConnection;415;0;199;0
WireConnection;415;1;417;0
WireConnection;415;2;134;0
WireConnection;418;0;311;0
WireConnection;418;1;415;0
WireConnection;418;2;313;0
WireConnection;418;4;113;0
WireConnection;418;9;176;0
ASEEND*/
//CHKSM=8FAE20B908BE511460FB73F7631FC2C1290577A2