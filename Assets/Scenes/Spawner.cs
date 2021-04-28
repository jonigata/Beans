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
    [SerializeField] HeightMapHolder heightMapHolder;
    [SerializeField] Vector2 boardSize;
    [SerializeField] float totalThreshold = 0.3f;
    [SerializeField] float forceFactor = 120.0f;

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
            typeof(Force)
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

        var boardConfig = 
            entityManager.CreateEntity(typeof(BoardConfig));
        entityManager.SetSharedComponentData(
            boardConfig,
            new BoardConfig {
                size = boardSize,
                sdfTexture = sdfTexture,
                heightMap = heightMapHolder.texture,
                heightScale = heightMapHolder.heightScale,
                neighborThreshold = totalThreshold,
                forceFactor = forceFactor,
                quadTree = new BoardConfig.QuadTree()
            });
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
