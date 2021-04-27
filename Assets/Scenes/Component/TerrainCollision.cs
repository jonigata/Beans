using UnityEngine;
using Unity.Entities;
using System;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TerrainCollision : ISharedComponentData, IEquatable<TerrainCollision>
{
    public Texture2D sdfTexture;
    public float2 size;

    public bool Equals(TerrainCollision t) {
        if (sdfTexture != t.sdfTexture) return false;
        if (!size.Equals(t.size)) return false;
        return true;
    }

    public int GetHashCode() {
        if (sdfTexture == null) { return 0; }
        return sdfTexture.GetHashCode() ^ size.GetHashCode();
    }
}
