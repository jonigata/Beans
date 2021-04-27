using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System;
using static Unity.Mathematics.math;

public class HeightFallSystem : SystemBase
{
    protected override void OnUpdate() {
        Entities
            .WithoutBurst()
            .ForEach((ref Force force, 
                      in Translation translation, 
                      in HeightMap heightMap) =>
            {
                ApplyFall(ref force, in translation, in heightMap);
            })
            .Run();
    }

    void ApplyFall(
        ref Force force,
        in Translation translation,
        in HeightMap heightMap) {
        var t = heightMap.heightMap;
        if (t == null) { return; }

        var uv = GetUVFromLocalPoint(heightMap.size, translation.Value);

        var g = Gradient(t, uv);
        force.Value = force.Value + float3(g.x, 0, g.y) * -10;
    }
    
    float2 Gradient(Texture2D texture, float2 uv) {
        float delta = 0.01f;
        var dis0 = Pick(texture, float2(uv.x + delta, uv.y));
        var dis1 = Pick(texture, float2(uv.x - delta, uv.y));
        var dis2 = Pick(texture, float2(uv.x, uv.y + delta));
        var dis3 = Pick(texture, float2(uv.x, uv.y - delta));
        // Debug.Log($"{dis0} {dis1} {dis2} {dis3}");
        return float2(dis0-dis1, dis2-dis3);
    }

    float Pick(Texture2D texture, float2 uv) {
        return texture.GetPixelBilinear(uv.x, uv.y).a;
    }

    float2 GetUVFromLocalPoint(float2 size, float3 localPoint) {
        float2 p = float2(localPoint.x, localPoint.z);
        return p / size + 0.5f;
    }

    float3 GetLocalPointFromUV(float2 size, float2 uv) {
        float2 p = uv * size - (size * 0.5f);
        return float3(p.x, 0, p.y);
    }

}
