#define USE_NATIVEQUADTREE

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

#if USE_NATIVEQUADTREE
    protected override void OnUpdate()
    {
        int dataCount = query.CalculateEntityCount();

        var quadTree = new NativeQuadTree.NativeQuadTree<int>(
            new NativeQuadTree.AABB2D(10.24f, 10.24f));
        var elements = new NativeArray<NativeQuadTree.QuadElement<int>>(
            dataCount, Allocator.Temp);

        // elements 収集
        Entities.ForEach(
            (int entityInQueryIndex, in Translation t, in Force f) => {
                elements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xy + float2(5.12f, 5.12f),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .Run();

        // Debug.Log($"{chunk.Count}, {translations.Length}, {chunkIndex}, {firstEntityIndex}");

        // main process
        var forces = query.ToComponentDataArray<Force>(Allocator.Temp);
        Job.WithCode(
            () => {
                quadTree.ClearAndBulkInsert(elements);

                var totalThreshold = 0.3f;
                var totalThresholdSq = lengthsq(totalThreshold);

                var results = new NativeList<NativeQuadTree.QuadElement<int>>(
                    dataCount, Allocator.Temp);
                for (int i = 0; i < elements.Length ; i++) {
                    results.Clear();
                    var bounds = new NativeQuadTree.AABB2D(
                        elements[i].pos, 
                        float2(totalThreshold));
                    quadTree.RangeQuery(bounds, results);
                
                    var p = elements[i].pos.xy;
                    for (int jj = 0 ; jj < results.Length ; jj++) {
                        int j = results[jj].element;
                        if (j <= i) { continue; }
                        var q = elements[j].pos;
                    
                        var d = q - p;
                        var distsq = lengthsq(d);
                        if (distsq < totalThresholdSq) {
                            var rd = lengthsq(totalThreshold - sqrt(distsq));
                        
                            var v = d * (rd * 120.0f);
                            var activeWeight = 1.0f;
                            var passiveWeight = 1.0f;
                            var denominator = activeWeight + passiveWeight;
                            if (0 < denominator) {
                                var vv = float3(v / denominator, 0);
                                forces[i] += vv * -passiveWeight;
                                forces[j] += vv * activeWeight;
                            }
                        }
                    }
                }
            })
            // .WithoutBurst()
            .Run();
        query.CopyFromComponentDataArray(forces);

        elements.Dispose();
    }


#else

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
   }

#endif
}
