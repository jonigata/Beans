using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoardTouchHandler : MonoBehaviour {
    [SerializeField] SpriteRenderer cursor;
    [SerializeField] SpriteRenderer cursor2;
    [SerializeField] BoardTouch boardTouch;
    [SerializeField] Spawner spawner;
    [SerializeField] int countPerTouch = 1000;

    public void OnClickBoard(Vector2 uv) {
        var v = boardTouch.GetLocalPointFromUV(uv);
        v.y = -0.2f;
        if (cursor != null) { cursor.transform.localPosition = v; }
        Debug.Log($"Spawn {uv}, {v}");
        StartCoroutine(Spawn(v));
    }

    IEnumerator Spawn(Vector3 v) { 
        for(int i = 0 ; i < countPerTouch ; i++) {
            spawner.Spawn(
                v + 
                new Vector3(
                    UnityEngine.Random.Range(-0.1f, 0.1f), 
                    0,
                    UnityEngine.Random.Range(-0.1f, 0.1f)));
            // spawner.Dump();
            yield return null;
        }
    }
}
