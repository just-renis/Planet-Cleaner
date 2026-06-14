HEADER
{
    Description = "Custom Smoke Particle Shader";
}

MODES
{
    Default();
    VrForward();
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
    #include "common/shared.hlsl"
}

struct VertexInput
{
    #include "common/vertexinput.hlsl"
};

struct PixelInput
{
    #include "common/pixelinput.hlsl"
};

VS
{
    #include "common/vertex.hlsl"

    PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
    {
        PixelInput o = ProcessVertex( i );
        return FinalizeVertex( o );
    }
}

PS
{
    #include "common/pixel.hlsl"

    // Set up translucent blending and disable depth writing so smoke layers correctly
    RenderState( BlendEnable, true );
    RenderState( SrcBlend, SRC_ALPHA );
    RenderState( DestBlend, INV_SRC_ALPHA );
    RenderState( ZWriteEnable, false ); 

    // Define the smoke texture property that will show up in the Material Editor
    CreateTexture2D( g_tSmokeTex ) < Attribute( "SmokeTexture" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        // Sample the smoke texture using the particle's UVs
        float4 texColor = Tex2DS( g_tSmokeTex, TextureFiltering, i.vTextureCoords.xy );

        // i.vColor comes from the Particle Emitter. We multiply the texture by this 
        // to inherit the particle's tint and fade-out animations.
        float4 finalColor = texColor * i.vColor;

        return finalColor;
    }
}