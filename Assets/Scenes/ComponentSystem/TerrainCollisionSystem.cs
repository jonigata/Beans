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
    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();
        if (config.sdfTexture == null) { return; }
        
        Entities
            .ForEach((ref Translation translation) =>
            {
                ApplyWall(ref translation, config);
            })
            .WithoutBurst()
            .Run();
    }

    static void ApplyWall(ref Translation translation, BoardConfig config) {
        var uv = GetUVFromLocalPoint(config.size, translation.Value);
        var z = Pick(config.sdfTexture, uv);

        if (0.5f < z) {
            // do nothing
        } else {
            // Debug.Log($"outside {z}");
            var g = Gradient(config.sdfTexture, uv);
            var location = GetLocalPointFromUV(
                config.size, uv + g * (0.5f - z));
            translation.Value = location;
        }
    }
    
    static float2 Gradient(Texture2D texture, float2 uv) {
        float delta = 0.01f;
        var dis0 = Pick(texture, float2(uv.x + delta, uv.y));
        var dis1 = Pick(texture, float2(uv.x - delta, uv.y));
        var dis2 = Pick(texture, float2(uv.x, uv.y + delta));
        var dis3 = Pick(texture, float2(uv.x, uv.y - delta));
        // Debug.Log($"{dis0} {dis1} {dis2} {dis3}");
        return normalizesafe(float2(dis0-dis1, dis2-dis3));
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
