using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System;
using static Unity.Mathematics.math;

public class ForceSystem : SystemBase
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        // 略記も可
        query = GetEntityQuery(typeof(Translation), typeof(Force));
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        NativeArray<Translation> translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Force> forces = query.ToComponentDataArray<Force>(Allocator.TempJob);

        var totalThreshold = 0.4f;
        var totalThresholdSq = lengthsq(totalThreshold);

        for (int i = 0; i < translations.Length; i++) {
            for (int j = i+1 ; j < translations.Length ; j++) {
                var p = translations[i].Value;
                var q = translations[j].Value;
                var d = q - p;
                var distsq = lengthsq(d);
                if (distsq < totalThresholdSq) {
                    var rd = lengthsq(totalThreshold - sqrt(distsq));

                    var v = d * (rd * 120.0f);
                    var activeWeight = 1.0f;
                    var passiveWeight = 1.0f;
                    var denominator = activeWeight + passiveWeight;
                    if (0 < denominator) {
                        v = v / denominator;
                        forces[i] += v * -passiveWeight;
                        forces[j] += v * activeWeight;
                    }
                }
            }
        }

        query.CopyFromComponentDataArray(forces);

        translations.Dispose();
        forces.Dispose();

        Entities
            .ForEach((ref Translation translation, ref Force force) =>
            {
                translation.Value = translation.Value + force.Value * deltaTime;
                force.Value = float3(0, 0, 0);
            })
           .ScheduleParallel();
   }
}
