using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class HitEffectEmitter : MonoBehaviour {
    [SerializeField] GameObject effectPrefab;

    void Update() {
        foreach (var e in HitEffectSystem.hitEffects) {
            GameObject o = Instantiate(effectPrefab, null, false);
            o.transform.position = new Vector3(e.x, 1.0f, e.y);
        }
        HitEffectSystem.hitEffects.Clear();
    }
}
