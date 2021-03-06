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
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .ForEach((ref Translation translation, ref Force force) =>
            {
                translation.Value = translation.Value + force.Value * deltaTime;
                force.Value = float3(0, 0, 0);
            })
           .ScheduleParallel();
   }
}
