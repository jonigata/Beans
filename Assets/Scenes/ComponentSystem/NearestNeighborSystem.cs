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
    private EntityQuery allQuery;
    private EntityQuery alphaQuery;
    private EntityQuery betaQuery;

    protected override void OnCreate() {
        allQuery = GetEntityQuery(typeof(Force));
        alphaQuery = GetEntityQuery(typeof(Force), typeof(Alpha));
        betaQuery = GetEntityQuery(typeof(Force), typeof(Beta));
    }

    protected override void OnUpdate() {
        // config
        BoardConfig config = new BoardConfig();
        Entities.ForEach((in BoardConfig c) => config = c).WithoutBurst().Run();

        // elements 収集
        int allCount = allQuery.CalculateEntityCount();
        int alphaCount = alphaQuery.CalculateEntityCount();
        int betaCount = betaQuery.CalculateEntityCount();

        var size = config.size;
        var size2 = size*2;
        var allElements = new NativeArray<NativeQuadTree.QuadElement<int>>(
            allCount, Allocator.TempJob);
        var alphaElements = new NativeArray<NativeQuadTree.QuadElement<int>>(
            alphaCount, Allocator.TempJob);
        var betaElements = new NativeArray<NativeQuadTree.QuadElement<int>>(
            betaCount, Allocator.TempJob);

        Entities.ForEach(
            (int entityInQueryIndex, in Translation t, in Force f) => {
                allElements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xz + float2(size),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .ScheduleParallel();

        Entities.ForEach(
            (int entityInQueryIndex, in Alpha a, in Translation t, in Force f) => {
                alphaElements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xz + float2(size),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .ScheduleParallel();

        Entities.ForEach(
            (int entityInQueryIndex, in Beta a, in Translation t, in Force f) => {
                betaElements[entityInQueryIndex] = 
                    new NativeQuadTree.QuadElement<int> {
                    pos = t.Value.xz + float2(size),
                    element = entityInQueryIndex
                };
            })
            // .WithoutBurst()
            .ScheduleParallel();

        // Debug.Log($"{chunk.Count}, {translations.Length}, {chunkIndex}, {firstEntityIndex}");

        // insert to quadtree
        CompleteDependency();
        var allTree = new NativeQuadTree.NativeQuadTree<int>(
            new NativeQuadTree.AABB2D(size2, size2), Allocator.TempJob);
        var alphaTree = new NativeQuadTree.NativeQuadTree<int>(
            new NativeQuadTree.AABB2D(size2, size2), Allocator.TempJob);
        var betaTree = new NativeQuadTree.NativeQuadTree<int>(
            new NativeQuadTree.AABB2D(size2, size2), Allocator.TempJob);
        Job.WithCode(
            () => {
                allTree.ClearAndBulkInsert(allElements);
                alphaTree.ClearAndBulkInsert(alphaElements);
                betaTree.ClearAndBulkInsert(betaElements);
            })
            .Schedule();
        
        CompleteDependency();

        Entities.ForEach(
            (BoardConfig c) => {
                c.quadTree.allTree = allTree;
                c.quadTree.alphaTree = alphaTree;
                c.quadTree.betaTree = betaTree;
                c.quadTree.allElements = allElements;
                c.quadTree.alphaElements = alphaElements;
                c.quadTree.betaElements = betaElements;
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
                c.quadTree.allTree.Dispose();
                c.quadTree.alphaTree.Dispose();
                c.quadTree.betaTree.Dispose();
                c.quadTree.allElements.Dispose();
                c.quadTree.alphaElements.Dispose();
                c.quadTree.betaElements.Dispose();
            })
            .WithoutBurst()
            .Run();
    }
}
