using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[GenerateAuthoringComponent]
public struct Force : IComponentData
{
    public float3 Value;
    public static implicit operator Force(float3 value) => new Force { Value = value };
    public static implicit operator float3(Force value) => value.Value;
}
