
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
	
}

MODES
{
	Forward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
		#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
		#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "common/gradient.hlsl"
	#include "procedural.hlsl"
	
	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	
	float4 vColor : COLOR0 < Semantic( Color ); >;
	
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
	
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
				
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	SamplerState g_sSampler_9E3605E1 < Filter( ANISOTROPIC ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 16 ); >;
	CreateInputTexture2D( DirtTexture, Srgb, 8, "None", "_color", "Dirt,0/,0/0", DefaultFile( "materials/dirt/dirt_color.jpg" ) );
	CreateInputTexture2D( MudTexture, Srgb, 8, "None", "_color", "Mud,0/,0/0", DefaultFile( "materials/dirt/mud_color.jpg" ) );
	CreateInputTexture2D( DirtNormal, Linear, 8, "None", "_normal", "Dirt,0/,0/1", DefaultFile( "materials/dirt/dirt_normal.jpg" ) );
	CreateInputTexture2D( MudNormal, Linear, 8, "None", "_normal", "Mud,0/,0/0", DefaultFile( "materials/dirt/mud_normal.jpg" ) );
	CreateInputTexture2D( MudRoughness, Linear, 8, "None", "_rough", "Mud,0/,0/0", DefaultFile( "materials/dirt/mud_rough.jpg" ) );
	CreateInputTexture2D( MudAmbientOcclusion, Linear, 8, "None", "_ao", "Mud,0/,0/0", DefaultFile( "materials/dirt/mud_ao.jpg" ) );
	Texture2D g_tDirtTexture < Channel( RGBA, Box( DirtTexture ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tMudTexture < Channel( RGBA, Box( MudTexture ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tDirtNormal < Channel( RGBA, Box( DirtNormal ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tMudNormal < Channel( RGBA, Box( MudNormal ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tMudRoughness < Channel( RGBA, Box( MudRoughness ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tMudAmbientOcclusion < Channel( RGBA, Box( MudAmbientOcclusion ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tMudTexture )
	TextureAttribute( RepresentativeTexture, g_tMudTexture )
	float2 g_vDirtUVScale < UiGroup( "Dirt,0/,0/2" ); Default2( 1,1 ); Range2( 0,0, 100,100 ); >;
	float2 g_vDirtUVOffset < UiGroup( "Dirt,0/,0/3" ); Default2( 1,1 ); Range2( 0,0, 100,100 ); >;
	float2 g_vMudUVScale < UiGroup( "Mud,0/,0/0" ); Default2( 1,1 ); Range2( 0,0, 100,100 ); >;
	float2 g_vMudUVOffset < UiGroup( "Mud,0/,0/0" ); Default2( 1,1 ); Range2( 0,0, 100,100 ); >;
	float g_flBlendAmount < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;
		
	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : BACK );
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init( i );
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float2 l_0 = g_vDirtUVScale;
		float2 l_1 = g_vDirtUVOffset;
		float2 l_2 = TileAndOffsetUv( i.vTextureCoords.xy, l_0, l_1 );
		float4 l_3 = g_tDirtTexture.Sample( g_sSampler_9E3605E1,l_2 );
		float2 l_4 = g_vMudUVScale;
		float2 l_5 = g_vMudUVOffset;
		float2 l_6 = TileAndOffsetUv( i.vTextureCoords.xy, l_4, l_5 );
		float4 l_7 = g_tMudTexture.Sample( g_sSampler_9E3605E1,l_6 );
		float l_8 = g_flBlendAmount;
		float4 l_9 = saturate( lerp( l_3, l_7, l_8 ) );
		float2 l_10 = g_vDirtUVScale;
		float2 l_11 = g_vDirtUVOffset;
		float2 l_12 = TileAndOffsetUv( i.vTextureCoords.xy, l_10, l_11 );
		float4 l_13 = g_tDirtNormal.Sample( g_sSampler_9E3605E1,l_12 );
		float2 l_14 = g_vMudUVScale;
		float2 l_15 = g_vMudUVOffset;
		float2 l_16 = TileAndOffsetUv( i.vTextureCoords.xy, l_14, l_15 );
		float4 l_17 = g_tMudNormal.Sample( g_sSampler_9E3605E1,l_16 );
		float4 l_18 = saturate( lerp( l_13, l_17, l_8 ) );
		float2 l_19 = g_vMudUVScale;
		float2 l_20 = g_vMudUVOffset;
		float2 l_21 = TileAndOffsetUv( i.vTextureCoords.xy, l_19, l_20 );
		float4 l_22 = g_tMudRoughness.Sample( g_sSampler_9E3605E1,l_21 );
		float2 l_23 = g_vMudUVScale;
		float2 l_24 = g_vMudUVOffset;
		float2 l_25 = TileAndOffsetUv( i.vTextureCoords.xy, l_23, l_24 );
		float4 l_26 = g_tMudAmbientOcclusion.Sample( g_sSampler_9E3605E1,l_25 );
		
		m.Albedo = l_9.xyz;
		m.Opacity = 1;
		m.Normal = l_18.xyz;
		m.Roughness = l_22.x;
		m.Metalness = 0;
		m.AmbientOcclusion = l_26.x;
		
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
				
		return ShadingModelStandard::Shade( m );
	}
}
