using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using System;
using static Unity.Mathematics.math;

public class NearestNeighborGroup : ComponentSystemGroup {}

[UpdateBefore(typeof(NearestNeighborGroup))]
public class NearestNeighborSystem : SystemBase {
    private EntityQuery query;

    protected override void OnCreate() {
        query = GetEntityQuery(typeof(Translation), typeof(Force));
    }

    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();

        // elements 収集
        int dataCount = query.CalculateEntityCount();

        var size = config.size;
        var size2 = size*2;
        var elements = new NativeArray<NativeQuadTree.QuadElement<int>>(
            dataCount, Allocator.TempJob);

        Entities.ForEach(
            (int entityInQueryIndex, in Translation t, in Force f) => {
                elements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xz + float2(size),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .ScheduleParallel();

        Entities.ForEach(
            (int entityInQueryIndex, in Translation t, in Force f) => {
                elements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xz + float2(size),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .ScheduleParallel();

        // Debug.Log($"{chunk.Count}, {translations.Length}, {chunkIndex}, {firstEntityIndex}");

        // insert to quadtree
        var quadTree = new NativeQuadTree.NativeQuadTree<int>(
            new NativeQuadTree.AABB2D(size2, size2), Allocator.TempJob);
        Job.WithCode(
            () => {
                quadTree.ClearAndBulkInsert(elements);
            })
            .Schedule();
        
        Entities.ForEach(
            (BoardConfig c) => {
                c.quadTree.content = quadTree;
                c.quadTree.elements = elements;
            })
            .WithoutBurst()
            .Run();
    }
}

[UpdateAfter(typeof(NearestNeighborGroup))]
public class NearestNeighborCleanUpSystem : SystemBase {
    protected override void OnUpdate() {
        Entities.ForEach(
            (BoardConfig c) => {
                c.quadTree.content.Dispose();
                c.quadTree.elements.Dispose();
            })
            .WithoutBurst()
            .Run();
    }
}
