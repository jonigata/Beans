using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System;
using static Unity.Mathematics.math;

public class TerrainCollisionSystem : SystemBase
{
    EntityQuery query;

    protected override void OnCreate() {
        query = GetEntityQuery(typeof(Translation), typeof(TerrainCollision));
    }

    protected override void OnUpdate() {
        Entities
            .WithoutBurst()
            .ForEach((ref Translation translation, in TerrainCollision terrain) =>
            {
                ApplyWall(ref translation, in terrain);
            })
            .Run();
    }

    void ApplyWall(ref Translation translation, in TerrainCollision terrain) {
        var t = terrain.sdfTexture;
        var uv = GetUVFromLocalPoint(terrain.size, translation.Value);
        var z = Pick(t, uv);

        if (0.5f < z) {
            // do nothing
        } else {
            // Debug.Log($"outside {z}");
            var g = Gradient(t, uv);
            var location = GetLocalPointFromUV(
                terrain.size, uv + g * (0.5f - z));
            translation.Value = location;
        }
    }
    
    float2 Gradient(Texture2D texture, float2 uv) {
        float delta = 0.01f;
        var dis0 = Pick(texture, float2(uv.x + delta, uv.y));
        var dis1 = Pick(texture, float2(uv.x - delta, uv.y));
        var dis2 = Pick(texture, float2(uv.x, uv.y + delta));
        var dis3 = Pick(texture, float2(uv.x, uv.y - delta));
        // Debug.Log($"{dis0} {dis1} {dis2} {dis3}");
        return normalize(float2(dis0-dis1, dis2-dis3));
    }

    float Pick(Texture2D texture, float2 uv) {
        return texture.GetPixelBilinear(uv.x, uv.y).a;
    }

    float2 GetUVFromLocalPoint(float2 size, float3 localPoint) {
        float2 p = float2(localPoint.x, localPoint.y);
        return p / size + 0.5f;
    }

    float3 GetLocalPointFromUV(float2 size, float2 uv) {
        float2 p = uv * size - (size * 0.5f);
        return float3(p.x, p.y, 0);
    }

}
