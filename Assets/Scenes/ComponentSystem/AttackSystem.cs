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
    EntityQuery alphaQuery;
    EntityQuery betaQuery;
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate() {
        alphaQuery = GetEntityQuery(typeof(Pawn), typeof(Alpha));
        betaQuery = GetEntityQuery(typeof(Pawn), typeof(Beta));
        entityCommandBufferSystem =
            World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();

        // elements 収集
        var alphaTree = config.quadTree.alphaTree;
        var betaTree = config.quadTree.betaTree;
        var alphaElements = config.quadTree.alphaElements;
        var betaElements = config.quadTree.betaElements;

        // main process
        var totalThresholdSq = lengthsq(config.neighborThreshold);
        var neighborThreshold = config.neighborThreshold;
        var forceFactor = config.forceFactor;
        var extent = float2(neighborThreshold);

        var alphaPawns = alphaQuery.ToComponentDataArray<Pawn>(Allocator.TempJob);
        var betaPawns = betaQuery.ToComponentDataArray<Pawn>(Allocator.TempJob);
        var deltaTime = Time.DeltaTime;

        Job.WithCode(
            () => {
                for (int t = 0 ; t < 2 ; t++) {
                    var activeElements = t == 0 ? alphaElements : betaElements;
                    var passiveElements = t == 0 ? betaElements : alphaElements;
                    var quadTree = t == 0 ? betaTree : alphaTree;
                    var activePawns = t == 0 ? alphaPawns : betaPawns;
                    var passivePawns = t == 0 ? betaPawns : alphaPawns;

                    var results = new NativeList<NativeQuadTree.QuadElement<int>>(
                        activeElements.Length, Allocator.Temp);
                    for (int i = 0 ; i < activeElements.Length ; i++) { 
                        results.Clear();
                        float2 p = activeElements[i].pos;
                        var bounds = new NativeQuadTree.AABB2D(p, extent);
                        quadTree.RangeQuery(bounds, results);
                    
                        int k = -1;
                        float nearestDistSq = totalThresholdSq;
                        float2 nearestPosition = new float2();
                        for (int jj = 0 ; jj < results.Length ; jj++) {
                            int j = results[jj].element;

                            float2 q = passiveElements[j].pos;
                    
                            float2 d = q - p;
                            float distSq = lengthsq(d);
                            if (distSq < nearestDistSq) {
                                nearestDistSq = distSq;
                                nearestPosition = q;
                                k = j;
                            }
                        }

                        if (0 <= k) {
                            Pawn active = activePawns[i];
                            active.HitEffectInterval += deltaTime;
                            active.HitEffectPosition = nearestPosition;
                            activePawns[i] = active;
                            Pawn passive = passivePawns[k];
                            passive.Health -= 30.0f * deltaTime;
                            passivePawns[k] = passive;
                        }
                    }
                    results.Dispose();
                }
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
        alphaQuery.CopyFromComponentDataArray(alphaPawns);
        betaQuery.CopyFromComponentDataArray(betaPawns);
        alphaPawns.Dispose();
        betaPawns.Dispose();
    }
}
