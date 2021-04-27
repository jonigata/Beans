using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[GenerateAuthoringComponent]
public struct RepulsionConfig : IComponentData
{
    public float totalThreshold;
    public float boardSize;
    public float forceFactor;
}
