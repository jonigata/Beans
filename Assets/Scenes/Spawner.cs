using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using System.Collections.Generic;
using static Unity.Mathematics.math;

public class Spawner : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    [SerializeField] float scale;
    [SerializeField] Texture2D sdfTexture;
    [SerializeField] BoxCollider boardCollider;
    [SerializeField] HeightMapGenerator heightMapGenerator;

    EntityManager entityManager;
    EntityArchetype archetype;
    Entity parentEntity;
    List<Entity> entities =  new List<Entity>();

    void Start() {
        // エンティティの生成
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        archetype = entityManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(LocalToParent),
            typeof(Parent),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(TerrainCollision),
            typeof(Force),
            typeof(HeightMap)
        );

        parentEntity = entityManager.CreateEntity(
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale));

        entityManager.SetComponentData(
            parentEntity,
            new Translation {
                Value = transform.position
            });
        entityManager.SetComponentData(
            parentEntity,
            new Rotation {
                Value = transform.rotation
            });

        entityManager.SetComponentData(
            parentEntity,
            new Scale {
                Value = transform.lossyScale.x
            });
    }

    public void Spawn(Vector2 v) {
        Spawn(new Vector3(v.x, v.y, 0));
    }
    
    public void Spawn(Vector3 v) {
        Entity entity = entityManager.CreateEntity(archetype);
        entities.Add(entity);

        entityManager.SetComponentData(
            entity,
            new Translation {
                Value = new float3(v.x, v.y, v.z)
            });

        entityManager.SetComponentData(
           entity,
            new Scale {
                Value = scale
            });

        entityManager.SetComponentData(
            entity,
            new Parent {
                Value = parentEntity
            });

        Vector3 size = boardCollider.size;
        entityManager.SetSharedComponentData(
            entity,
            new TerrainCollision {
                sdfTexture = sdfTexture,
                size = float2(size.x, size.y),
            });

        entityManager.SetSharedComponentData(
            entity,
            new HeightMap {
                heightMap = heightMapGenerator.texture,
                size = float2(size.x, size.y),
            });

        entityManager.SetSharedComponentData(
            entity,
            new RenderMesh {
                mesh = mesh,
                material = material
            });
    }

    public void Dump() {
        Debug.Log("================");
        foreach (var entity in entities) {
            var x = entityManager.GetChunk(entity);
            Debug.Log($"x = {x.GetHashCode()}");
        }
    }
}
