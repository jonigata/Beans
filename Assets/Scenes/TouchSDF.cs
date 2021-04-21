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
        StartCoroutine(Spawn(v));
    }

    IEnumerator Spawn(Vector3 v) { 
        for(int i = 0 ; i < 1000 ; i++) {
            spawner.Spawn(
                v + 
                new Vector3(
                    UnityEngine.Random.Range(-0.1f, 0.1f), 
                    UnityEngine.Random.Range(-0.1f, 0.1f), 0));
            // spawner.Dump();
            yield return null;
        }
    }
}
