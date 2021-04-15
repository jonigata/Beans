using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SDFCollisionResolver : MonoBehaviour {
    [SerializeField] Texture2D sdfTexture;

    public float Pick(Vector2 uv) {
        return sdfTexture.GetPixelBilinear(uv.x, uv.y).a;
    }

    public Vector2 Gradient(Vector2 uv) {
        float delta = 0.01f;
        var dis0 = Pick(new Vector2(uv.x + delta, uv.y));
        var dis1 = Pick(new Vector2(uv.x - delta, uv.y));
        var dis2 = Pick(new Vector2(uv.x, uv.y + delta));
        var dis3 = Pick(new Vector2(uv.x, uv.y - delta));
        Debug.Log($"{dis0} {dis1} {dis2} {dis3}");
        return new Vector2(dis0-dis1, dis2-dis3).normalized;
    }
}
