using UnityEngine;
using Unity.Entities;
using System;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct HeightMap : ISharedComponentData, IEquatable<HeightMap>
{
    public Texture2D heightMap;
    public float2 size;

    public bool Equals(HeightMap h) {
        if (heightMap != h.heightMap) return false;
        if (!size.Equals(h.size)) return false;
        return true;
    }

    public int GetHashCode() {
        if (heightMap == null) { return 0; }
        return heightMap.GetHashCode() ^ size.GetHashCode();
    }
}
