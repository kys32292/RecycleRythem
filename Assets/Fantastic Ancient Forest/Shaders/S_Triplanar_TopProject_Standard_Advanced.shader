// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TidalFlask/Triplanar TopProject Standard Advanced"
{
	Properties
	{
		[NoScaleOffset]_AssetSpecificNormal("Asset Specific Normal", 2D) = "bump" {}
		_AssetSpecificNormalStrength("Asset Specific Normal Strength", Range( 0 , 3)) = 1
		_DetailTextureTint("Detail Texture Tint", Color) = (1,1,1,0)
		[NoScaleOffset]_DetailTexture("Detail Texture", 2D) = "white" {}
		[NoScaleOffset][Normal]_DetailNormal("Detail Normal", 2D) = "bump" {}
		_DetailNormalStrength("Detail Normal Strength", Range( 0 , 3)) = 1
		[NoScaleOffset]_DetailPBR("Detail PBR", 2D) = "white" {}
		_DetailRoughnessMin("Detail Roughness Min", Range( 0 , 1)) = 0.1
		_DetailRoughnessMax("Detail Roughness Max", Range( 0 , 1)) = 1
		_DetailTextureTiling("Detail Texture Tiling", Float) = 1
		_TriplanarBlendContrast("Triplanar Blend Contrast", Float) = 22
		_TopTextureTint("Top Texture Tint", Color) = (1,1,1,0)
		[NoScaleOffset]_TopTexture("Top Texture", 2D) = "white" {}
		_TopTextureTiling("Top Texture Tiling", Float) = 1
		_TopCoverageAmount("Top Coverage Amount", Range( -2 , 1)) = -1
		_TopCoverageFalloff("Top Coverage Falloff", Range( 0.01 , 2)) = 0.5
		_TopCoverageNoiseScale("Top Coverage Noise Scale", Range( 0 , 5)) = 0.85
		_TopRoughness("Top Roughness", Range( 0 , 1)) = 0.8
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _AssetSpecificNormal;
		uniform float _AssetSpecificNormalStrength;
		uniform sampler2D _DetailNormal;
		uniform float _DetailTextureTiling;
		uniform float _DetailNormalStrength;
		uniform float _TriplanarBlendContrast;
		uniform float _TopCoverageNoiseScale;
		uniform float _TopCoverageAmount;
		uniform float _TopCoverageFalloff;
		uniform float4 _DetailTextureTint;
		uniform sampler2D _DetailTexture;
		uniform float4 _TopTextureTint;
		uniform sampler2D _TopTexture;
		uniform float _TopTextureTiling;
		uniform sampler2D _DetailPBR;
		uniform float _DetailRoughnessMin;
		uniform float _DetailRoughnessMax;
		uniform float _TopRoughness;


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
			float2 uv_AssetSpecificNormal15 = i.uv_texcoord;
			float3 AssetSpecificNormal137 = UnpackScaleNormal( tex2D( _AssetSpecificNormal, uv_AssetSpecificNormal15 ), _AssetSpecificNormalStrength );
			float3 ase_worldPos = i.worldPos;
			float3 temp_output_95_0 = ( ase_worldPos * _DetailTextureTiling );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 break67 = ( sign( ase_worldNormal ) * float3( 1,1,-1 ) );
			float2 appendResult68 = (float2(break67.x , 1.0));
			float3 break165 = (UnpackScaleNormal( tex2D( _DetailNormal, ( (temp_output_95_0).zy * appendResult68 ) ), _DetailNormalStrength )).zyx;
			float WorldNormal_X229 = ase_worldNormal.x;
			float WorldNormal_Y230 = ase_worldNormal.y;
			float3 break178 = sign( ase_worldNormal );
			float WorldNormalSign_X236 = break178.x;
			float WorldNormal_Z231 = ase_worldNormal.z;
			float3 appendResult174 = (float3(( break165.x * WorldNormal_X229 ) , ( break165.y + WorldNormal_Y230 ) , ( ( break165.z * WorldNormalSign_X236 ) + WorldNormal_Z231 )));
			float2 appendResult69 = (float2(break67.y , 1.0));
			float3 break180 = (UnpackScaleNormal( tex2D( _DetailNormal, ( (temp_output_95_0).xz * appendResult69 ) ), _DetailNormalStrength )).xzy;
			float WorldNormalSign_Y237 = break178.y;
			float3 appendResult185 = (float3(( ( break180.x * WorldNormalSign_Y237 ) + WorldNormal_X229 ) , ( break180.y * WorldNormal_Y230 ) , ( break180.z + WorldNormal_Z231 )));
			float3 temp_cast_0 = (_TriplanarBlendContrast).xxx;
			float3 temp_output_72_0 = pow( abs( ase_worldNormal ) , temp_cast_0 );
			float dotResult64 = dot( temp_output_72_0 , float3( 1,1,1 ) );
			float3 break75 = ( temp_output_72_0 / dotResult64 );
			float TriplanarBlendY153 = break75.y;
			float3 lerpResult102 = lerp( appendResult174 , appendResult185 , TriplanarBlendY153);
			float2 appendResult160 = (float2(break67.z , 1.0));
			float3 break192 = ( UnpackScaleNormal( tex2D( _DetailNormal, ( (temp_output_95_0).xy * appendResult160 ) ), _DetailNormalStrength ) * float3( -1,1,1 ) );
			float WorldNormalSign_Z238 = break178.z;
			float3 appendResult197 = (float3(( ( break192.x * WorldNormalSign_Z238 ) + WorldNormal_X229 ) , ( break192.y + WorldNormal_Y230 ) , ( break192.z * WorldNormal_Z231 )));
			float TriplanarBlendZ154 = break75.z;
			float3 lerpResult103 = lerp( lerpResult102 , appendResult197 , TriplanarBlendZ154);
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float3 worldToTangentDir209 = mul( ase_worldToTangent, lerpResult103);
			float3 DetailNormal208 = worldToTangentDir209;
			float3 break20 = ase_worldPos;
			float2 appendResult23 = (float2(break20.x , break20.z));
			float simplePerlin2D27 = snoise( (appendResult23*_TopCoverageNoiseScale + 0.0) );
			simplePerlin2D27 = simplePerlin2D27*0.5 + 0.5;
			float TopProjAlpha147 = saturate( pow( saturate( ( simplePerlin2D27 + ( ase_worldNormal.y + _TopCoverageAmount ) ) ) , _TopCoverageFalloff ) );
			float3 lerpResult2 = lerp( BlendNormals( AssetSpecificNormal137 , DetailNormal208 ) , float3(0,0,1) , TopProjAlpha147);
			o.Normal = lerpResult2;
			float TriplanarBlendX152 = break75.x;
			float4 lerpResult80 = lerp( tex2D( _DetailTexture, ( (temp_output_95_0).xz * appendResult69 ) ) , tex2D( _DetailTexture, ( (temp_output_95_0).zy * appendResult68 ) ) , TriplanarBlendX152);
			float4 lerpResult81 = lerp( lerpResult80 , tex2D( _DetailTexture, ( (temp_output_95_0).xy * appendResult160 ) ) , TriplanarBlendZ154);
			float4 TriplanarBaseTexture142 = ( _DetailTextureTint * lerpResult81 );
			float4 TopProjBaseTexture144 = ( _TopTextureTint * tex2D( _TopTexture, (( ase_worldPos * _TopTextureTiling )).xz ) );
			float4 lerpResult1 = lerp( TriplanarBaseTexture142 , TopProjBaseTexture144 , TopProjAlpha147);
			o.Albedo = lerpResult1.rgb;
			o.Metallic = 0.0;
			float4 tex2DNode116 = tex2D( _DetailPBR, (temp_output_95_0).xz );
			float4 tex2DNode110 = tex2D( _DetailPBR, (temp_output_95_0).zy );
			float lerpResult113 = lerp( tex2DNode116.g , tex2DNode110.g , TriplanarBlendX152);
			float4 tex2DNode115 = tex2D( _DetailPBR, (temp_output_95_0).xy );
			float lerpResult114 = lerp( lerpResult113 , tex2DNode115.g , TriplanarBlendZ154);
			float clampResult43 = clamp( lerpResult114 , _DetailRoughnessMin , _DetailRoughnessMax );
			float DetailRoughness139 = ( 1.0 - clampResult43 );
			float TopRoughness260 = ( 1.0 - _TopRoughness );
			float lerpResult270 = lerp( DetailRoughness139 , TopRoughness260 , TopProjAlpha147);
			o.Smoothness = lerpResult270;
			float lerpResult121 = lerp( tex2DNode116.b , tex2DNode110.b , TriplanarBlendX152);
			float lerpResult122 = lerp( lerpResult121 , tex2DNode115.b , TriplanarBlendZ154);
			float DetailAO140 = lerpResult122;
			float TopAO265 = 1.0;
			float lerpResult271 = lerp( DetailAO140 , TopAO265 , TopProjAlpha147);
			o.Occlusion = lerpResult271;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.CommentaryNode;276;580.0468,-447.5756;Inherit;False;940.2134;304;base color;3;1;146;143;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;275;586.8402,147.8199;Inherit;False;926.3772;355.3329;normal;5;211;138;210;2;283;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;273;621.0009,554.6826;Inherit;False;889.1191;244.8593;roughness;3;270;266;268;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;272;651.7559,838.3573;Inherit;False;854.9749;247.2662;ao;3;271;267;269;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;136;-3484.643,-969.9871;Inherit;False;3719.291;2177.022;triplanar normal;27;215;214;234;233;232;175;230;231;229;240;241;239;178;177;238;237;236;209;103;208;102;190;187;188;278;281;287;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;188;-2973.065,-163.1269;Inherit;False;2044.308;419.4538;Y;10;168;200;180;179;101;181;184;182;183;185;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;187;-2974.527,-603.4139;Inherit;False;2042.483;416.3286;X;10;171;199;100;165;164;174;172;170;173;280;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;150;-3484.234,3698.168;Inherit;False;3034.456;677.1356;top projection alpha;15;147;30;24;21;33;39;22;26;31;29;28;27;25;23;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;123;-3490.854,1241.503;Inherit;False;2611.711;1242.419;triplanar pbr;11;158;157;119;116;115;112;111;110;109;124;125;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;125;-2434.802,1501.245;Inherit;False;1315.7;510.0204;roughness;7;139;113;114;41;45;44;43;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;124;-2432.196,2055.453;Inherit;False;1311.203;352.1608;ambient occlusion;3;140;121;122;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;97;-3488.701,-1984.978;Inherit;False;2852.061;964.715;triplanar base texture;20;163;162;161;94;91;92;90;156;155;81;80;142;17;55;95;78;77;79;96;76;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;89;-5239.096,396.0167;Inherit;False;859.4316;356.8742;temp cheep method triplanar blend;6;83;84;85;86;87;88;;1,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;53;-3473.525,2628.854;Inherit;False;3020;940.2972;top projection;13;286;265;285;262;260;144;19;6;7;10;217;216;50;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;52;-3485.633,-2441.688;Inherit;False;1101.174;275.7933;asset specific normal map;3;15;13;137;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;62;-5243.044,-16.99763;Inherit;False;1677.021;321.5135;triplanar blend;10;152;153;154;65;73;72;71;64;74;75;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;63;-4275.492,-467.4065;Inherit;False;712.0381;414.161;mirror projection based on vertex normal;6;160;69;67;70;66;68;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SwizzleNode;109;-3440.854,1934.039;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;112;-3440.854,1744.034;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;115;-3249.685,1909.735;Inherit;True;Property;_BaseTexture8;Base Texture;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;122;-2382.196,2260.535;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;121;-2380.804,2105.453;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;114;-2384.802,1719.339;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;113;-2381.625,1551.246;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;190;-2974.829,281.1083;Inherit;False;2049.608;383.0863;Z;10;195;193;169;201;98;192;194;196;197;203;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;68;-3751.481,-417.406;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;67;-3896.484,-416.406;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;69;-3752.64,-291.6441;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;160;-3753.666,-177.0736;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;75;-4031.137,39.79531;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SwizzleNode;83;-4968.657,447.1127;Inherit;False;FLOAT2;1;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;84;-4968.087,532.8657;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;85;-4968.087,614.8662;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;86;-4772.087,447.8647;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;87;-4557.664,591.8903;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;88;-5189.096,446.0168;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;74;-4460.724,40.27724;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-4610.43,109.2839;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SignOpNode;66;-4225.492,-415.406;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.AbsOpNode;71;-4968.694,42.13539;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;72;-4799.728,39.27724;Inherit;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;73;-5194.943,43.5292;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-4075.491,-415.406;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,-1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-5192.433,203.2477;Inherit;False;Property;_TriplanarBlendContrast;Triplanar Blend Contrast;10;0;Create;True;0;0;0;False;0;False;22;22;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;154;-3847.222,213.8392;Inherit;False;TriplanarBlendZ;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;153;-3847.222,128.395;Inherit;False;TriplanarBlendY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-3850.427,40.81448;Inherit;False;TriplanarBlendX;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;157;-2751.62,1724.459;Inherit;False;152;TriplanarBlendX;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;-2755.62,1899.46;Inherit;False;154;TriplanarBlendZ;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;140;-1451.223,2260.637;Inherit;False;DetailAO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;43;-1840.548,1719.412;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;41;-1659.029,1718.612;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-2190.264,1832.192;Inherit;False;Property;_DetailRoughnessMin;Detail Roughness Min;7;0;Create;True;0;0;0;False;0;False;0.1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-2193.32,1911.194;Inherit;False;Property;_DetailRoughnessMax;Detail Roughness Max;8;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;20;-3188.401,3754.91;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;23;-3034.346,3754.529;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;25;-2817.009,3754.597;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;27;-2542.412,3748.168;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-2216.888,3753.642;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;29;-1947.303,3754.665;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;31;-1726.219,3752.615;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-2430.359,4020.639;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;22;-2784.913,3974.131;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;39;-3426.234,3755.245;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;33;-1148.393,3753.361;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;50;-2967.084,2900.999;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;216;-3404.785,2901.593;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;217;-3170.29,2901.853;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-3406.88,3058.343;Inherit;False;Property;_TopTextureTiling;Top Texture Tiling;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;7;-2490.414,2678.854;Inherit;False;Property;_TopTextureTint;Top Texture Tint;11;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-2152.148,2853.032;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;19;-2494.101,2876.879;Inherit;True;Property;_TopTexture;Top Texture;12;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;1a0f352c8e3b8324cbc0fca09bdeaddc;1a0f352c8e3b8324cbc0fca09bdeaddc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;144;-754.2445,2854.249;Inherit;False;TopProjBaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-3159.542,4232.443;Inherit;False;Property;_TopCoverageNoiseScale;Top Coverage Noise Scale;16;0;Create;True;0;0;0;False;0;False;0.85;1;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-2796.954,4231.609;Float;False;Property;_TopCoverageAmount;Top Coverage Amount;14;0;Create;True;0;0;0;False;0;False;-1;-0.18;-2;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2064.289,4225.546;Float;False;Property;_TopCoverageFalloff;Top Coverage Falloff;15;0;Create;True;0;0;0;False;0;False;0.5;0.65;0.01;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;269;703.4987,899.9732;Inherit;False;140;DetailAO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;267;701.7559,976.2318;Inherit;False;265;TopAO;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;271;1269.186,888.3571;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;268;671.9539,615.3747;Inherit;False;139;DetailRoughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;266;671.0009,688.3462;Inherit;False;260;TopRoughness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;270;1268.966,604.6826;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;2;1268.455,197.8199;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;210;959.6415,199.4653;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;211;700.0503,285.2535;Inherit;False;208;DetailNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;151;1275.148,-39.80754;Inherit;False;Constant;_Metallic;Metallic;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1;1259.26,-397.5755;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;146;630.0468,-316.1302;Inherit;False;144;TopProjBaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;143;631.0988,-395.633;Inherit;False;142;TriplanarBaseTexture;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;148;663.9731,-42.13575;Inherit;False;147;TopProjAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;15;-3133.042,-2391.688;Inherit;True;Property;_AssetSpecificNormal;Asset Specific Normal;0;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;d9e29060299f3dc49a89e7eb39ec1580;d9e29060299f3dc49a89e7eb39ec1580;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;137;-2800.834,-2391.607;Inherit;False;AssetSpecificNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-3435.633,-2343.48;Inherit;False;Property;_AssetSpecificNormalStrength;Asset Specific Normal Strength;1;0;Create;True;0;0;0;False;0;False;1;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-1390.566,-1307.173;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;142;-1174.581,-1305.782;Inherit;False;TriplanarBaseTexture;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;81;-1610.946,-1283.87;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;17;-1456.693,-1484.675;Inherit;False;Property;_DetailTextureTint;Detail Texture Tint;2;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;156;-2027.995,-1185.061;Inherit;False;154;TriplanarBlendZ;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;201;-2693.747,447.779;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;194;-1348.114,325.5612;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;-1348.344,440.9021;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;195;-1352.114,545.5619;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;203;-2069.622,425.0842;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;-1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;98;-2945.385,446.5804;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;192;-1866.103,425.1669;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;197;-1137.894,417.8392;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-1515.843,325.8418;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;102;-636.9684,-43.96581;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;103;-470.2572,390.2814;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;164;-2062.527,-468.3552;Inherit;False;FLOAT3;2;1;0;3;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;165;-1870.527,-468.3552;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SwizzleNode;100;-2940.504,-445.6754;Inherit;False;FLOAT2;2;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;199;-2697.816,-444.8482;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;171;-1342.128,-441.1299;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;174;-1153.679,-467.6973;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;173;-1341.632,-309.4916;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;-1521.556,-312.4663;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;-1342.833,-544.1686;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;101;-2943.908,9.943236;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;180;-1868.24,-18.06958;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;200;-2696.608,5.463559;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;183;-1336.705,17.11771;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;181;-1336.182,135.8047;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;179;-2060.24,-18.06958;Inherit;False;FLOAT3;0;2;1;3;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;182;-1334.705,-105.8818;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-1518.182,-106.1947;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;185;-1138.502,-19.46222;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;209;-279.6562,391.5587;Inherit;False;World;Tangent;False;Fast;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SignOpNode;177;-2729.839,997.165;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;178;-2569.841,996.165;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;239;-2158.092,945.4166;Inherit;False;236;WorldNormalSign_X;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;241;-2158.754,1096.889;Inherit;False;238;WorldNormalSign_Z;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;240;-2157.983,1021.832;Inherit;False;237;WorldNormalSign_Y;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;229;-2426.161,708.2767;Inherit;False;WorldNormal_X;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;231;-2425.161,845.2761;Inherit;False;WorldNormal_Z;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;230;-2425.161,776.2761;Inherit;False;WorldNormal_Y;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;232;-2130.614,706.1963;Inherit;False;229;WorldNormal_X;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;233;-2129.327,774.6921;Inherit;False;230;WorldNormal_Y;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;234;-2129.472,844.9202;Inherit;False;231;WorldNormal_Z;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;214;-859.8663,144.1499;Inherit;False;153;TriplanarBlendY;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;215;-727.837,534.0208;Inherit;False;154;TriplanarBlendZ;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;168;-2425.135,-17.70987;Inherit;True;Property;_TextureSample0;Texture Sample 0;22;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;169;-2427.758,423.5013;Inherit;True;Property;_TextureSample2;Texture Sample 0;21;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;79;-2947.322,-1241.489;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;163;-2689.498,-1240.816;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-3417.451,-1260.008;Inherit;False;Property;_DetailTextureTiling;Detail Texture Tiling;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;92;-2426.669,-1264.656;Inherit;True;Property;_BaseTexture3;Base Texture;1;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;77;-2948.889,-1621.243;Inherit;False;FLOAT2;2;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;78;-2947.322,-1431.49;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;162;-2689.418,-1428.451;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;161;-2690.244,-1620.769;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;76;-3418.258,-1433.805;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-3183.762,-1433.545;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;90;-2429.337,-1643.684;Inherit;True;Property;_BaseTexture1;Base Texture;2;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;91;-2427.669,-1453.657;Inherit;True;Property;_BaseTexture2;Base Texture;3;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;94;-2433.185,-1881.884;Inherit;True;Property;_DetailTexture;Detail Texture;3;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.LerpOp;80;-1809.869,-1453.729;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;-2029.995,-1343.061;Inherit;False;152;TriplanarBlendX;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;139;-1446.014,1717.632;Inherit;False;DetailRoughness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;119;-3256.2,1291.503;Inherit;True;Property;_DetailPBR;Detail PBR;6;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;d8db77d9c109ea14fb5cdf586d531dc1;d8db77d9c109ea14fb5cdf586d531dc1;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;278;-2429.822,-826.1844;Inherit;True;Property;_DetailNormal;Detail Normal;4;2;[NoScaleOffset];[Normal];Create;True;0;0;0;False;0;False;None;b02298606b5c6f545a43152a316520db;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;280;-2433.986,-468.9567;Inherit;True;Property;_TextureSample1;Texture Sample 1;22;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;281;-2815.228,-711.1995;Inherit;False;Property;_DetailNormalStrength;Detail Normal Strength;5;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;147;-731.6432,3753.381;Inherit;False;TopProjAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;208;-23.28091,393.7151;Inherit;False;DetailNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;138;636.8402,199.9652;Inherit;False;137;AssetSpecificNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;282;1770.394,-111.093;Float;False;True;-1;2;;0;0;Standard;TidalFlask/Triplanar TopProject Standard Advanced;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.SwizzleNode;111;-3436.422,1553.279;Inherit;False;FLOAT2;2;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector3Node;283;992.5039,346.1888;Inherit;False;Constant;_Vector0;Vector 0;22;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;260;-738.1285,3139.844;Inherit;False;TopRoughness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;262;-1481.131,3141.505;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;285;-1866.734,3143.775;Inherit;False;Property;_TopRoughness;Top Roughness;17;0;Create;True;0;0;0;False;0;False;0.8;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;265;-735.0999,3374.67;Inherit;False;TopAO;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;286;-1036.036,3375.295;Inherit;False;Constant;_Float0;Float 0;21;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;116;-3250.685,1719.732;Inherit;True;Property;_BaseTexture9;Base Texture;3;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;110;-3252.353,1529.703;Inherit;True;Property;_BaseTexture7;Base Texture;2;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;b02298606b5c6f545a43152a316520db;b02298606b5c6f545a43152a316520db;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;175;-2943.345,723.2181;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;287;-2945.995,993.9371;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;236;-2423.65,946.6033;Inherit;False;WorldNormalSign_X;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;237;-2423.65,1021.604;Inherit;False;WorldNormalSign_Y;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;238;-2422.65,1094.604;Inherit;False;WorldNormalSign_Z;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
WireConnection;109;0;95;0
WireConnection;112;0;95;0
WireConnection;115;0;119;0
WireConnection;115;1;109;0
WireConnection;122;0;121;0
WireConnection;122;1;115;3
WireConnection;122;2;158;0
WireConnection;121;0;116;3
WireConnection;121;1;110;3
WireConnection;121;2;157;0
WireConnection;114;0;113;0
WireConnection;114;1;115;2
WireConnection;114;2;158;0
WireConnection;113;0;116;2
WireConnection;113;1;110;2
WireConnection;113;2;157;0
WireConnection;68;0;67;0
WireConnection;67;0;70;0
WireConnection;69;0;67;1
WireConnection;160;0;67;2
WireConnection;75;0;74;0
WireConnection;83;0;88;0
WireConnection;84;0;88;0
WireConnection;85;0;88;0
WireConnection;86;0;84;0
WireConnection;86;1;83;0
WireConnection;87;0;86;0
WireConnection;87;1;85;0
WireConnection;74;0;72;0
WireConnection;74;1;64;0
WireConnection;64;0;72;0
WireConnection;66;0;73;0
WireConnection;71;0;73;0
WireConnection;72;0;71;0
WireConnection;72;1;65;0
WireConnection;70;0;66;0
WireConnection;154;0;75;2
WireConnection;153;0;75;1
WireConnection;152;0;75;0
WireConnection;140;0;122;0
WireConnection;43;0;114;0
WireConnection;43;1;44;0
WireConnection;43;2;45;0
WireConnection;41;0;43;0
WireConnection;20;0;39;0
WireConnection;23;0;20;0
WireConnection;23;1;20;2
WireConnection;25;0;23;0
WireConnection;25;1;21;0
WireConnection;27;0;25;0
WireConnection;28;0;27;0
WireConnection;28;1;26;0
WireConnection;29;0;28;0
WireConnection;31;0;29;0
WireConnection;31;1;30;0
WireConnection;26;0;22;2
WireConnection;26;1;24;0
WireConnection;33;0;31;0
WireConnection;50;0;217;0
WireConnection;217;0;216;0
WireConnection;217;1;10;0
WireConnection;6;0;7;0
WireConnection;6;1;19;0
WireConnection;19;1;50;0
WireConnection;144;0;6;0
WireConnection;271;0;269;0
WireConnection;271;1;267;0
WireConnection;271;2;148;0
WireConnection;270;0;268;0
WireConnection;270;1;266;0
WireConnection;270;2;148;0
WireConnection;2;0;210;0
WireConnection;2;1;283;0
WireConnection;2;2;148;0
WireConnection;210;0;138;0
WireConnection;210;1;211;0
WireConnection;1;0;143;0
WireConnection;1;1;146;0
WireConnection;1;2;148;0
WireConnection;15;5;13;0
WireConnection;137;0;15;0
WireConnection;55;0;17;0
WireConnection;55;1;81;0
WireConnection;142;0;55;0
WireConnection;81;0;80;0
WireConnection;81;1;92;0
WireConnection;81;2;156;0
WireConnection;201;0;98;0
WireConnection;201;1;160;0
WireConnection;194;0;196;0
WireConnection;194;1;232;0
WireConnection;193;0;192;1
WireConnection;193;1;233;0
WireConnection;195;0;192;2
WireConnection;195;1;234;0
WireConnection;203;0;169;0
WireConnection;98;0;95;0
WireConnection;192;0;203;0
WireConnection;197;0;194;0
WireConnection;197;1;193;0
WireConnection;197;2;195;0
WireConnection;196;0;192;0
WireConnection;196;1;241;0
WireConnection;102;0;174;0
WireConnection;102;1;185;0
WireConnection;102;2;214;0
WireConnection;103;0;102;0
WireConnection;103;1;197;0
WireConnection;103;2;215;0
WireConnection;164;0;280;0
WireConnection;165;0;164;0
WireConnection;100;0;95;0
WireConnection;199;0;100;0
WireConnection;199;1;68;0
WireConnection;171;0;165;1
WireConnection;171;1;233;0
WireConnection;174;0;170;0
WireConnection;174;1;171;0
WireConnection;174;2;173;0
WireConnection;173;0;172;0
WireConnection;173;1;234;0
WireConnection;172;0;165;2
WireConnection;172;1;239;0
WireConnection;170;0;165;0
WireConnection;170;1;232;0
WireConnection;101;0;95;0
WireConnection;180;0;179;0
WireConnection;200;0;101;0
WireConnection;200;1;69;0
WireConnection;183;0;180;1
WireConnection;183;1;233;0
WireConnection;181;0;180;2
WireConnection;181;1;234;0
WireConnection;179;0;168;0
WireConnection;182;0;184;0
WireConnection;182;1;232;0
WireConnection;184;0;180;0
WireConnection;184;1;240;0
WireConnection;185;0;182;0
WireConnection;185;1;183;0
WireConnection;185;2;181;0
WireConnection;209;0;103;0
WireConnection;177;0;287;0
WireConnection;178;0;177;0
WireConnection;229;0;175;1
WireConnection;231;0;175;3
WireConnection;230;0;175;2
WireConnection;168;0;278;0
WireConnection;168;1;200;0
WireConnection;168;5;281;0
WireConnection;169;0;278;0
WireConnection;169;1;201;0
WireConnection;169;5;281;0
WireConnection;79;0;95;0
WireConnection;163;0;79;0
WireConnection;163;1;160;0
WireConnection;92;0;94;0
WireConnection;92;1;163;0
WireConnection;77;0;95;0
WireConnection;78;0;95;0
WireConnection;162;0;78;0
WireConnection;162;1;69;0
WireConnection;161;0;77;0
WireConnection;161;1;68;0
WireConnection;95;0;76;0
WireConnection;95;1;96;0
WireConnection;90;0;94;0
WireConnection;90;1;161;0
WireConnection;91;0;94;0
WireConnection;91;1;162;0
WireConnection;80;0;91;0
WireConnection;80;1;90;0
WireConnection;80;2;155;0
WireConnection;139;0;41;0
WireConnection;280;0;278;0
WireConnection;280;1;199;0
WireConnection;280;5;281;0
WireConnection;147;0;33;0
WireConnection;208;0;209;0
WireConnection;282;0;1;0
WireConnection;282;1;2;0
WireConnection;282;3;151;0
WireConnection;282;4;270;0
WireConnection;282;5;271;0
WireConnection;111;0;95;0
WireConnection;260;0;262;0
WireConnection;262;0;285;0
WireConnection;265;0;286;0
WireConnection;116;0;119;0
WireConnection;116;1;112;0
WireConnection;110;0;119;0
WireConnection;110;1;111;0
WireConnection;236;0;178;0
WireConnection;237;0;178;1
WireConnection;238;0;178;2
ASEEND*/
//CHKSM=95F16D54D1BC6DAD65156F2FE9884B7B37AC93CA