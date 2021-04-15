using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchSDF : MonoBehaviour {
    [SerializeField] SpriteRenderer cursor;
    [SerializeField] SpriteRenderer cursor2;
    [SerializeField] SDFCollisionResolver sdfCollisionResolver;
    [SerializeField] SpriteTouch spriteTouch;
    [SerializeField] Spawner spawner;

    public void OnClickBoard(Vector3 uv) {
        var v = spriteTouch.GetLocalPointFromUV(uv);
        v.z = -0.2f;
        cursor.transform.localPosition = v;
        Debug.Log($"Spawn {uv}, {v}");
        spawner.Spawn(v);
    }
}
