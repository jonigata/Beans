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
    [SerializeField] Material rubyMaterial;
    [SerializeField] Material sapphireMaterial;
    [SerializeField] float scale;
    [SerializeField] Texture2D sdfTexture;
    [SerializeField] HeightMapHolder heightMapHolder;
    [SerializeField] Vector2 boardSize;
    [SerializeField] float totalThreshold = 0.3f;
    [SerializeField] float forceFactor = 120.0f;

    EntityManager entityManager;
    EntityArchetype alphaArchetype;
    EntityArchetype betaArchetype;
    Entity parentEntity;
    List<Entity> entities =  new List<Entity>();

    void Start() {
        // エンティティの生成
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        alphaArchetype = entityManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(LocalToParent),
            typeof(Parent),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(Force),
            typeof(Pawn),
            typeof(Alpha)
        );

        betaArchetype = entityManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(LocalToParent),
            typeof(Parent),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(Force),
            typeof(Pawn),
            typeof(Beta)
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

    public void Spawn(TeamTag team, Vector3 v) {
        Entity entity = entityManager.CreateEntity(
            team == TeamTag.Alpha ? alphaArchetype : betaArchetype);
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

        entityManager.SetComponentData(
            entity,
            new Pawn {
                Team = team,
                Health = 100.0f
            });

        Material material = null;
        if (team == TeamTag.Alpha) {
            material = rubyMaterial;
        } else {
            material = sapphireMaterial;
        }

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
