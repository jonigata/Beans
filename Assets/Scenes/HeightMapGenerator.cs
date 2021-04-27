using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapGenerator : MonoBehaviour
{
    [SerializeField] HeightMapHolder holder;

    public Vector2 origin = Vector2.zero;
    public Vector2 scale = Vector2.one;

    Vector2 gOrigin;
    Vector2 gScale;

    void Start() {
        UpdateTexture();
    }

        
    void Update() {
        if (origin != gOrigin || scale != gScale) {
            UpdateTexture();
        }
    }

    void UpdateTexture() {
        int width = 512;
        int height = 512;
        Color[] pixels = new Color[width*height];

        for (int y = 0 ; y < height ; y++) {
            for (int x = 0 ; x < width ; x++) {
                float n = Mathf.PerlinNoise(
                    origin.x + x * scale.x / width, 
                    origin.y + y * scale.y / height);
                pixels[y*width+x] = new Color(n, n, n, n);
            }
        }

        // テクスチャの生成
        Texture2D tex = new Texture2D(
            width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        
        tex.SetPixels(pixels);
        tex.Apply();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Sprite sprite = 
            Sprite.Create(
                tex, 
                new Rect (0, 0, width, height), 
                new Vector2 (0.5f, 0.5f), 
                100);
        sr.sprite = sprite;

        gOrigin = origin;
        gScale = scale;
        holder.texture = tex;
    }
    /*
    void MakeTextureFromTerrain(TerrainData terrainData) {
        int width = 512;
        int height = 512;
        Color[] pixels = new Color[width*height];
        
        float interval = 1.0f / 512;
        float[,] heights = terrainData.GetInterpolatedHeights(
            interval/2, interval/2, 512, 512, interval, interval);

        for (int y = 0 ; y < height ; y++) {
            for (int x = 0 ; x < width ; x++) {
                var n = heights[x,y];
                pixels[y*width+x] = new Color(n, n, n, n);
            }
        }

        // テクスチャの生成
        Texture2D tex = new Texture2D(
            width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        
        tex.SetPixels(pixels);
        tex.Apply();

        gOrigin = origin;
        gScale = scale;
        texture = tex;
    }
    */
}
