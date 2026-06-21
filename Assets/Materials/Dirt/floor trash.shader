
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
		#define S_ALPHA_TEST 1
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
	
	SamplerState g_sSampler_CA6AEA38 < Filter( ANISOTROPIC ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 16 ); >;
	SamplerState g_sSampler_D4B8677D < Filter( BILINEAR ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", "Texture,0/,0/0", DefaultFile( "materials/dirt/landfill_seamless.jpg" ) );
	CreateInputTexture2D( DetailMask, Srgb, 8, "None", "_color", "Detail Texture,0/,0/0", DefaultFile( "materials/dirt/landfill_seamless_detail_mask.jpg" ) );
	CreateInputTexture2D( Mask, Srgb, 8, "None", "_mask", "Texture,0/,0/1", DefaultFile( "materials/dirt/landfill_seamless_mask.jpg" ) );
	Texture2D g_tColor < Channel( RGBA, Box( Color ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tDetailMask < Channel( RGBA, Box( DetailMask ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	Texture2D g_tMask < Channel( RGBA, Box( Mask ), Linear ); OutputFormat( DXT1 ); SrgbRead( False ); >;
	TextureAttribute( LightSim_DiffuseAlbedoTexture, g_tColor )
	TextureAttribute( RepresentativeTexture, g_tColor )
	float g_flTextureScale < UiStep( 1 ); UiGroup( "Texture,0/,0/2" ); Default1( 1 ); Range1( 0, 100 ); >;
	float2 g_vTextureOffset < UiGroup( "Texture,0/,0/3" ); Default2( 1,1 ); Range2( 0,0, 10,10 ); >;
	float g_flDetailTextureScale < UiStep( 1 ); UiGroup( "Detail Texture,0/,0/1" ); Default1( 1 ); Range1( 0, 100 ); >;
	float2 g_vDetailTextureOffset < UiGroup( "Detail Texture,0/,0/2" ); Default2( 1,1 ); Range2( 0,0, 10,10 ); >;
		
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
		
		float l_0 = g_flTextureScale;
		float2 l_1 = g_vTextureOffset;
		float2 l_2 = TileAndOffsetUv( i.vTextureCoords.xy, float2( l_0, l_0 ), l_1 );
		float4 l_3 = g_tColor.Sample( g_sSampler_CA6AEA38,l_2 );
		float l_4 = g_flDetailTextureScale;
		float2 l_5 = g_vDetailTextureOffset;
		float2 l_6 = TileAndOffsetUv( i.vTextureCoords.xy, float2( l_4, l_4 ), l_5 );
		float4 l_7 = g_tDetailMask.Sample( g_sSampler_D4B8677D,l_6 );
		float4 l_8 = g_tMask.Sample( g_sSampler_CA6AEA38,l_2 );
		float4 l_9 = lerp( l_7, (l_7) + (l_8) - (l_7) * (l_8), 1 );
		
		m.Albedo = l_3.xyz;
		m.Opacity = l_9.x;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		
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
