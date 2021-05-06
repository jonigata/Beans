using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public enum TeamTag {
    Alpha,
    Beta,
}

[GenerateAuthoringComponent]
public struct Pawn : IComponentData
{
    public TeamTag Team;
    public float Health;
    public float HitEffectInterval;
    public float2 HitEffectPosition;
}

[GenerateAuthoringComponent]
public struct Alpha : IComponentData
{
}
[GenerateAuthoringComponent]
public struct Beta : IComponentData
{
}
