using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapFromTerrain : MonoBehaviour
{
    [SerializeField] HeightMapHolder holder;
    [SerializeField] TerrainCollider terrainCollider;
    [SerializeField] SpriteRenderer spriteRenderer;

    void Start() {
        UpdateTexture();
    }

        
    void UpdateTexture() {
        var terrainData = terrainCollider.terrainData;
        Debug.Log(terrainCollider.bounds);

        int width = 512;
        int height = 512;
        Color[] pixels = new Color[width*height];
        
        float interval = 1.0f / 512;
        float[,] heights = terrainData.GetInterpolatedHeights(
            interval/2, interval/2, 512, 512, interval, interval);
        float heightScale = terrainCollider.bounds.extents.y * 2;

        for (int y = 0 ; y < height ; y++) {
            for (int x = 0 ; x < width ; x++) {
                var n = heights[y,x] / heightScale;
                pixels[y*width+x] = new Color(n, n, n, n);
            }
        }

        // テクスチャの生成
        Texture2D tex = new Texture2D(
            width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        
        tex.SetPixels(pixels);
        tex.Apply();

        holder.texture = tex;
        holder.heightScale = heightScale;
        Debug.Log(heightScale);

        spriteRenderer.sprite= Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}
