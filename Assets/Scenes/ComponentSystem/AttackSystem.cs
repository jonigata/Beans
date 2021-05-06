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

[UpdateInGroup(typeof(NearestNeighborGroup))]
public class AttackSystem : SystemBase
{
    EntityQuery query;
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate() {
        query = GetEntityQuery(typeof(Pawn));
        entityCommandBufferSystem =
            World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();

        // elements 収集
        var quadTree = config.quadTree.content;
        var elements = config.quadTree.elements;

        // main process
        var totalThresholdSq = lengthsq(config.neighborThreshold);
        var neighborThreshold = config.neighborThreshold;
        var forceFactor = config.forceFactor;

        var pawns = query.ToComponentDataArray<Pawn>(Allocator.TempJob);
        var deltaTime = Time.DeltaTime;

        Job.WithCode(
            () => {
                var results = new NativeList<NativeQuadTree.QuadElement<int>>(
                    elements.Length, Allocator.Temp);
                for (int i = 0 ; i < elements.Length ; i++) { 
                    results.Clear();
                    var bounds = new NativeQuadTree.AABB2D(
                        elements[i].pos, 
                        float2(neighborThreshold));
                    quadTree.RangeQuery(bounds, results);
                    
                    float2 p = elements[i].pos;
                    int k = -1;
                    float nearestDistSq = totalThresholdSq;
                    for (int jj = 0 ; jj < results.Length ; jj++) {
                        int j = results[jj].element;
                        if (pawns[i].Team == pawns[j].Team) { continue; }

                        float2 q = elements[j].pos;
                    
                        float2 d = q - p;
                        float distSq = lengthsq(d);
                        if (distSq < nearestDistSq) {
                            nearestDistSq = distSq;
                            k = j;
                        }
                    }

                    if (0 <= k) {
                        Pawn active = pawns[i];
                        active.HitEffectInterval += deltaTime;
                        active.HitEffectPosition = elements[k].pos;
                        pawns[i] = active;
                        Pawn passive = pawns[k];
                        passive.Health -= 30.0f * deltaTime;
                        pawns[k] = passive;
                    }
                }
                results.Dispose();
            })
            // .WithoutBurst()
            .Schedule();
        
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();        
        Entities.ForEach(
            (Entity entity, int entityInQueryIndex, in Pawn p) => {
                if (p.Health <= 0) { 
                    commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                }
            })
            .ScheduleParallel();
        
        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        CompleteDependency();
        query.CopyFromComponentDataArray(pawns);
        pawns.Dispose();
    }
}
