using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using System;

public class JitterSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
        
        Entities
            .WithNativeDisableParallelForRestriction(randomArray)
            .ForEach((int nativeThreadIndex, ref Translation translation, in Jitter jitter) =>
            {
                var random = randomArray[nativeThreadIndex];
                float3 move = float3.zero;
                switch(random.NextInt(4)) {
                    case 0: move.x = -1;    break;
                    case 1: move.x = 1;     break;
                    case 2: move.y = -1;    break;
                    case 3: move.y = 1;     break;
                }

                translation.Value = translation.Value + move * jitter.speed * deltaTime;
            })
           .ScheduleParallel();
   }
}
