using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using System;
using System.Collections.Generic;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(AttackSystem))]
public class HitEffectSystem : SystemBase
{
    public static List<float2> hitEffects = new List<float2>();
    
    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();
        var size = float2(config.size);

        Entities.ForEach(
            (Entity entity, int entityInQueryIndex, ref Pawn p) => {
                if (2.0f <= p.HitEffectInterval)  {
                    hitEffects.Add(p.HitEffectPosition - size);
                    p.HitEffectInterval = 0;
                }
            })
            .WithoutBurst()
            .Run();
    }
}
