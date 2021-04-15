using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct Jitter : IComponentData
{
    public float speed;
}
