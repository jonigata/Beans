using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System;
using static Unity.Mathematics.math;

public class HeightLevelSystem : SystemBase
{
    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();
        if (config.heightMap == null) { return; }

        Entities
            .ForEach((ref Translation translation) =>
            {
                ApplyLevel(ref translation, config);
            })
            .WithoutBurst()
            .Run();
    }

    static void ApplyLevel(ref Translation translation, BoardConfig config) {
        var t = config.heightMap;
        var uv = GetUVFromLocalPoint(config.size, translation.Value);
        var level = Pick(t, uv) * config.heightScale;
        translation.Value = float3(
            translation.Value.x, 
            level + 0.2f,
            translation.Value.z); 
    }
    
    static float Pick(Texture2D texture, float2 uv) {
        return texture.GetPixelBilinear(uv.x, uv.y).a;
    }

    static float2 GetUVFromLocalPoint(float2 size, float3 localPoint) {
        float2 p = float2(localPoint.x, localPoint.z);
        return p / size + 0.5f;
    }

    static float3 GetLocalPointFromUV(float2 size, float2 uv) {
        float2 p = uv * size - (size * 0.5f);
        return float3(p.x, 0, p.y);
    }
}
