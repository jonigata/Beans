using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using System;

[GenerateAuthoringComponent]
public struct BoardConfig : ISharedComponentData, IEquatable<BoardConfig>
{
    public class QuadTree {
        public NativeQuadTree.NativeQuadTree<int> allTree;
        public NativeQuadTree.NativeQuadTree<int> alphaTree;
        public NativeQuadTree.NativeQuadTree<int> betaTree;
        public NativeArray<NativeQuadTree.QuadElement<int>> allElements;
        public NativeArray<NativeQuadTree.QuadElement<int>> alphaElements;
        public NativeArray<NativeQuadTree.QuadElement<int>> betaElements;
    }

    public float2 size;
    public Texture2D sdfTexture;
    public Texture2D heightMap;
    public float heightScale;
    public float neighborThreshold;
    public float forceFactor;
    public QuadTree quadTree;

    public bool Equals(BoardConfig c) {
        if (!size.Equals(c.size)) return false;
        if (sdfTexture != c.sdfTexture) return false;
        if (heightMap != c.heightMap) return false;
        if (heightScale.Equals(c.heightScale)) return false;
        if (neighborThreshold.Equals(c.neighborThreshold)) return false;
        if (forceFactor.Equals(c.forceFactor)) return false;
        if (quadTree != c.quadTree) { return false; }
        return true;
    }

    public int GetHashCode() {
        return
            size.GetHashCode() ^
            sdfTexture.GetHashCode() ^
            heightMap.GetHashCode() ^
            heightScale.GetHashCode() ^
            neighborThreshold.GetHashCode() ^
            forceFactor.GetHashCode() ^
            quadTree.allTree.GetHashCode() ^
            quadTree.alphaTree.GetHashCode() ^
            quadTree.betaTree.GetHashCode() ^
            quadTree.allElements.GetHashCode() ^
            quadTree.alphaElements.GetHashCode() ^
            quadTree.betaElements.GetHashCode()
            ;
    }
}
