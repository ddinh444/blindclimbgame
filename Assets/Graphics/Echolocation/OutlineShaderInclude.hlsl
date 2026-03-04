struct Kernel{
    float3x3 x;
    float3x3 y;
};

struct AudioSourceData{
    float3 position;
    float radius;
    float strength;
};

StructuredBuffer<AudioSourceData> _AudioSources;

#define MAX_AUDIO_SOURCES 13

float LinearSampleDepthBuffer(float2 UV)
{
    return Linear01Depth(SHADERGRAPH_SAMPLE_SCENE_DEPTH(UV), _ZBufferParams);
}

float3 SampleNormalBuffer(float2 UV){
    return SHADERGRAPH_SAMPLE_SCENE_NORMAL(UV);
}

Kernel GetEdgeDetectionKernels(){
    Kernel kernel;
    kernel.x = float3x3(-3,-10,-3,0,0,0,3,10,3);
    kernel.y = float3x3(-3,0,3,-10,0,10,-3,0,3); 
    return kernel;
}

void DepthBasedOutlines_float(float4 UV, float2 maxPxOffset, out float Out){
    Kernel kernels = GetEdgeDetectionKernels();
    float gx = 0;
    float gy = 0;
    for(int i = -1; i <= 1; i++){
        for(int j = -1; j <= 1; j++){
            if(i == 0 && j == 0){
                continue;
            }
            float2 offset = maxPxOffset * float2(i,j);
            float4 sampleUV = UV + float4(offset,0,0);
            float d = LinearSampleDepthBuffer(sampleUV);
            gx += d * kernels.x[i+1][j+1];
            gy += d * kernels.y[i+1][j+1];
        }
    }
    float g = (gx * gx + gy * gy);

    /*
    float d = SampleDepthBuffer(UV);
    float dx = ddx(d);
    float dy = ddy(d);
    float g = dx + dy;
    */

    Out = smoothstep(0.00009, 0.00011,g);
    //Out = smoothstep(0.01, 0.03, g);
}

void NormalBasedOutlines_float(float4 UV, float2 maxPxOffset, out float Out){
    Kernel kernels = GetEdgeDetectionKernels();
    float gx = 0;
    float gy = 0;
    float3 centerNormal = SampleNormalBuffer(UV.xy);
    for(int i = -1; i <= 1; i++){
        for(int j = -1; j <= 1; j++){
            if(i == 0 && j == 0){
                continue;
            }
            float2 offset = maxPxOffset * float2(i,j);
            float2 sampleUV = UV.xy + offset;
            float3 normal = SampleNormalBuffer(sampleUV);
            float dp = dot(centerNormal, normal);
            gx += dp * kernels.x[i+1][j+1];
            gy += dp * kernels.y[i+1][j+1];
        }
    }
    float g = (gx * gx + gy * gy);

    /*
    float3 dx = ddx(centerNormal);
    float3 dy = ddy(centerNormal);
    float g = dot(dx,dx) + dot(dy,dy);
    */
    Out = smoothstep(1.5f, 2.25f,sqrt(g));
}

void RobertsCrossNormalBasedOutlines(float2 UV, float2 maxPxOffset, out float Out){
    float2 UVs[4];
    UVs[0] = UV + maxPxOffset * float2(-1,-1);
    UVs[1] = UV + maxPxOffset * float2(1,-1);
    UVs[2] = UV + maxPxOffset * float2(-1,1);
    UVs[3] = UV + maxPxOffset * float2(1,1);

    float3 normals[4];
    [unroll]
    for(int i = 0; i < 4; i++){
        normals[i] = SampleNormalBuffer(UVs[i]);
    }

    float3 diff1 = normals[1] - normals[2];
    float3 diff2 = normals[0] - normals[3];

    Out = step(0.25,sqrt(dot(diff1, diff1) + dot(diff2,diff2)));
}

void RobertsCrossDepthBasedOutlines(float2 UV, float2 maxPxOffset, out float Out){
    float2 UVs[4];
    UVs[0] = UV + maxPxOffset * float2(-1,-1);
    UVs[1] = UV + maxPxOffset * float2(1,-1);
    UVs[2] = UV + maxPxOffset * float2(-1,1);
    UVs[3] = UV + maxPxOffset * float2(1,1);

    float depths[4];
    [unroll]
    for(int i = 0; i < 4; i++){
        depths[i] = LinearSampleDepthBuffer(UVs[i]);
    }

    float diff1 = depths[1] - depths[2];
    float diff2 = depths[0] - depths[3];

    Out = step(0.01,sqrt(diff1 * diff1 + diff2 * diff2));
}


void CalculateOutlinesWithAudioSourceMasks_float(float3 wsPos, float4 UV, float2 maxPxOffset, out float Out){
    float accumEchoStrength = 0;
    [unroll]
    for(int i = 0; i < MAX_AUDIO_SOURCES; i++){
        AudioSourceData src = _AudioSources[i];
        //get the distance from all active audio sources. If in range, add to accumulated outline strength
        float3 diff = wsPos - src.position;
        float distSq = dot(diff, diff);
        float radiusSq = src.radius * src.radius;

        float mask = step(distSq, radiusSq);
        accumEchoStrength += mask * src.strength;
    }

    float normalOut = 0, depthOut = 0;
    RobertsCrossNormalBasedOutlines(UV, maxPxOffset, normalOut);
    RobertsCrossDepthBasedOutlines(UV, maxPxOffset, depthOut);

    Out = accumEchoStrength * (normalOut + depthOut);
}