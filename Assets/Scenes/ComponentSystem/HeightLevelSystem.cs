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
        HeightMap heightMap = new HeightMap();
        Entities.ForEach(
            (in HeightMap h) => heightMap = h)
            .WithoutBurst()
            .Run();
        var t = heightMap.heightMap;
        if (t == null) { return; }

        Entities
            .WithoutBurst()
            .ForEach((ref Translation translation) =>
            {
                ApplyLevel(ref translation, heightMap);
            })
            .Run();
    }

    static void ApplyLevel(
        ref Translation translation, HeightMap heightMap) {

        var t = heightMap.heightMap;
        var uv = GetUVFromLocalPoint(heightMap.size, translation.Value);
        var level = Pick(t, uv) * heightMap.heightScale;
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
