using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using System;
using static Unity.Mathematics.math;

public class RepulsionSystem : SystemBase
{
    private EntityQuery query;

    protected override void OnCreate()
    {
        // 略記も可
        query = GetEntityQuery(typeof(Translation), typeof(Force));
    }

    protected override void OnUpdate()
    {
        // config
        RepulsionConfig config = new RepulsionConfig();
        Entities.ForEach((in RepulsionConfig c) => config = c).Run();

        // elements 収集
        int dataCount = query.CalculateEntityCount();

        var wh = config.boardSize*2;
        var quadTree = new NativeQuadTree.NativeQuadTree<int>(
            new NativeQuadTree.AABB2D(wh, wh), Allocator.TempJob);
        var elements = new NativeArray<NativeQuadTree.QuadElement<int>>(
            dataCount, Allocator.TempJob);

        Entities.ForEach(
            (int entityInQueryIndex, in Translation t, in Force f) => {
                elements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xz + float2(config.boardSize),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .ScheduleParallel();

        // Debug.Log($"{chunk.Count}, {translations.Length}, {chunkIndex}, {firstEntityIndex}");

        // insert to quadtree
        Job.WithCode(
            () => {
                quadTree.ClearAndBulkInsert(elements);
            })
            .Schedule();

        // main process
        var totalThresholdSq = lengthsq(config.totalThreshold);

        var forces = query.ToComponentDataArray<Force>(Allocator.TempJob);

        Job.WithCode(
            () => {
                var results = new NativeList<NativeQuadTree.QuadElement<int>>(
                    dataCount, Allocator.Temp);
                for (int i = 0 ; i < elements.Length ; i++) { 
                    results.Clear();
                    var bounds = new NativeQuadTree.AABB2D(
                        elements[i].pos, 
                        float2(config.totalThreshold));
                    quadTree.RangeQuery(bounds, results);
                
                    float2 p = elements[i].pos;
                    for (int jj = 0 ; jj < results.Length ; jj++) {
                        int j = results[jj].element;
                        if (j <= i) { continue; }
                        float2 q = elements[j].pos;
                    
                        float2 d = q - p;
                        float distsq = lengthsq(d);
                        if (distsq < totalThresholdSq) {
                            var rd = lengthsq(
                                config.totalThreshold - sqrt(distsq));
                        
                            float2 v = d * (rd * config.forceFactor);
                            float activeWeight = 1.0f;
                            float passiveWeight = 1.0f;
                            float denominator = activeWeight + passiveWeight;
                            if (0 < denominator) {
                                float2 vv = v / denominator;
                                float3 vvv = float3(v.x, 0, v.y);
                                forces[i] += vvv * -passiveWeight;
                                forces[j] += vvv * activeWeight;
                            }
                        }
                    }
                }
                results.Dispose();
            })
            // .WithoutBurst()
            .Schedule();
        
        CompleteDependency();
        query.CopyFromComponentDataArray(forces);

        forces.Dispose();
        elements.Dispose();
        quadTree.Dispose();
    }
}
