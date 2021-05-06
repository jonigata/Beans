using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using System;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(NearestNeighborGroup))]
public class RepulsionSystem : SystemBase
{
    private EntityQuery query;

    protected override void OnCreate() {
        query = GetEntityQuery(typeof(Translation), typeof(Force));
    }

    protected override void OnUpdate()
    {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();

        var quadTree = config.quadTree.allTree;
        var elements = config.quadTree.allElements;

        // main process
        var totalThresholdSq = lengthsq(config.neighborThreshold);
        var neighborThreshold = config.neighborThreshold;
        var forceFactor = config.forceFactor;

        var forces = query.ToComponentDataArray<Force>(Allocator.TempJob);

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
                    for (int jj = 0 ; jj < results.Length ; jj++) {
                        int j = results[jj].element;
                        if (j <= i) { continue; }
                        float2 q = elements[j].pos;
                    
                        float2 d = q - p;
                        float distsq = lengthsq(d);
                        if (distsq < totalThresholdSq) {
                            var rd = lengthsq(neighborThreshold - sqrt(distsq));
                        
                            float2 v = d * (rd * forceFactor);
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
    }
}
